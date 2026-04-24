using System;
using System.Collections.Generic;
using Proto.TD.Sim;

namespace Proto.TD.Level.Block
{
    /// <summary>
    /// TD 용 블록 레이아웃 플래너 (순수 C#, 결정론).
    ///
    /// CityscapeGenerator 의 Block Subdivision 알고리즘을 **셀 격자** 형식으로 이식.
    /// 연속 좌표 대신 정수 셀 인덱스에서 동작하고, Unity 에셋 스폰 없이 **역할 맵만** 반환.
    /// WFC 하이브리드 파이프라인의 1차 거시 레이아웃 단계.
    ///
    /// 참조:
    /// - HR-TD-2: Xorshift <see cref="Rng"/> 만 사용
    /// - OR-P-006 (PROP-2026-04-24-001): 다양성 축을 영역 구조 레벨에서 주입
    ///   (도로 위계 / 블록 간격 / 광장 / 블록 밀도)
    /// </summary>
    public static class BlockLayoutPlanner
    {
        public enum CellRole : byte
        {
            Grass = 0,   // 기본값, 블록 내부 빈 공간
            Road = 1,    // 차도 (게임 경로)
            Sidewalk = 2,// 인도 (블록 가장자리 링, tower pad 후보)
            Building = 3,// 블록 내부 건물 lot
            Park = 4,    // Open 블록 (공원 전체, bush)
            EmptyBlock = 5, // Empty 블록 (도로만 보이는 광장 효과)
            Planter = 6  // 차도-인도 경계 녹지 띠 (sidewalk 중 road 접한 셀)
        }

        public struct Params
        {
            /// <summary>블록 1변의 기대 셀 수. 예: 7 → 40x40 에서 축당 ~5블록.</summary>
            public int TargetBlockCellSize;
            /// <summary>블록 간격 편차 비율 (0.2 = ±20%).</summary>
            public double BlockSpacingJitter;

            /// <summary>도로 위계별 폭 (셀). Local/Collector/Arterial.</summary>
            public int RoadWidthLocal;
            public int RoadWidthCollector;
            public int RoadWidthArterial;

            public double WeightLocal;
            public double WeightCollector;
            public double WeightArterial;

            /// <summary>블록 완전 비움 확률 (광장).</summary>
            public double EmptyBlockProbability;

            /// <summary>Non-empty 블록 중 Open(공원) 비율.</summary>
            public double OpenBlockFractionOfNonEmpty;
            public double MixedBlockFractionOfNonEmpty;
            // Dense = 1 - Empty - Open - Mixed

            /// <summary>Mixed / Dense 블록에서 lot 재분할 최대 깊이.</summary>
            public int MaxLotSplitDepth;
            /// <summary>재분할 최소 변 (셀).</summary>
            public int MinLotCellEdge;
            public double SplitChance;

            /// <summary>Building footprint ratio (0.7~0.9 권장).</summary>
            public double BuildingFootprint;

            /// <summary>블록 가장자리 sidewalk 두께 (셀).</summary>
            public int SidewalkInsetCells;

            /// <summary>차도 접한 인도 셀이 planter (bush) 로 승격될 확률 (0..1).</summary>
            public double PlanterProbabilityOnSidewalk;

            public static Params TdDefault() => new Params
            {
                TargetBlockCellSize = 9,
                BlockSpacingJitter = 0.20,
                RoadWidthLocal = 1,
                RoadWidthCollector = 2,
                RoadWidthArterial = 3,
                WeightLocal = 0.55,
                WeightCollector = 0.30,
                WeightArterial = 0.15,
                EmptyBlockProbability = 0.12,
                OpenBlockFractionOfNonEmpty = 0.22,
                MixedBlockFractionOfNonEmpty = 0.50,
                MaxLotSplitDepth = 3,
                MinLotCellEdge = 2,
                SplitChance = 0.70,
                BuildingFootprint = 0.78,
                SidewalkInsetCells = 2,
                PlanterProbabilityOnSidewalk = 0.35
            };
        }

        public struct Result
        {
            public bool Success;
            public string Reason;
            public CellRole[,] Roles;   // [x, y]
            public int BlockCount;
            public int EmptyBlocks;
            public int OpenBlocks;
            public int MixedBlocks;
            public int DenseBlocks;
            public int BuildingCellCount;
            public int RoadCellCount;
        }

