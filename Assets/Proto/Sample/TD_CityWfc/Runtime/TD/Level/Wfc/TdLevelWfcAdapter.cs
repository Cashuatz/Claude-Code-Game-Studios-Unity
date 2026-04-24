using System;
using System.Collections.Generic;
using Proto.Stage.PCG.Wfc;
using Proto.TD.Level.Block;

namespace Proto.TD.Level.Wfc
{
    /// <summary>
    /// TD 레벨 하이브리드 어댑터 (v2 기본).
    ///
    /// 파이프라인:
    ///   1) BlockLayoutPlanner 로 거시 블록 격자 생성 (도로 / 블록 / 건물 lot / 공원 / 광장)
    ///   2) 블록 role 을 cost hint 로 TdPathGenerator A* 경로 생성 (road 선호)
    ///   3) 경로 셀 / CoreBase / CoreBase 주변 / PowerPad 로 블록 role 오버라이드
    ///   4) 최종 셀별 role → 타일 ID 매핑 (road 는 N/E/S/W 기반 분류)
    ///   5) 모든 셀을 WFC FixedCell 로 주입
    ///   6) WFC 는 인접 호환을 최종 검증 (iterations 는 거의 0)
    ///   7) level.schema.json 직렬화
    ///
    /// 설계 근거:
    /// - PROP-2026-04-24-001 / OR-P-006: "시가지 도메인 다양성은 영역 구조 레벨"
    ///   단독 WFC v1 은 로컬 인접 규칙만으로 블록 격자를 창발시키지 못해 OR-08 REJECTED.
    ///   하이브리드는 블록 구조를 1차 결정하고 WFC 는 인접 검증자로 남긴다.
    /// - HR-TD-2: 모든 RNG 는 Xorshift64 <see cref="Proto.TD.Sim.Rng"/>.
    /// - HR-TD-5: 경로는 사전 정의(고정). A* 결과 waypoint 를 경로로 기록.
    /// - 좌표: (x=0 y=0) grid 좌하단, y 증가 = 북 (Unity world z+).
    ///
    /// 참조: [`design/proto-modules/stage-pcg-wfc.md`](../../../../../../design/proto-modules/stage-pcg-wfc.md)
    /// </summary>
    public static class TdLevelWfcAdapter
    {
        public struct Spec
        {
            public int GridSize;                          // 20 / 40 / 80
            public ulong Seed;
            public int LaneCount;                         // Q6: 1..4
            public AStarGrid.Cell CoreBase;               // must be interior
            public IReadOnlyList<AStarGrid.Cell> PowerSources;
            public WfcTileSetData TileSet;                // null → TdDefaultTileSet.Build()
            public double LinkBudget;                     // default: GridSize * 2.5
            public string LevelId;                        // null → auto
            public BlockLayoutPlanner.Params? BlockParams; // null → BlockLayoutPlanner.Params.TdDefault()
        }

        public struct Result
        {
            public bool Success;
            public WfcStatus WfcStatus;
            public string Reason;
            public string[,] Grid;
            public List<List<AStarGrid.Cell>> Paths;
            public int Iterations;
            public string LevelJson;
            public int ContradictionX;
            public int ContradictionY;
            public BlockLayoutStats BlockStats;
        }

        public struct BlockLayoutStats
        {
            public int Blocks;
            public int Empty;
            public int Open;
            public int Mixed;
            public int Dense;
            public int BuildingCells;
            public int RoadCells;
        }

