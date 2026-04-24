using System;
using System.Collections.Generic;
using Proto.Stage.PCG.Wfc;

namespace Proto.TD.Level.Wfc
{
    /// <summary>
    /// TD 레벨 WFC 어댑터. spec (크기/seed/lane 수/power/core) → WFC 입력 구성 →
    /// 솔버 호출 → 결과를 level.schema.json 문자열로 직렬화.
    ///
    /// 공유 좌표계: (x=0 y=0) 이 grid 좌하단, y 증가가 북쪽.
    /// A* 및 WFC 내부 좌표와 일치한다 (Direction.N = y+1 방향으로 이동).
    ///
    /// 참조: HR-TD-2 (Xorshift seed), HR-TD-5 (고정 경로), HR-TD-10 (JSON 경로).
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

            // ── Path generation ─────────────────────────────────
            var pathRes = TdPathGenerator.Generate(
                width: gs, height: gs,
                seed: spec.Seed,
                coreBase: spec.CoreBase,
                laneCount: spec.LaneCount,
                borderInset: 1);
            if (!pathRes.Success)
                return Fail("path generation failed: " + pathRes.Reason);

            // ── Mark path cells ─────────────────────────────────
            var pathUnion = new HashSet<(int x, int y)>();
            foreach (var lane in pathRes.Paths)
                foreach (var cell in lane)
                    pathUnion.Add((cell.X, cell.Y));

            // Core neighbors that aren't in a path must still be forced-road (toward core).
            var coreNeighbors = new[]
            {
                (x: spec.CoreBase.X, y: spec.CoreBase.Y + 1, dir: Direction.S), // N of core; this neighbor's S faces core
                (x: spec.CoreBase.X + 1, y: spec.CoreBase.Y, dir: Direction.W),
                (x: spec.CoreBase.X, y: spec.CoreBase.Y - 1, dir: Direction.N),
                (x: spec.CoreBase.X - 1, y: spec.CoreBase.Y, dir: Direction.E),
            };
            foreach (var (x, y, _) in coreNeighbors)
            {
                if (InBounds(x, y, gs, gs))
                    pathUnion.Add((x, y));
            }

            // ── Fixed cells ─────────────────────────────────────
            var fixedCells = new List<WfcFixedCell>();

            // Power pads
            if (spec.PowerSources != null)
            {
                foreach (var p in spec.PowerSources)
                {
                    if (!InBounds(p.X, p.Y, gs, gs)) continue;
                    if (pathUnion.Contains((p.X, p.Y)))
                        return Fail($"power source overlaps a path cell at ({p.X},{p.Y})");
                    if (p.Equals(spec.CoreBase))
                        return Fail($"power source coincides with coreBase at ({p.X},{p.Y})");
                    fixedCells.Add(new WfcFixedCell(p.X, p.Y, TdDefaultTileSet.T_PowerPad));
                }
            }

            // Core base
            if (!InBounds(spec.CoreBase.X, spec.CoreBase.Y, gs, gs))
                return Fail("coreBase out of bounds");
            fixedCells.Add(new WfcFixedCell(spec.CoreBase.X, spec.CoreBase.Y, TdDefaultTileSet.T_CoreBase));

            // Classify each path cell into a concrete tile ID (considering core-base as path-like).
            // Build a boolean "road neighborhood" from pathUnion plus CoreBase.
            Func<int, int, bool> isRoadLike = (x, y) =>
            {
                if (x == spec.CoreBase.X && y == spec.CoreBase.Y) return true;
                return pathUnion.Contains((x, y));
            };

