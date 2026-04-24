using System;
using System.Collections.Generic;
using Proto.Shared;

namespace Proto.TD.Level
{
    /// <summary>
    /// TD 고정 경로 생성 (HR-TD-5).
    /// 가장자리에서 시작점을 seed 로 고르고 CoreBase 까지 A* 로 길을 뽑는다.
    /// 여러 lane 요청 시 이전 경로 셀에 높은 비용을 주어 분기를 유도한다.
    /// 결정론: 동일 (seed, width, height, coreBase, laneCount) → 동일 결과.
    /// </summary>
    public static class TdPathGenerator
    {
        public struct Result
        {
            public bool Success;
            public string Reason;
            public List<List<AStarGrid.Cell>> Paths;
        }

        /// <param name="laneCount">Q6 답변(1~4).</param>
        /// <param name="borderInset">경계 타일을 WFC 가 먹으므로 스폰 후보는 inset 만큼 안쪽.</param>
        /// <param name="cellCostHint">옵션. 셀별 추가 비용 (int.MaxValue = 통과 불가).
        /// 블록 하이브리드 모드에서 road 셀은 싸게, building 셀은 막고, grass 는 비싸게 하는 데 쓴다.</param>
        public static Result Generate(
            int width, int height,
            ulong seed,
            AStarGrid.Cell coreBase,
            int laneCount,
            int borderInset = 1,
            Func<int, int, int> cellCostHint = null)
        {
            if (laneCount < 1) laneCount = 1;
            if (laneCount > 8) laneCount = 8;
            if (width <= borderInset * 2 + 2 || height <= borderInset * 2 + 2)
                return Fail("grid too small for border inset");
            if (coreBase.X < borderInset || coreBase.X >= width - borderInset
                || coreBase.Y < borderInset || coreBase.Y >= height - borderInset)
                return Fail("coreBase outside playable region");

            var rng = new Rng(seed);
            var pathCellCost = new int[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pathCellCost[x, y] = 0;

            // 경계 셀 진입 불가.
            Func<int, int, int> cost = (x, y) =>
            {
                if (x < borderInset || x >= width - borderInset
                    || y < borderInset || y >= height - borderInset)
                    return int.MaxValue;

                int baseCost = 1;
                if (cellCostHint != null)
                {
                    int hint = cellCostHint(x, y);
                    if (hint == int.MaxValue) return int.MaxValue;
                    baseCost = hint;
                }
                return baseCost + pathCellCost[x, y];
            };

            var candidates = CollectPlayableBorderCells(width, height, borderInset, coreBase);
            if (candidates.Count == 0)
                return Fail("no border candidate spawn cells");

            var paths = new List<List<AStarGrid.Cell>>();
            var usedSpawns = new HashSet<int>(); // encoded idx

            for (int lane = 0; lane < laneCount; lane++)
            {
                AStarGrid.Cell spawn;
                int attempts = 0;
                while (true)
                {
                    int pickIdx = rng.NextIntRange(candidates.Count);
                    spawn = candidates[pickIdx];
                    int key = Idx(spawn.X, spawn.Y, width);
                    if (!usedSpawns.Contains(key))
                    {
                        usedSpawns.Add(key);
                        break;
                    }
                    attempts++;
                    if (attempts > candidates.Count * 4)
                    {
                        // fallback: linear scan
                        foreach (var c in candidates)
                        {
                            int k = Idx(c.X, c.Y, width);
                            if (!usedSpawns.Contains(k))
                            {
                                spawn = c;
                                usedSpawns.Add(k);
                                goto found;
                            }
                        }
                        return Fail("ran out of unique spawn cells");
                    }
                }
                found:

                var r = AStarGrid.FindPath(width, height, spawn, coreBase, cost);
                if (!r.Found)
                    return Fail($"A* failed for lane {lane}: spawn ({spawn.X},{spawn.Y}) -> core ({coreBase.X},{coreBase.Y})");

                paths.Add(r.Path);

                // bump cost for cells on this path to encourage next lane to diverge.
                foreach (var cell in r.Path)
                    pathCellCost[cell.X, cell.Y] += 4;
            }

            return new Result { Success = true, Reason = "", Paths = paths };
        }

        private static List<AStarGrid.Cell> CollectPlayableBorderCells(int w, int h, int inset, AStarGrid.Cell core)
        {
            var list = new List<AStarGrid.Cell>();
            // Ring at (x=inset, x=w-1-inset, y=inset, y=h-1-inset).
            int xLo = inset;
            int xHi = w - 1 - inset;
            int yLo = inset;
            int yHi = h - 1 - inset;

            for (int x = xLo; x <= xHi; x++)
            {
                AddIfNotCore(list, new AStarGrid.Cell(x, yLo), core);
                if (yHi != yLo)
                    AddIfNotCore(list, new AStarGrid.Cell(x, yHi), core);
            }
            for (int y = yLo + 1; y <= yHi - 1; y++)
            {
                AddIfNotCore(list, new AStarGrid.Cell(xLo, y), core);
                if (xHi != xLo)
                    AddIfNotCore(list, new AStarGrid.Cell(xHi, y), core);
            }
            return list;
        }

        private static void AddIfNotCore(List<AStarGrid.Cell> list, AStarGrid.Cell c, AStarGrid.Cell core)
        {
            if (!c.Equals(core)) list.Add(c);
        }

        private static int Idx(int x, int y, int w) => y * w + x;

        private static Result Fail(string reason) => new Result
        {
            Success = false,
            Reason = reason,
            Paths = null
        };
    }
}