        public static Result Build(Spec spec)
        {
            if (spec.GridSize < 10)
                return Fail("grid size must be ≥ 10");
            if (spec.LaneCount < 1) spec.LaneCount = 1;
            if (spec.LaneCount > 4) spec.LaneCount = 4;
            if (spec.LinkBudget <= 0) spec.LinkBudget = spec.GridSize * 2.5;
            if (string.IsNullOrEmpty(spec.LevelId))
                spec.LevelId = $"level-wfc-{spec.GridSize}-0x{spec.Seed:x}";

            int gs = spec.GridSize;
            var tileSet = spec.TileSet ?? TdDefaultTileSet.Build();
            var blockParams = spec.BlockParams ?? BlockLayoutPlanner.Params.TdDefault();

            // ── 1. Block layout ─────────────────────────────────
            var blockResult = BlockLayoutPlanner.Plan(gs, gs, spec.Seed, blockParams);
            if (!blockResult.Success)
                return Fail("block layout failed: " + blockResult.Reason);
            var roles = blockResult.Roles;

            // Power source cell 은 open socket 타일이므로, 경로가 이를 가로지르면
            // 주변 road 와 socket 불호환이 발생한다. A* 가 power 셀을 회피하도록 차단.
            var powerBlock = new HashSet<long>();
            if (spec.PowerSources != null)
            {
                foreach (var p in spec.PowerSources)
                    powerBlock.Add(((long)p.X << 32) | (uint)p.Y);
            }

            // ── 2. Path generation with road-preferring cost ────
            var pathRes = TdPathGenerator.Generate(
                width: gs, height: gs,
                seed: spec.Seed ^ 0x9E3779B97F4A7C15UL, // 경로용 sub-seed (블록과 섞임)
                coreBase: spec.CoreBase,
                laneCount: spec.LaneCount,
                borderInset: 1,
                cellCostHint: (x, y) =>
                {
                    if (powerBlock.Contains(((long)x << 32) | (uint)y)) return int.MaxValue;
                    return RoleToPathCost(roles[x, y]);
                });
            if (!pathRes.Success)
                return Fail("path generation failed: " + pathRes.Reason);

            // ── 3. Role overrides (paths + core + power) ────────
            var pathUnion = new HashSet<(int x, int y)>();
            foreach (var lane in pathRes.Paths)
                foreach (var cell in lane)
                    pathUnion.Add((cell.X, cell.Y));

            foreach (var (x, y) in pathUnion)
                if (roles[x, y] != BlockLayoutPlanner.CellRole.Road)
                    roles[x, y] = BlockLayoutPlanner.CellRole.Road;

            // CoreBase 주변 4 neighbors 도 road 로 (아직 road 가 아니라면).
            var coreNeighbors = new[]
            {
                (spec.CoreBase.X, spec.CoreBase.Y + 1),
                (spec.CoreBase.X + 1, spec.CoreBase.Y),
                (spec.CoreBase.X, spec.CoreBase.Y - 1),
                (spec.CoreBase.X - 1, spec.CoreBase.Y),
            };
            foreach (var (nx, ny) in coreNeighbors)
            {
                if (InBounds(nx, ny, gs, gs))
                {
                    roles[nx, ny] = BlockLayoutPlanner.CellRole.Road;
                    pathUnion.Add((nx, ny));
                }
            }

            // Power sources / core base validation + role demotion (Grass)
            // PowerPad 는 open socket → 주변 road 와 호환 안 됨. role 을 Grass 로 덮어
            // 이웃 classify 가 road 로 연결하지 않도록 한다.
            if (spec.PowerSources != null)
            {
                foreach (var p in spec.PowerSources)
                {
                    if (!InBounds(p.X, p.Y, gs, gs))
                        return Fail($"power source out of bounds ({p.X},{p.Y})");
                    if (p.Equals(spec.CoreBase))
                        return Fail("power source coincides with coreBase");
                    roles[p.X, p.Y] = BlockLayoutPlanner.CellRole.Grass;
                }
            }
            if (!InBounds(spec.CoreBase.X, spec.CoreBase.Y, gs, gs))
                return Fail("coreBase out of bounds");

            // ── 4. Build FixedCells (모든 non-border 셀 고정) ────
            var fixedCells = new List<WfcFixedCell>(gs * gs);

            bool isCore(int x, int y) => x == spec.CoreBase.X && y == spec.CoreBase.Y;
            bool isPower(int x, int y)
            {
                if (spec.PowerSources == null) return false;
                foreach (var p in spec.PowerSources)
                    if (p.X == x && p.Y == y) return true;
                return false;
            }
            bool isRoadLike(int x, int y)
            {
                if (!InBounds(x, y, gs, gs)) return false;
                // 경계 셀은 WFC 가 void 로 덮는다 → road 로 취급하면 road socket 이
                // void 이웃과 불호환 → contradiction. 항상 false 리턴.
                if (x == 0 || y == 0 || x == gs - 1 || y == gs - 1) return false;
                if (isCore(x, y)) return true;
                return roles[x, y] == BlockLayoutPlanner.CellRole.Road;
            }

            for (int y = 1; y < gs - 1; y++)
            {
                for (int x = 1; x < gs - 1; x++)
                {
                    // Core / Power 오버라이드 먼저
                    if (isCore(x, y))
                    {
                        fixedCells.Add(new WfcFixedCell(x, y, TdDefaultTileSet.T_CoreBase));
                        continue;
                    }
                    if (isPower(x, y))
                    {
                        fixedCells.Add(new WfcFixedCell(x, y, TdDefaultTileSet.T_PowerPad));
                        // power 셀은 도로 이웃 요구 → 주변 구조가 이미 road 쪽이면 OK.
                        // 도시블록 한복판에 뜰 수 있으므로 role 을 road 로 승격 (주변 접근성 보장은 여기서 안 함).
                        continue;
                    }

                    string tile = RoleToTile(roles[x, y], x, y, isRoadLike);
                    fixedCells.Add(new WfcFixedCell(x, y, tile));
                }
            }

            // ── 5. WFC Solve (어댑터 검증 역할) ─────────────────
            var input = new WfcInput
            {
                Width = gs,
                Height = gs,
                Seed = spec.Seed,
                TileSet = tileSet,
                FixedCells = fixedCells
            };
            var wfc = WfcSolver.Solve(input);

            if (wfc.Status != WfcStatus.Ok)
            {
                return new Result
                {
                    Success = false,
                    WfcStatus = wfc.Status,
                    Reason = wfc.Reason,
                    Grid = wfc.Grid,
                    Paths = pathRes.Paths,
                    Iterations = wfc.Iterations,
                    LevelJson = null,
                    ContradictionX = wfc.ContradictionCellX,
                    ContradictionY = wfc.ContradictionCellY,
                    BlockStats = ToStats(blockResult)
                };
            }

            // ── 6. Serialize ────────────────────────────────────
            string json = SerializeLevel(spec, wfc, pathRes.Paths, tileSet);

            return new Result
            {
                Success = true,
                WfcStatus = WfcStatus.Ok,
                Reason = "",
                Grid = wfc.Grid,
                Paths = pathRes.Paths,
                Iterations = wfc.Iterations,
                LevelJson = json,
                ContradictionX = -1,
                ContradictionY = -1,
                BlockStats = ToStats(blockResult)
            };
        }