        public static Result Plan(int width, int height, ulong seed, Params p)
        {
            if (width < 10 || height < 10)
                return Fail("grid too small (min 10x10)");

            var rng = new Rng(seed);

            // ── 축별 stops / widths ─────────────────────────────
            ComputeAxis(rng, width, p, out var xStops, out var xWidths);
            ComputeAxis(rng, height, p, out var yStops, out var yWidths);

            var roles = new CellRole[width, height];

            // 기본값 Grass (Unity default 0, 이미 CellRole.Grass).

            // ── 도로 배치 ────────────────────────────────────
            // 수평 도로: 각 yStop 을 중심으로 yWidth 폭만큼
            for (int j = 0; j < yStops.Length; j++)
                PaintHorizontalRoad(roles, width, height, yStops[j], yWidths[j]);
            // 수직 도로: 각 xStop 을 중심으로 xWidth 폭만큼
            for (int i = 0; i < xStops.Length; i++)
                PaintVerticalRoad(roles, width, height, xStops[i], xWidths[i]);

            // ── 블록 순회 ────────────────────────────────────
            int blockCount = 0, eBlocks = 0, oBlocks = 0, mBlocks = 0, dBlocks = 0;
            int buildingCount = 0;

            for (int i = 0; i < xStops.Length - 1; i++)
            {
                int xLo = xStops[i] + (xWidths[i] + 1) / 2;
                int xHi = xStops[i + 1] - (xWidths[i + 1]) / 2 - 1;
                if (xHi - xLo < 1) continue;

                for (int j = 0; j < yStops.Length - 1; j++)
                {
                    int yLo = yStops[j] + (yWidths[j] + 1) / 2;
                    int yHi = yStops[j + 1] - (yWidths[j + 1]) / 2 - 1;
                    if (yHi - yLo < 1) continue;

                    blockCount++;
                    var density = PickDensity(rng, p);

                    if (density == BlockDensity.Empty)
                    {
                        eBlocks++;
                        // 블록 내부를 EmptyBlock 으로 마킹 (grass 와 구분 — 광장/사거리 효과)
                        for (int yy = yLo; yy <= yHi; yy++)
                            for (int xx = xLo; xx <= xHi; xx++)
                                if (roles[xx, yy] == CellRole.Grass) roles[xx, yy] = CellRole.EmptyBlock;
                        continue;
                    }

                    // 블록 가장자리는 Sidewalk
                    int inset = Math.Max(0, p.SidewalkInsetCells);
                    PaintBlockSidewalk(roles, xLo, yLo, xHi, yHi, inset);

                    int lotXLo = xLo + inset;
                    int lotYLo = yLo + inset;
                    int lotXHi = xHi - inset;
                    int lotYHi = yHi - inset;

                    if (density == BlockDensity.Open || lotXHi - lotXLo < 1 || lotYHi - lotYLo < 1)
                    {
                        oBlocks++;
                        // 공원: lot 영역 전부 Park
                        for (int yy = Math.Max(yLo, lotYLo); yy <= Math.Min(yHi, lotYHi); yy++)
                            for (int xx = Math.Max(xLo, lotXLo); xx <= Math.Min(xHi, lotXHi); xx++)
                                if (roles[xx, yy] == CellRole.Grass) roles[xx, yy] = CellRole.Park;
                        continue;
                    }

                    if (density == BlockDensity.Mixed) mBlocks++;
                    else dBlocks++;

                    int maxDepth = density == BlockDensity.Dense ? p.MaxLotSplitDepth + 1 : p.MaxLotSplitDepth;
                    double splitChance = density == BlockDensity.Dense ? Math.Min(1.0, p.SplitChance + 0.2) : p.SplitChance;

                    SubdivideAndBuild(roles, rng, lotXLo, lotYLo, lotXHi, lotYHi, 0, maxDepth, splitChance, p, ref buildingCount);
                }
            }

            // ── Planter strip: 인도 중 도로 접한 셀을 확률적으로 Planter 로 승격 ──
            // 도로 옆 녹지 띠 효과. 차도-인도 시각 구분 + 도메인 룰 (타워 배치 공간은 인도).
            if (p.PlanterProbabilityOnSidewalk > 0.0)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (roles[x, y] != CellRole.Sidewalk) continue;
                        if (!IsAdjacentToRole(roles, width, height, x, y, CellRole.Road)) continue;
                        if (rng.NextDouble() < p.PlanterProbabilityOnSidewalk)
                            roles[x, y] = CellRole.Planter;
                    }
                }
            }

            // Road 셀 수 최종 집계 (path override 없이 순수 블록 레이아웃 기준)
            int roadCount = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (roles[x, y] == CellRole.Road) roadCount++;

            return new Result
            {
                Success = true,
                Reason = "",
                Roles = roles,
                BlockCount = blockCount,
                EmptyBlocks = eBlocks,
                OpenBlocks = oBlocks,
                MixedBlocks = mBlocks,
                DenseBlocks = dBlocks,
                BuildingCellCount = buildingCount,
                RoadCellCount = roadCount
            };
        }

        // ─────────────────────────────────────────────────────────
        //  Axis
        // ─────────────────────────────────────────────────────────

        private static void ComputeAxis(Rng rng, int length, Params p, out int[] stops, out int[] widths)
        {
            // 경계(0, length-1) 는 void border 전용이므로 도로 stop 에 포함하지 않는다.
            // 안쪽 ring 에서 1 번째 / 마지막 도로를 painter 가 그린다 → 외곽 도로가 "perimeter avenue" 역할.
            var sList = new List<int>();
            var wList = new List<int>();

            int first = 1;
            sList.Add(first);
            wList.Add(SampleRoadWidth(rng, p));

            int cursor = first;
            int last = length - 2;
            while (true)
            {
                double jitter = 1.0 + (rng.NextDouble() - 0.5) * 2.0 * p.BlockSpacingJitter;
                int step = (int)Math.Round(Math.Max(p.TargetBlockCellSize * 0.4, p.TargetBlockCellSize * jitter));
                if (step < 2) step = 2;
                int next = cursor + step;
                if (next >= last - (int)(p.TargetBlockCellSize * 0.3)) break;
                cursor = next;
                sList.Add(cursor);
                wList.Add(SampleRoadWidth(rng, p));
            }

            if (sList[sList.Count - 1] != last)
            {
                sList.Add(last);
                wList.Add(SampleRoadWidth(rng, p));
            }

            stops = sList.ToArray();
            widths = wList.ToArray();
        }

        private static int SampleRoadWidth(Rng rng, Params p)
        {
            double wL = Math.Max(0.0, p.WeightLocal);
            double wC = Math.Max(0.0, p.WeightCollector);
            double wA = Math.Max(0.0, p.WeightArterial);
            double sum = wL + wC + wA;
            if (sum <= 0.0) return p.RoadWidthLocal;
            double r = rng.NextDouble() * sum;
            if (r < wL) return p.RoadWidthLocal;
            if (r < wL + wC) return p.RoadWidthCollector;
            return p.RoadWidthArterial;
        }

        // ─────────────────────────────────────────────────────────
        //  Painting
        // ─────────────────────────────────────────────────────────

        private static void PaintHorizontalRoad(CellRole[,] roles, int w, int h, int centerY, int widthCells)
        {
            int half = widthCells / 2;
            int yLo = Math.Max(1, centerY - half);            // skip border row 0
            int yHi = Math.Min(h - 2, centerY + (widthCells - 1 - half));   // skip border row h-1
            if (yLo > yHi) return;
            for (int y = yLo; y <= yHi; y++)
                for (int x = 1; x < w - 1; x++)
                    roles[x, y] = CellRole.Road;
        }

        private static void PaintVerticalRoad(CellRole[,] roles, int w, int h, int centerX, int widthCells)
        {
            int half = widthCells / 2;
            int xLo = Math.Max(1, centerX - half);
            int xHi = Math.Min(w - 2, centerX + (widthCells - 1 - half));
            if (xLo > xHi) return;
            for (int x = xLo; x <= xHi; x++)
                for (int y = 1; y < h - 1; y++)
                    roles[x, y] = CellRole.Road;
        }

        private static void PaintBlockSidewalk(CellRole[,] roles, int xLo, int yLo, int xHi, int yHi, int inset)
        {
            if (inset <= 0) return;
            for (int y = yLo; y <= yHi; y++)
            {
                for (int x = xLo; x <= xHi; x++)
                {
                    bool edge = (x < xLo + inset) || (x > xHi - inset) || (y < yLo + inset) || (y > yHi - inset);
                    if (edge && roles[x, y] == CellRole.Grass)
                        roles[x, y] = CellRole.Sidewalk;
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        //  Block density
        // ─────────────────────────────────────────────────────────

        private enum BlockDensity { Empty, Open, Mixed, Dense }

        private static BlockDensity PickDensity(Rng rng, Params p)
        {
            double r = rng.NextDouble();
            double e = p.EmptyBlockProbability;
            double rest = Math.Max(0.0, 1.0 - e);
            double cO = e + rest * p.OpenBlockFractionOfNonEmpty;
            double cM = e + rest * (p.OpenBlockFractionOfNonEmpty + p.MixedBlockFractionOfNonEmpty);
            if (r < e) return BlockDensity.Empty;
            if (r < cO) return BlockDensity.Open;
            if (r < cM) return BlockDensity.Mixed;
            return BlockDensity.Dense;
        }

        // ─────────────────────────────────────────────────────────
        //  Lot subdivision
        // ─────────────────────────────────────────────────────────

        private static void SubdivideAndBuild(
            CellRole[,] roles, Rng rng,
            int xLo, int yLo, int xHi, int yHi,
            int depth, int maxDepth, double splitChance, Params p,
            ref int buildingCount)
        {
            int w = xHi - xLo + 1;
            int h = yHi - yLo + 1;
            bool canSplit =
                depth < maxDepth &&
                w > p.MinLotCellEdge * 2 &&
                h > p.MinLotCellEdge * 2 &&
                rng.NextDouble() < splitChance;

            if (!canSplit)
            {
                BuildLot(roles, rng, xLo, yLo, xHi, yHi, p, ref buildingCount);
                return;
            }

            bool splitVertical = w >= h
                ? rng.NextDouble() < 0.7
                : rng.NextDouble() < 0.3;

            if (splitVertical)
            {
                double t = Lerp(0.30, 0.70, rng.NextDouble());
                // xMid 은 절대 셀 인덱스 (xLo + offset). Left: [xLo, xMid-1], Right: [xMid, xHi].
                int xMid = xLo + Math.Max(1, Math.Min(w - 1, (int)Math.Round(w * t)));
                if (xMid <= xLo) xMid = xLo + 1;
                if (xMid > xHi) xMid = xHi;
                SubdivideAndBuild(roles, rng, xLo, yLo, xMid - 1, yHi, depth + 1, maxDepth, splitChance, p, ref buildingCount);
                SubdivideAndBuild(roles, rng, xMid, yLo, xHi, yHi, depth + 1, maxDepth, splitChance, p, ref buildingCount);
            }
            else
            {
                double t = Lerp(0.30, 0.70, rng.NextDouble());
                int yMid = yLo + Math.Max(1, Math.Min(h - 1, (int)Math.Round(h * t)));
                if (yMid <= yLo) yMid = yLo + 1;
                if (yMid > yHi) yMid = yHi;
                SubdivideAndBuild(roles, rng, xLo, yLo, xHi, yMid - 1, depth + 1, maxDepth, splitChance, p, ref buildingCount);
                SubdivideAndBuild(roles, rng, xLo, yMid, xHi, yHi, depth + 1, maxDepth, splitChance, p, ref buildingCount);
            }
        }

        private static void BuildLot(CellRole[,] roles, Rng rng, int xLo, int yLo, int xHi, int yHi, Params p, ref int buildingCount)
        {
            int w = xHi - xLo + 1;
            int h = yHi - yLo + 1;

            // Footprint ratio 로 lot 중앙에 빌딩 배치. 가장자리는 grass 로 남김.
            int bw = Math.Max(1, (int)Math.Round(w * p.BuildingFootprint));
            int bh = Math.Max(1, (int)Math.Round(h * p.BuildingFootprint));
            int bxOff = (w - bw) / 2;
            int byOff = (h - bh) / 2;

            int bxLo = xLo + bxOff;
            int byLo = yLo + byOff;
            int bxHi = bxLo + bw - 1;
            int byHi = byLo + bh - 1;

            for (int y = byLo; y <= byHi; y++)
            {
                for (int x = bxLo; x <= bxHi; x++)
                {
                    if (roles[x, y] == CellRole.Grass || roles[x, y] == CellRole.Sidewalk)
                    {
                        roles[x, y] = CellRole.Building;
                        buildingCount++;
                    }
                }
            }
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;

        private static bool IsAdjacentToRole(CellRole[,] roles, int w, int h, int x, int y, CellRole target)
        {
            if (x + 1 < w && roles[x + 1, y] == target) return true;
            if (x - 1 >= 0 && roles[x - 1, y] == target) return true;
            if (y + 1 < h && roles[x, y + 1] == target) return true;
            if (y - 1 >= 0 && roles[x, y - 1] == target) return true;
            return false;
        }

        private static Result Fail(string reason) => new Result { Success = false, Reason = reason, Roles = null };
    }
}
