using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SysRandom = System.Random;
using Debug = UnityEngine.Debug;

namespace Proto.Stage.PCG
{
    /// <summary>
    /// 시드 기반 시가지 그레이박싱 PCG (Block Subdivision + 불균일 그리드).
    /// 실제 시각 표현은 <see cref="CityscapeAssetSet"/> 의 프리팹으로 위임.
    ///
    /// 주요 비대칭성 소스:
    ///  - 블록 간격 jitter (blockSpacingJitter)
    ///  - 큰 도로 확률 (bigRoadProbability × bigRoadWidthMultiplier)
    ///  - Empty 블록 (emptyBlockProbability) — 도로만 보이는 광장 효과
    ///  - 블록 타입 3종 (Open/Mixed/Dense)
    ///  - 건물 높이 power curve + yaw jitter
    ///  - 풀은 인도-차도 사이 녹지 띠에 배치 (Open 블록만 전체 공원)
    ///
    /// HR-1: Rigidbody/Collider 미부착(프리팹 단계 관리).
    /// HR-3: System.Random(seed) 결정론.
    /// HR-8: 수치는 CityscapeProfile SO. HR-10: Assets/Proto 하위 전용.
    /// </summary>
    public static class CityscapeGenerator
    {
        private const float BuildingHeightPower = 2.5f;
        // 시가지 도메인 룰: 건물은 도로 축에 정렬 (HR-PCG-City-01 제안, PROP-2026-04-24-001).
        // 과거의 yaw jitter 는 "균일함" 해결책을 잘못된 축에 적용한 것이었고 FT-02 (Hidden
        // Semantic Rule Misfill) 로 분류되어 제거됨.

        public enum GenerateStatus { Ok, ProfileMissing, AssetSetMissing, AssetSetIncomplete }

        public struct GenerateResult
        {
            public GenerateStatus Status;
            public string Reason;
            public CityscapeMetrics Metrics;
        }

        private enum BlockDensity { Empty, Open, Mixed, Dense }

        private struct BlockParams
        {
            public int MaxDepth;
            public float SplitChance;
            public float HeightScale;
        }

        private class GenContext
        {
            public Transform Parent;
            public CityscapeProfile Profile;
            public CityscapeAssetSet Assets;
            public SysRandom Rng;
            public CityscapeMetrics Metrics;
            public List<string> Logs;
            public int BlocksEmpty, BlocksOpen, BlocksMixed, BlocksDense;
        }

        public static GenerateResult Generate(Transform parent, int seed, CityscapeProfile profile, CityscapeAssetSet assets, List<string> logs = null)
        {
            var localLogs = logs ?? new List<string>();

            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (profile == null) return Fail(GenerateStatus.ProfileMissing, "CityscapeProfile is null.", localLogs);
            if (assets == null) return Fail(GenerateStatus.AssetSetMissing, "CityscapeAssetSet is null.", localLogs);
            if (!assets.IsComplete())
                return Fail(GenerateStatus.AssetSetIncomplete, $"AssetSet missing prefabs: {assets.DescribeMissing()}", localLogs);

            var sw = Stopwatch.StartNew();
            ClearChildren(parent);

            var ctx = new GenContext
            {
                Parent = parent,
                Profile = profile,
                Assets = assets,
                Rng = new SysRandom(seed),
                Metrics = new CityscapeMetrics { Seed = seed },
                Logs = localLogs,
            };

            ctx.Logs.Add($"[Cityscape] Generate seed={seed}  city={profile.citySize.x}x{profile.citySize.y}  block={profile.blockSize}");

            // 1) 축별 불균일 stops + widths 계산 (도로 폭 jitter 포함)
            ComputeAxis(ctx.Rng, profile, profile.citySize.x, out var xStops, out var xWidths);
            ComputeAxis(ctx.Rng, profile, profile.citySize.y, out var zStops, out var zWidths);

            // 2) 도로/페인트 배치
            BuildRoadsAndPaint(ctx, xStops, xWidths, zStops, zWidths);

            // 3) 블록 순회
            BuildBlocks(ctx, xStops, xWidths, zStops, zWidths);

            sw.Stop();
            ctx.Metrics.GenerateMs = sw.Elapsed.TotalMilliseconds;
            ctx.Metrics.TotalGoCount =
                ctx.Metrics.RoadQuadCount + ctx.Metrics.PaintQuadCount +
                ctx.Metrics.CurbCubeCount + ctx.Metrics.BuildingCubeCount +
                ctx.Metrics.PlanterCubeCount + ctx.Metrics.GrassSphereCount;
            ctx.Metrics.BoundsSize = new Vector3(profile.citySize.x, 0f, profile.citySize.y);

            ctx.Logs.Add($"[Cityscape] BlockMix  Empty={ctx.BlocksEmpty}  Open={ctx.BlocksOpen}  Mixed={ctx.BlocksMixed}  Dense={ctx.BlocksDense}");
            ctx.Logs.Add($"[Cityscape] Done {ctx.Metrics.GenerateMs:F1} ms  blocks={ctx.Metrics.BlockCount}  lots={ctx.Metrics.LotCount}  GO={ctx.Metrics.TotalGoCount}");

            // MCP read_console 용 단일 라인 요약
            Debug.Log($"[Cityscape] seed={seed} ms={ctx.Metrics.GenerateMs:F1} blocks={ctx.Metrics.BlockCount}(E{ctx.BlocksEmpty}/O{ctx.BlocksOpen}/M{ctx.BlocksMixed}/D{ctx.BlocksDense}) lots={ctx.Metrics.LotCount} GO={ctx.Metrics.TotalGoCount} road={ctx.Metrics.RoadQuadCount} paint={ctx.Metrics.PaintQuadCount} curb={ctx.Metrics.CurbCubeCount} bld={ctx.Metrics.BuildingCubeCount} plan={ctx.Metrics.PlanterCubeCount} grass={ctx.Metrics.GrassSphereCount}");

            return new GenerateResult { Status = GenerateStatus.Ok, Reason = "", Metrics = ctx.Metrics };
        }

