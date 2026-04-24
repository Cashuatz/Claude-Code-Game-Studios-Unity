using System;
using System.Collections.Generic;

namespace Proto.TD.Level
{
    /// <summary>
    /// 결정론적 A* 격자 탐색 (4-connected, Manhattan heuristic).
    /// HR-TD-1 정신: 순수 C# (UnityEngine 참조 없음).
    /// Tie-break 순서는 (f, g, y, x, idx) 오름차순 — 동일 입력이면 동일 경로 반환.
    /// 사용자 정의 비용 함수로 특정 셀 기피 또는 차단 가능.
    /// </summary>
    public static class AStarGrid
    {
        public struct Cell : IEquatable<Cell>
        {
            public int X, Y;
            public Cell(int x, int y) { X = x; Y = y; }
            public bool Equals(Cell other) => X == other.X && Y == other.Y;
            public override bool Equals(object obj) => obj is Cell c && Equals(c);
            public override int GetHashCode() => (X * 397) ^ Y;
            public override string ToString() => $"({X},{Y})";
        }

        public struct Result
        {
            public bool Found;
            public List<Cell> Path; // inclusive start and goal
            public int Cost;
            public int NodesExplored;
        }

        /// <summary>
        /// <paramref name="costFn"/> 은 셀 진입 비용을 반환한다. int.MaxValue 는 통과 불가.
        /// null 이면 모든 셀 비용 1.
        /// </summary>
        public static Result FindPath(
            int width, int height,
            Cell start, Cell goal,
            Func<int, int, int> costFn = null)
        {
            if (width <= 0 || height <= 0) return new Result { Found = false };
            if (!InBounds(start, width, height) || !InBounds(goal, width, height))
                return new Result { Found = false };
            if (start.Equals(goal))
            {
                return new Result { Found = true, Path = new List<Cell> { start }, Cost = 0, NodesExplored = 0 };
            }

            costFn = costFn ?? ((x, y) => 1);

            int size = width * height;
            var gScore = new int[size];
            var came = new int[size];
            var closed = new bool[size];
            for (int i = 0; i < size; i++)
            {
                gScore[i] = int.MaxValue;
                came[i] = -1;
            }

            int startIdx = Idx(start.X, start.Y, width);
            int goalIdx = Idx(goal.X, goal.Y, width);
            gScore[startIdx] = 0;

            var open = new SortedSet<OpenNode>(OpenNodeComparer.Instance);
            open.Add(new OpenNode
            {
                F = Manhattan(start, goal),
                G = 0,
                Y = start.Y,
                X = start.X,
                Idx = startIdx
            });

            int nodesExplored = 0;

            // 4-neighbours in stable order: N, E, S, W.
            // Convention: y+1 = north (Unity world z+).
            var dx = new[] { 0, 1, 0, -1 };
            var dy = new[] { 1, 0, -1, 0 };

            while (open.Count > 0)
            {
                var cur = open.Min;
                open.Remove(cur);

                if (closed[cur.Idx]) continue;
                closed[cur.Idx] = true;
                nodesExplored++;

                if (cur.Idx == goalIdx)
                {
                    return Reconstruct(came, goalIdx, width, gScore[goalIdx], nodesExplored);
                }

                for (int d = 0; d < 4; d++)
                {
                    int nx = cur.X + dx[d];
                    int ny = cur.Y + dy[d];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    int nIdx = Idx(nx, ny, width);
                    if (closed[nIdx]) continue;

                    int stepCost = costFn(nx, ny);
                    if (stepCost == int.MaxValue) continue;
                    int tentativeG = cur.G + stepCost;
                    if (tentativeG < gScore[nIdx])
                    {
                        gScore[nIdx] = tentativeG;
                        came[nIdx] = cur.Idx;
                        int f = tentativeG + Manhattan(new Cell(nx, ny), goal);
                        open.Add(new OpenNode
                        {
                            F = f,
                            G = tentativeG,
                            Y = ny,
                            X = nx,
                            Idx = nIdx
                        });
                    }
                }
            }

            return new Result { Found = false, NodesExplored = nodesExplored };
        }

        private static Result Reconstruct(int[] came, int goalIdx, int width, int cost, int explored)
        {
            var list = new List<Cell>();
            int cur = goalIdx;
            while (cur != -1)
            {
                list.Add(new Cell(cur % width, cur / width));
                cur = came[cur];
            }
            list.Reverse();
            return new Result { Found = true, Path = list, Cost = cost, NodesExplored = explored };
        }

        private static int Idx(int x, int y, int w) => y * w + x;
        private static int Manhattan(Cell a, Cell b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        private static bool InBounds(Cell c, int w, int h) => c.X >= 0 && c.X < w && c.Y >= 0 && c.Y < h;

        private struct OpenNode
        {
            public int F;
            public int G;
            public int Y;
            public int X;
            public int Idx;
        }

        private sealed class OpenNodeComparer : IComparer<OpenNode>
        {
            public static readonly OpenNodeComparer Instance = new OpenNodeComparer();
            public int Compare(OpenNode a, OpenNode b)
            {
                int c = a.F.CompareTo(b.F);
                if (c != 0) return c;
                c = a.G.CompareTo(b.G);
                if (c != 0) return c;
                c = a.Y.CompareTo(b.Y);
                if (c != 0) return c;
                c = a.X.CompareTo(b.X);
                if (c != 0) return c;
                return a.Idx.CompareTo(b.Idx);
            }
        }
    }
}
