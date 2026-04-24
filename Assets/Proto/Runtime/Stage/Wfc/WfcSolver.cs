using System;
using System.Collections.Generic;
using Proto.Shared;

namespace Proto.Stage.PCG.Wfc
{
    /// <summary>
    /// Wave Function Collapse 솔버 (결정론, 순수 C#).
    /// - HR-TD-2: Xorshift64 <see cref="Rng"/> 만 사용.
    /// - WFC-2: 수렴 실패 시 재시도 금지. 호출부가 에스컬레이션 판단.
    /// - WFC-3: 경계 조건 자동 적용 (WfcInput.TileSet.BorderTileId 가 지정된 경우).
    /// - WFC-4: 동일 엔트로피 tie-break 은 (y, x) 사전순.
    /// - 현재 구현은 n &lt;= 64 tileset 에 최적화 (ulong bitmask 경로).
    /// </summary>
    public static class WfcSolver
    {
        public static WfcOutput Solve(WfcInput input)
        {
            // ── Input validation ─────────────────────────────────
            if (input.TileSet == null)
                return Fail(WfcStatus.InvalidInput, "TileSet is null");
            if (input.Width <= 0 || input.Height <= 0)
                return Fail(WfcStatus.InvalidInput, $"invalid grid size {input.Width}x{input.Height}");
            if (input.TileSet.TileCount > 64)
                return Fail(WfcStatus.TilesetIncomplete, "tileset > 64 tiles not supported in current build");

            int n = input.TileSet.TileCount;
            ulong allMask = n == 64 ? ulong.MaxValue : ((1UL << n) - 1UL);

            // Weight-0 tiles are "fixed-only": they never appear via observation.
            // They must be placed via FixedCells or they won't appear at all.
            ulong observableMask = allMask;
            for (int i = 0; i < n; i++)
            {
                if (input.TileSet.Tiles[i].Weight <= 0)
                    observableMask &= ~(1UL << i);
            }

            int w = input.Width;
            int h = input.Height;

            var cells = new ulong[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    cells[x, y] = observableMask;

            var rng = new Rng(input.Seed);
            var propQueue = new Queue<(int x, int y)>();

            // ── Apply border constraint (WFC-3) ──────────────────
            // Fixed placement (overwrite, not AND) — border tile may be weight=0 so
            // it's excluded from observableMask and AND would wipe it.
            string borderId = input.TileSet.BorderTileId;
            if (!string.IsNullOrEmpty(borderId))
            {
                if (!input.TileSet.TryGetIndex(borderId, out int borderIdx))
                    return Fail(WfcStatus.TilesetIncomplete, $"border tile not found: {borderId}");
                ulong borderMask = 1UL << borderIdx;

                for (int x = 0; x < w; x++)
                {
                    cells[x, 0] = borderMask;
                    cells[x, h - 1] = borderMask;
                    propQueue.Enqueue((x, 0));
                    propQueue.Enqueue((x, h - 1));
                }
                for (int y = 1; y < h - 1; y++)
                {
                    cells[0, y] = borderMask;
                    cells[w - 1, y] = borderMask;
                    propQueue.Enqueue((0, y));
                    propQueue.Enqueue((w - 1, y));
                }
            }

            // ── Apply fixed cells ────────────────────────────────
            // Same: overwrite (fixed tiles may have weight=0 and be outside observable mask).
            if (input.FixedCells != null)
            {
                for (int i = 0; i < input.FixedCells.Count; i++)
                {
                    var fc = input.FixedCells[i];
                    if (fc.X < 0 || fc.X >= w || fc.Y < 0 || fc.Y >= h)
                        return Fail(WfcStatus.InvalidInput,
                            $"fixed cell out of range: ({fc.X},{fc.Y})");
                    if (!input.TileSet.TryGetIndex(fc.TileId, out int idx))
                        return Fail(WfcStatus.TilesetIncomplete,
                            $"fixed tile not in set: {fc.TileId} at ({fc.X},{fc.Y})");
                    cells[fc.X, fc.Y] = 1UL << idx;
                    propQueue.Enqueue((fc.X, fc.Y));
                }
            }

            // ── Initial propagation ──────────────────────────────
            if (!Propagate(cells, w, h, input.TileSet, propQueue, out var contradiction0))
                return Contradiction(contradiction0.x, contradiction0.y);

            // ── Main collapse loop ───────────────────────────────
            int iterations = 0;
            int maxIterations = w * h + 8; // each iteration collapses ≥1 cell; safety margin
            while (true)
            {
                if (iterations > maxIterations)
                    return Fail(WfcStatus.Contradiction, $"exceeded max iterations {maxIterations}");

                // Find min-entropy cell (WFC-4 tie-break: (y,x) lex min).
                int targetX = -1, targetY = -1;
                int bestCount = int.MaxValue;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        ulong mask = cells[x, y];
                        int c = PopCount(mask);
                        if (c == 0) return Contradiction(x, y); // should have been caught, safety
                        if (c == 1) continue; // collapsed
                        if (c < bestCount)
                        {
                            bestCount = c;
                            targetX = x;
                            targetY = y;
                            if (c == 2) goto foundMin; // can't go lower
                        }
                    }
                }
                foundMin:
                if (targetX < 0)
                {
                    // all collapsed
                    return Success(cells, w, h, input.TileSet, iterations);
                }

                // Observe: pick one tile by weighted RNG (stable index order).
                int picked = PickWeighted(cells[targetX, targetY], input.TileSet, rng);
                ulong pickedMask = 1UL << picked;

                if (!CollapseCell(cells, targetX, targetY, pickedMask, propQueue, out var rc))
                    return Contradiction(rc.x, rc.y);

                if (!Propagate(cells, w, h, input.TileSet, propQueue, out var contra))
                    return Contradiction(contra.x, contra.y);

                iterations++;
            }