        public static void Clear(Transform parent)
        {
            if (parent == null) return;
            ClearChildren(parent);
        }

        private static GenerateResult Fail(GenerateStatus s, string reason, List<string> logs)
        {
            logs.Add($"[Cityscape] FAIL {s}: {reason}");
            Debug.LogWarning($"[Cityscape] Generate FAIL status={s} reason={reason}");
            return new GenerateResult { Status = s, Reason = reason, Metrics = default };
        }

        // ---------- axis computation ----------

        /// <summary>
        /// 시드 기반 불균일 간격 stops + 각 stop 위치의 도로 폭을 계산.
        /// stops[i] 는 도로 i 의 중심선 위치, widths[i] 는 그 도로의 폭.
        /// </summary>
        private static void ComputeAxis(SysRandom rng, CityscapeProfile p, float cityLen, out float[] stops, out float[] widths)
        {
            var stopsList = new List<float>();
            var widthsList = new List<float>();

            float cursor = 0f;
            stopsList.Add(cursor);
            widthsList.Add(SampleRoadWidth(rng, p));

            while (true)
            {
                float jitter = 1f + ((float)rng.NextDouble() - 0.5f) * 2f * p.blockSpacingJitter;
                float step = Mathf.Max(p.blockSize * 0.4f, p.blockSize * jitter);
                float next = cursor + step;
                if (next >= cityLen - p.blockSize * 0.3f) break;
                cursor = next;
                stopsList.Add(cursor);
                widthsList.Add(SampleRoadWidth(rng, p));
            }

            // 마지막 도로를 cityLen 에
            stopsList.Add(cityLen);
            widthsList.Add(SampleRoadWidth(rng, p));

            stops = stopsList.ToArray();
            widths = widthsList.ToArray();
        }

        /// <summary>
        /// 가중치 기반 3-class 도로 폭 샘플링 (Local / Collector / Arterial).
        /// 현실 시가지 위계와 동일: Local 이 가장 많고 Arterial 이 드물다.
        /// </summary>
        private static float SampleRoadWidth(SysRandom rng, CityscapeProfile p)
        {
            float wL = Mathf.Max(0f, p.weightLocal);
            float wC = Mathf.Max(0f, p.weightCollector);
            float wA = Mathf.Max(0f, p.weightArterial);
            float sum = wL + wC + wA;
            if (sum <= 0f) return p.roadWidthLocal; // degenerate — fallback
            double r = rng.NextDouble() * sum;
            if (r < wL) return p.roadWidthLocal;
            if (r < wL + wC) return p.roadWidthCollector;
            return p.roadWidthArterial;
        }

        // ---------- roads ----------