            foreach (var (x, y) in pathUnion)
            {
                if (x == spec.CoreBase.X && y == spec.CoreBase.Y) continue; // core handled above
                // Skip if this cell is a power source (shouldn't happen due to overlap check).
                bool powerOverlap = false;
                if (spec.PowerSources != null)
                {
                    foreach (var p in spec.PowerSources)
                    {
                        if (p.X == x && p.Y == y) { powerOverlap = true; break; }
                    }
                }
                if (powerOverlap) continue;

                bool n = isRoadLike(x, y + 1);
                bool e = isRoadLike(x + 1, y);
                bool s = isRoadLike(x, y - 1);
                bool w = isRoadLike(x - 1, y);
                string tileId = ClassifyRoadTile(n, e, s, w);
                fixedCells.Add(new WfcFixedCell(x, y, tileId));
            }

            // ── Solver call ─────────────────────────────────────
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
                    ContradictionY = wfc.ContradictionCellY
                };
            }

            // ── Serialize to JSON ───────────────────────────────
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
                ContradictionY = -1
            };
        }

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

            // tiles[y][x]
            jw.Key("tiles");
            jw.BeginArray();
            for (int y = 0; y < h; y++)
            {
                jw.BeginArray();
                for (int x = 0; x < w; x++)
                {
                    jw.Value(wfc.Grid[x, y]);
                }
                jw.EndArray();
            }
            jw.EndArray();

            // paths
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

            // power sources
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
            jw.Key("generator"); jw.Value("stage-pcg-wfc");
            jw.Key("generatorVersion"); jw.Value("1.0.0");
            jw.Key("tileSetId"); jw.Value("TD_Default");
            jw.Key("iterations"); jw.Value(wfc.Iterations);
            jw.EndObject();

            jw.EndObject();
            return jw.ToString();
        }

        /// <summary>
        /// 4방향 road-like 이웃 플래그로부터 구체 타일 ID 를 고른다.
        /// Default 타일셋의 회전 파생물 ID (`road-corner-ne@rN`, `road-t-n@rN`, `road-end-n@rN`) 반환.
        /// 회전 규칙은 <see cref="WfcTileSetData.BuildWithRotations"/> 와 일치.
        /// </summary>
        private static string ClassifyRoadTile(bool n, bool e, bool s, bool w)
        {
            int count = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);
            switch (count)
            {
                case 0:
                    // shouldn't happen for a path cell; fallback to straight-ns
                    return TdDefaultTileSet.T_RoadStraightNS;
                case 1:
                    // road-end-<openDir> — name is by the ROAD direction
                    if (n) return TdDefaultTileSet.T_RoadEndN;
                    if (e) return TdDefaultTileSet.T_RoadEndN + "@r1";
                    if (s) return TdDefaultTileSet.T_RoadEndN + "@r2";
                    return TdDefaultTileSet.T_RoadEndN + "@r3"; // w
                case 2:
                    if (n && s) return TdDefaultTileSet.T_RoadStraightNS;
                    if (e && w) return TdDefaultTileSet.T_RoadStraightEW;
                    if (n && e) return TdDefaultTileSet.T_RoadCornerNE;
                    if (e && s) return TdDefaultTileSet.T_RoadCornerNE + "@r1";
                    if (s && w) return TdDefaultTileSet.T_RoadCornerNE + "@r2";
                    return TdDefaultTileSet.T_RoadCornerNE + "@r3"; // n,w
                case 3:
                    // road-t-<openDir>: T_N has open=N, so if open is N, use T_RoadT_N (rotation 0).
                    // Rotation matrix (verified): T_N rotated r steps CW → open side rotates r steps CW.
                    // open N → r0, open E → r1, open S → r2, open W → r3.
                    if (!n) return TdDefaultTileSet.T_RoadT_N;
                    if (!e) return TdDefaultTileSet.T_RoadT_N + "@r1";
                    if (!s) return TdDefaultTileSet.T_RoadT_N + "@r2";
                    return TdDefaultTileSet.T_RoadT_N + "@r3"; // !w
                case 4:
                default:
                    return TdDefaultTileSet.T_RoadCross;
            }
        }

        private static bool InBounds(int x, int y, int w, int h) => x >= 0 && x < w && y >= 0 && y < h;

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
            ContradictionY = -1
        };
    }
}