            // Local helpers ──────────────────────────────────────
            WfcOutput Contradiction(int cx, int cy) => new WfcOutput
            {
                Status = WfcStatus.Contradiction,
                Reason = $"contradiction at ({cx},{cy})",
                Grid = null,
                Iterations = 0,
                ContradictionCellX = cx,
                ContradictionCellY = cy
            };
        }

        // ─────────────────────────────────────────────────────────
        //  Internals
        // ─────────────────────────────────────────────────────────

        private static bool CollapseCell(
            ulong[,] cells, int x, int y, ulong newMask,
            Queue<(int x, int y)> propQueue,
            out (int x, int y) contradictionCell)
        {
            ulong before = cells[x, y];
            ulong next = before & newMask;
            if (next == 0UL)
            {
                contradictionCell = (x, y);
                return false;
            }
            if (next != before)
            {
                cells[x, y] = next;
                propQueue.Enqueue((x, y));
            }
            contradictionCell = (-1, -1);
            return true;
        }

        private static bool Propagate(
            ulong[,] cells, int w, int h,
            WfcTileSetData ts,
            Queue<(int x, int y)> queue,
            out (int x, int y) contradictionCell)
        {
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                ulong myMask = cells[x, y];

                for (int d = 0; d < 4; d++)
                {
                    var (dx, dy) = DirectionOps.Delta((Direction)d);
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

                    ulong allowedForNeighbor = 0UL;
                    ulong mm = myMask;
                    while (mm != 0UL)
                    {
                        int ti = BitScanForward(mm);
                        mm &= mm - 1UL;
                        allowedForNeighbor |= ts.CompatibleMask[ti, d];
                    }

                    ulong neighborBefore = cells[nx, ny];
                    ulong neighborNext = neighborBefore & allowedForNeighbor;
                    if (neighborNext == 0UL)
                    {
                        contradictionCell = (nx, ny);
                        return false;
                    }
                    if (neighborNext != neighborBefore)
                    {
                        cells[nx, ny] = neighborNext;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
            contradictionCell = (-1, -1);
            return true;
        }

        private static int PickWeighted(ulong mask, WfcTileSetData ts, Rng rng)
        {
            // Gather weights in stable index order.
            int n = ts.TileCount;
            Span<int> weights = stackalloc int[64];
            Span<int> indices = stackalloc int[64];
            int k = 0;
            for (int i = 0; i < n; i++)
            {
                if ((mask & (1UL << i)) != 0UL)
                {
                    int w = ts.Tiles[i].Weight;
                    weights[k] = w > 0 ? w : 1; // avoid all-zero bucket
                    indices[k] = i;
                    k++;
                }
            }
            if (k == 0) throw new InvalidOperationException("PickWeighted on empty mask");
            if (k == 1) return indices[0];

            // Ensure total weight > 0
            int total = 0;
            for (int i = 0; i < k; i++) total += weights[i];
            if (total == 0)
            {
                for (int i = 0; i < k; i++) weights[i] = 1;
            }

            int pickIndexInSubset = rng.WeightedPick(weights.Slice(0, k));
            return indices[pickIndexInSubset];
        }

        private static WfcOutput Success(ulong[,] cells, int w, int h, WfcTileSetData ts, int iterations)
        {
            var grid = new string[w, h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    ulong m = cells[x, y];
                    if (PopCount(m) != 1)
                    {
                        // shouldn't happen if loop terminated by all-collapsed; treat as contradiction
                        return new WfcOutput
                        {
                            Status = WfcStatus.Contradiction,
                            Reason = $"non-singleton at ({x},{y}) on finalize",
                            Grid = null,
                            Iterations = iterations,
                            ContradictionCellX = x,
                            ContradictionCellY = y
                        };
                    }
                    int idx = BitScanForward(m);
                    grid[x, y] = ts.Tiles[idx].TileId;
                }
            }
            return new WfcOutput
            {
                Status = WfcStatus.Ok,
                Reason = "",
                Grid = grid,
                Iterations = iterations,
                ContradictionCellX = -1,
                ContradictionCellY = -1
            };
        }

        private static WfcOutput Fail(WfcStatus status, string reason) => new WfcOutput
        {
            Status = status,
            Reason = reason,
            Grid = null,
            Iterations = 0,
            ContradictionCellX = -1,
            ContradictionCellY = -1
        };

        private static int PopCount(ulong v)
        {
            v = v - ((v >> 1) & 0x5555555555555555UL);
            v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
            v = (v + (v >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
            return (int)((v * 0x0101010101010101UL) >> 56);
        }

        private static int BitScanForward(ulong v)
        {
            // de Bruijn sequence LSB lookup
            if (v == 0UL) return -1;
            const ulong deBruijn = 0x03f79d71b4cb0a89UL;
            int[] table = DeBruijnTable;
            return table[((v & (~v + 1UL)) * deBruijn) >> 58];
        }

        private static readonly int[] DeBruijnTable =
        {
            0,  1, 48,  2, 57, 49, 28,  3,
            61, 58, 50, 42, 38, 29, 17,  4,
            62, 55, 59, 36, 53, 51, 43, 22,
            45, 39, 33, 30, 24, 18, 12,  5,
            63, 47, 56, 27, 60, 41, 37, 16,
            54, 35, 52, 21, 44, 32, 23, 11,
            46, 26, 40, 15, 34, 20, 31, 10,
            25, 14, 19,  9, 13,  8,  7,  6
        };
    }
}