        private static void BuildRoadsAndPaint(GenContext ctx, float[] xStops, float[] xWidths, float[] zStops, float[] zWidths)
        {
            var p = ctx.Profile;
            float w = p.citySize.x;
            float d = p.citySize.y;

            // 가로 도로 (각 z stop 에서 X축 따라)
            for (int j = 0; j < zStops.Length; j++)
            {
                float z = zStops[j];
                float rw = zWidths[j];
                SpawnQuadFlat(ctx, ctx.Assets.roadQuadPrefab, $"Road_H_{j}",
                    new Vector3(w * 0.5f, 0.001f, z), new Vector2(w, rw));
                ctx.Metrics.RoadQuadCount++;

                SpawnQuadFlat(ctx, ctx.Assets.paintQuadPrefab, $"Paint_H_{j}",
                    new Vector3(w * 0.5f, 0.001f + p.paintYOffset, z), new Vector2(w * 0.95f, p.laneStripeWidth));
                ctx.Metrics.PaintQuadCount++;
            }

            // 세로 도로 (각 x stop 에서 Z축 따라)
            for (int i = 0; i < xStops.Length; i++)
            {
                float x = xStops[i];
                float rw = xWidths[i];
                SpawnQuadFlat(ctx, ctx.Assets.roadQuadPrefab, $"Road_V_{i}",
                    new Vector3(x, 0.001f, d * 0.5f), new Vector2(rw, d));
                ctx.Metrics.RoadQuadCount++;

                SpawnQuadFlat(ctx, ctx.Assets.paintQuadPrefab, $"Paint_V_{i}",
                    new Vector3(x, 0.001f + p.paintYOffset, d * 0.5f), new Vector2(p.laneStripeWidth, d * 0.95f));
                ctx.Metrics.PaintQuadCount++;
            }
        }

        // ---------- blocks ----------

        private static void BuildBlocks(GenContext ctx, float[] xStops, float[] xWidths, float[] zStops, float[] zWidths)
        {
            var p = ctx.Profile;

            for (int i = 0; i < xStops.Length - 1; i++)
            {
                float xMin = xStops[i]     + xWidths[i]     * 0.5f;
                float xMax = xStops[i + 1] - xWidths[i + 1] * 0.5f;
                if (xMax - xMin <= 1f) continue;

                for (int j = 0; j < zStops.Length - 1; j++)
                {
                    float zMin = zStops[j]     + zWidths[j]     * 0.5f;
                    float zMax = zStops[j + 1] - zWidths[j + 1] * 0.5f;
                    if (zMax - zMin <= 1f) continue;

                    var blockRect = Rect.MinMaxRect(xMin, zMin, xMax, zMax);
                    ctx.Metrics.BlockCount++;

                    var density = PickDensity(ctx);
                    if (density == BlockDensity.Empty)
                    {
                        // 완전 스킵: 연석도 풀도 배치하지 않음 → 인접 도로들이 광장처럼 트임
                        continue;
                    }

                    BuildCurbsAround(ctx, blockRect);
                    BuildSidewalkGrassStrip(ctx, blockRect);

                    if (density == BlockDensity.Open)
                    {
                        BuildParkBlock(ctx, blockRect);
                        continue;
                    }

                    var prm = ParamsFor(density, p);
                    var lotRect = new Rect(
                        blockRect.x + p.sidewalkWidth,
                        blockRect.y + p.sidewalkWidth,
                        blockRect.width - p.sidewalkWidth * 2f,
                        blockRect.height - p.sidewalkWidth * 2f);
                    if (lotRect.width > 0f && lotRect.height > 0f)
                        SubdivideAndBuildLots(ctx, lotRect, 0, prm);
                }
            }
        }

        private static BlockDensity PickDensity(GenContext ctx)
        {
            double r = ctx.Rng.NextDouble();
            double e = ctx.Profile.emptyBlockProbability;
            double rest = 1.0 - e;
            // 분배: Open 20% / Mixed 50% / Dense 30%  of rest
            double cO = e + rest * 0.20;
            double cM = e + rest * 0.70;
            if (r < e)  { ctx.BlocksEmpty++; return BlockDensity.Empty; }
            if (r < cO) { ctx.BlocksOpen++;  return BlockDensity.Open; }
            if (r < cM) { ctx.BlocksMixed++; return BlockDensity.Mixed; }
            ctx.BlocksDense++;
            return BlockDensity.Dense;
        }