        // ─────────────────────────────────────────────────────────
        //  Role → path cost
        // ─────────────────────────────────────────────────────────

        private static int RoleToPathCost(BlockLayoutPlanner.CellRole role)
        {
            // 모든 role 은 통과 가능 (Building 은 매우 비싸게). 스폰 셀이 건물 블록
            // 한복판이라도 A* 가 빠져나갈 수 있게 int.MaxValue 는 쓰지 않는다.
            // 경로가 건물을 뚫고 지나면 그 셀은 이후 Road 로 승격된다 (건물 훼손 허용).
            switch (role)
            {
                case BlockLayoutPlanner.CellRole.Road: return 1;
                case BlockLayoutPlanner.CellRole.Sidewalk: return 2;
                case BlockLayoutPlanner.CellRole.Planter: return 3;
                case BlockLayoutPlanner.CellRole.EmptyBlock: return 3;
                case BlockLayoutPlanner.CellRole.Park: return 4;
                case BlockLayoutPlanner.CellRole.Grass: return 5;
                case BlockLayoutPlanner.CellRole.Building: return 40;
                default: return 5;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Role + neighborhood → tile ID
        // ─────────────────────────────────────────────────────────

        private static string RoleToTile(
            BlockLayoutPlanner.CellRole role, int x, int y,
            Func<int, int, bool> isRoadLike)
        {
            switch (role)
            {
                case BlockLayoutPlanner.CellRole.Road:
                    return ClassifyRoadTile(
                        n: isRoadLike(x, y + 1),
                        e: isRoadLike(x + 1, y),
                        s: isRoadLike(x, y - 1),
                        w: isRoadLike(x - 1, y));
                case BlockLayoutPlanner.CellRole.Building:
                    return TdDefaultTileSet.T_BuildingSmall;
                case BlockLayoutPlanner.CellRole.Park:
                    return TdDefaultTileSet.T_GrassBush;
                case BlockLayoutPlanner.CellRole.Sidewalk:
                    return TdDefaultTileSet.T_Sidewalk;
                case BlockLayoutPlanner.CellRole.Planter:
                    return TdDefaultTileSet.T_Planter;
                case BlockLayoutPlanner.CellRole.EmptyBlock:
                case BlockLayoutPlanner.CellRole.Grass:
                default:
                    return TdDefaultTileSet.T_GrassFlat;
            }
        }

        /// <summary>4방향 road-like 이웃 플래그로 구체 road 타일 ID 결정.</summary>
        private static string ClassifyRoadTile(bool n, bool e, bool s, bool w)
        {
            int count = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);
            switch (count)
            {
                case 0:
                    return TdDefaultTileSet.T_RoadStraightNS;
                case 1:
                    if (n) return TdDefaultTileSet.T_RoadEndN;
                    if (e) return TdDefaultTileSet.T_RoadEndN + "@r1";
                    if (s) return TdDefaultTileSet.T_RoadEndN + "@r2";
                    return TdDefaultTileSet.T_RoadEndN + "@r3";
                case 2:
                    if (n && s) return TdDefaultTileSet.T_RoadStraightNS;
                    if (e && w) return TdDefaultTileSet.T_RoadStraightEW;
                    if (n && e) return TdDefaultTileSet.T_RoadCornerNE;
                    if (e && s) return TdDefaultTileSet.T_RoadCornerNE + "@r1";
                    if (s && w) return TdDefaultTileSet.T_RoadCornerNE + "@r2";
                    return TdDefaultTileSet.T_RoadCornerNE + "@r3"; // n,w
                case 3:
                    if (!n) return TdDefaultTileSet.T_RoadT_N;
                    if (!e) return TdDefaultTileSet.T_RoadT_N + "@r1";
                    if (!s) return TdDefaultTileSet.T_RoadT_N + "@r2";
                    return TdDefaultTileSet.T_RoadT_N + "@r3";
                case 4:
                default:
                    return TdDefaultTileSet.T_RoadCross;
            }
        }

        // ─────────────────────────────────────────────────────────
        //  JSON serialization
        // ─────────────────────────────────────────────────────────

        private static string SerializeLevel(
            Spec spec, WfcOutput wfc,
            List<List<AStarGrid.Cell>> paths,
            WfcTileSetData tileSet)
        {
            int w = wfc.Grid.GetLength(0);
            int h = wfc.Grid.GetLength(1);
            var jw = new JsonWriter(pretty: true);
            jw.BeginObject();
            jw.Key("schemaVersion"); jw.Value(1);
            jw.Key("levelId"); jw.Value(spec.LevelId);

            jw.Key("gridSize");
            jw.BeginObject();
            jw.Key("w"); jw.Value(w);
            jw.Key("h"); jw.Value(h);
            jw.EndObject();

            jw.Key("cellSize"); jw.Value(1.0);
            jw.Key("seed"); jw.Value("0x" + spec.Seed.ToString("x"));

            jw.Key("tiles");
            jw.BeginArray();
            for (int y = 0; y < h; y++)
            {
                jw.BeginArray();
                for (int x = 0; x < w; x++)
                    jw.Value(wfc.Grid[x, y]);
                jw.EndArray();
            }
            jw.EndArray();

            jw.Key("paths");
            jw.BeginArray();
            foreach (var lane in paths)
            {
                jw.BeginArray();
                foreach (var c in lane)
                {
                    jw.BeginObject();
                    jw.Key("x"); jw.Value(c.X);
                    jw.Key("y"); jw.Value(c.Y);
                    jw.EndObject();
                }
                jw.EndArray();
            }
            jw.EndArray();

            jw.Key("powerSources");
            jw.BeginArray();
            if (spec.PowerSources != null)
            {
                foreach (var p in spec.PowerSources)
                {
                    jw.BeginObject();
                    jw.Key("x"); jw.Value(p.X);
                    jw.Key("y"); jw.Value(p.Y);
                    jw.EndObject();
                }
            }
            jw.EndArray();

            jw.Key("coreBase");
            jw.BeginObject();
            jw.Key("x"); jw.Value(spec.CoreBase.X);
            jw.Key("y"); jw.Value(spec.CoreBase.Y);
            jw.EndObject();

            jw.Key("linkBudget"); jw.Value(spec.LinkBudget);

            jw.Key("meta");
            jw.BeginObject();
            jw.Key("generator"); jw.Value("stage-pcg-wfc-hybrid");
            jw.Key("generatorVersion"); jw.Value("2.0.0");
            jw.Key("tileSetId"); jw.Value("TD_Default");
            jw.Key("iterations"); jw.Value(wfc.Iterations);
            jw.EndObject();

            jw.EndObject();
            return jw.ToString();
        }

        // ─────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────

        private static bool InBounds(int x, int y, int w, int h) => x >= 0 && x < w && y >= 0 && y < h;

        private static BlockLayoutStats ToStats(BlockLayoutPlanner.Result r) => new BlockLayoutStats
        {
            Blocks = r.BlockCount,
            Empty = r.EmptyBlocks,
            Open = r.OpenBlocks,
            Mixed = r.MixedBlocks,
            Dense = r.DenseBlocks,
            BuildingCells = r.BuildingCellCount,
            RoadCells = r.RoadCellCount
        };

        private static Result Fail(string reason) => new Result
        {
            Success = false,
            WfcStatus = WfcStatus.InvalidInput,
            Reason = reason,
            Grid = null,
            Paths = null,
            Iterations = 0,
            LevelJson = null,
            ContradictionX = -1,
            ContradictionY = -1,
            BlockStats = default
        };
    }
}