        private static BlockParams ParamsFor(BlockDensity d, CityscapeProfile p)
        {
            switch (d)
            {
                case BlockDensity.Dense:
                    return new BlockParams
                    {
                        MaxDepth = p.maxLotSplitDepth + 1,
                        SplitChance = Mathf.Clamp01(p.splitChance + 0.2f),
                        HeightScale = 1.4f,
                    };
                default: // Mixed
                    return new BlockParams
                    {
                        MaxDepth = p.maxLotSplitDepth,
                        SplitChance = p.splitChance,
                        HeightScale = 1.0f,
                    };
            }
        }

        private static void BuildCurbsAround(GenContext ctx, Rect rect)
        {
            var p = ctx.Profile;
            float t = Mathf.Max(0.2f, p.sidewalkWidth * 0.4f);
            float h = p.curbHeight;

            SpawnCube(ctx, ctx.Assets.curbCubePrefab, "Curb_T", new Vector3(rect.center.x, h * 0.5f, rect.yMin), new Vector3(rect.width, h, t), 0f);
            SpawnCube(ctx, ctx.Assets.curbCubePrefab, "Curb_B", new Vector3(rect.center.x, h * 0.5f, rect.yMax), new Vector3(rect.width, h, t), 0f);
            SpawnCube(ctx, ctx.Assets.curbCubePrefab, "Curb_L", new Vector3(rect.xMin, h * 0.5f, rect.center.y), new Vector3(t, h, rect.height), 0f);
            SpawnCube(ctx, ctx.Assets.curbCubePrefab, "Curb_R", new Vector3(rect.xMax, h * 0.5f, rect.center.y), new Vector3(t, h, rect.height), 0f);
            ctx.Metrics.CurbCubeCount += 4;
        }

        // ---------- sidewalk grass strip (차도-인도 사이) ----------

        private static void BuildSidewalkGrassStrip(GenContext ctx, Rect rect)
        {
            var p = ctx.Profile;
            float off = p.sidewalkGrassOffset;

            var a1 = new Vector2(rect.xMin + off, rect.yMin + off);
            var a2 = new Vector2(rect.xMax - off, rect.yMin + off);
            var b1 = new Vector2(rect.xMax - off, rect.yMin + off);
            var b2 = new Vector2(rect.xMax - off, rect.yMax - off);
            var c1 = new Vector2(rect.xMax - off, rect.yMax - off);
            var c2 = new Vector2(rect.xMin + off, rect.yMax - off);
            var d1 = new Vector2(rect.xMin + off, rect.yMax - off);
            var d2 = new Vector2(rect.xMin + off, rect.yMin + off);

            ScatterAlongLine(ctx, a1, a2);
            ScatterAlongLine(ctx, b1, b2);
            ScatterAlongLine(ctx, c1, c2);
            ScatterAlongLine(ctx, d1, d2);
        }

        private static void ScatterAlongLine(GenContext ctx, Vector2 a, Vector2 b)
        {
            var p = ctx.Profile;
            float len = Vector2.Distance(a, b);
            if (len <= 0.5f) return;

            int count = Mathf.RoundToInt(len * p.sidewalkGrassDensityPerM);
            if (count <= 0) return;

            Vector2 dir = (b - a).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);

            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f + ((float)ctx.Rng.NextDouble() - 0.5f) * 0.8f) / count;
                t = Mathf.Clamp01(t);
                Vector2 pos = Vector2.Lerp(a, b, t);
                float jitter = ((float)ctx.Rng.NextDouble() - 0.5f) * 2f * p.sidewalkGrassJitter;
                pos += perp * jitter;

                float gr = (float)Lerp(p.grassRadiusMin, p.grassRadiusMax, ctx.Rng.NextDouble());
                SpawnSphere(ctx, ctx.Assets.grassSpherePrefab, "Grass_Strip",
                    new Vector3(pos.x, gr * 0.5f, pos.y), Vector3.one * gr * 2f);
                ctx.Metrics.GrassSphereCount++;
            }
        }

        // ---------- park block (Open) ----------

        private static void BuildParkBlock(GenContext ctx, Rect rect)
        {
            var p = ctx.Profile;
            // 블록 전체를 planter 한 장으로 깔고 풀을 촘촘히
            SpawnCube(ctx, ctx.Assets.planterCubePrefab, "Planter_Park",
                new Vector3(rect.center.x, p.planterHeight * 0.5f, rect.center.y),
                new Vector3(rect.width * 0.95f, p.planterHeight, rect.height * 0.95f), 0f);
            ctx.Metrics.PlanterCubeCount++;

            int target = Mathf.RoundToInt(rect.width * rect.height * p.grassDensityPerSqm);
            for (int i = 0; i < target; i++)
            {
                float gx = rect.x + (float)ctx.Rng.NextDouble() * rect.width;
                float gz = rect.y + (float)ctx.Rng.NextDouble() * rect.height;
                float gr = (float)Lerp(p.grassRadiusMin, p.grassRadiusMax, ctx.Rng.NextDouble());

                SpawnSphere(ctx, ctx.Assets.grassSpherePrefab, "Grass_Park",
                    new Vector3(gx, p.planterHeight + gr * 0.5f, gz),
                    Vector3.one * gr * 2f);
                ctx.Metrics.GrassSphereCount++;
            }
        }

        // ---------- lot subdivision (Mixed / Dense only) ----------

        private static void SubdivideAndBuildLots(GenContext ctx, Rect rect, int depth, BlockParams prm)
        {
            var p = ctx.Profile;
            bool canSplit =
                depth < prm.MaxDepth &&
                rect.width  > p.minLotEdge * 2f &&
                rect.height > p.minLotEdge * 2f &&
                ctx.Rng.NextDouble() < prm.SplitChance;

            if (!canSplit)
            {
                BuildLot(ctx, rect, prm);
                return;
            }

            bool splitVertical = rect.width >= rect.height
                ? ctx.Rng.NextDouble() < 0.7
                : ctx.Rng.NextDouble() < 0.3;

            if (splitVertical)
            {
                float t = (float)Lerp(0.30, 0.70, ctx.Rng.NextDouble());
                float xMid = rect.x + rect.width * t;
                SubdivideAndBuildLots(ctx, new Rect(rect.x, rect.y, xMid - rect.x, rect.height), depth + 1, prm);
                SubdivideAndBuildLots(ctx, new Rect(xMid, rect.y, rect.xMax - xMid, rect.height), depth + 1, prm);
            }
            else
            {
                float t = (float)Lerp(0.30, 0.70, ctx.Rng.NextDouble());
                float zMid = rect.y + rect.height * t;
                SubdivideAndBuildLots(ctx, new Rect(rect.x, rect.y, rect.width, zMid - rect.y), depth + 1, prm);
                SubdivideAndBuildLots(ctx, new Rect(rect.x, zMid, rect.width, rect.yMax - zMid), depth + 1, prm);
            }
        }

        private static void BuildLot(GenContext ctx, Rect rect, BlockParams prm)
        {
            ctx.Metrics.LotCount++;
            var p = ctx.Profile;

            float t = Mathf.Pow((float)ctx.Rng.NextDouble(), BuildingHeightPower);
            float h = Mathf.Lerp(p.buildingHeightMin, p.buildingHeightMax, t) * prm.HeightScale;
            float fp = p.buildingFootprint;
            float sx = rect.width  * fp;
            float sz = rect.height * fp;

            // HR-PCG-City-01: 건물은 도로 축 정렬 (yaw = 0).
            SpawnCube(ctx, ctx.Assets.buildingCubePrefab, "Building",
                new Vector3(rect.center.x, h * 0.5f, rect.center.y),
                new Vector3(sx, h, sz), yawDeg: 0f);
            ctx.Metrics.BuildingCubeCount++;
        }

        // ---------- prefab spawn helpers ----------

        private static void SpawnQuadFlat(GenContext ctx, GameObject prefab, string name, Vector3 center, Vector2 size)
        {
            var go = SpawnPrefab(prefab, ctx.Parent);
            go.name = name;
            go.transform.localPosition = center;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static void SpawnCube(GenContext ctx, GameObject prefab, string name, Vector3 center, Vector3 size, float yawDeg)
        {
            var go = SpawnPrefab(prefab, ctx.Parent);
            go.name = name;
            go.transform.localPosition = center;
            go.transform.localRotation = Quaternion.Euler(0f, yawDeg, 0f);
            go.transform.localScale = size;
        }

        private static void SpawnSphere(GenContext ctx, GameObject prefab, string name, Vector3 center, Vector3 size)
        {
            var go = SpawnPrefab(prefab, ctx.Parent);
            go.name = name;
            go.transform.localPosition = center;
            go.transform.localScale = size;
        }

        private static GameObject SpawnPrefab(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
#endif
            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying) UnityEngine.Object.DestroyImmediate(child);
                else UnityEngine.Object.Destroy(child);
#else
                UnityEngine.Object.Destroy(child);
#endif
            }
        }

        private static double Lerp(double a, double b, double t) => a + (b - a) * t;
    }
}
