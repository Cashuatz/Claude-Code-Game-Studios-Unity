using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SysRandom = System.Random;
using Debug = UnityEngine.Debug;

namespace Proto.Stage.PCG
{
    /// <summary>
    /// 시드 기반 시가지 그레이박싱 PCG (Block Subdivision).
    /// 실제 시각 표현은 전부 <see cref="CityscapeAssetSet"/> 의 프리팹으로 위임한다.
    /// 프리팹을 교체하면 Generator 코드 변경 없이 그레이박싱 → 실 에셋 전환 가능.
    ///
    /// HR-1: Rigidbody/Collider 미부착(프리팹 단계에서 관리).
    /// HR-3: System.Random(seed) 결정론.
    /// HR-8: 수치는 CityscapeProfile SO. HR-10: Assets/Proto 하위 전용.
    /// </summary>
    public static class CityscapeGenerator
    {
        private const float BuildingHeightPower = 2.5f;
        private const float BuildingYawJitterDeg = 4f;

        public enum GenerateStatus { Ok, ProfileMissing, AssetSetMissing, AssetSetIncomplete }

        public struct GenerateResult
        {
            public GenerateStatus Status;
            public string Reason;
            public CityscapeMetrics Metrics;
        }

        private enum BlockDensity { Open, Mixed, Dense }

        private struct BlockParams
        {
            public int MaxDepth;
            public float SplitChance;
            public float BuildingProb;
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
            public int BlocksOpen, BlocksMixed, BlocksDense;
        }

        public static GenerateResult Generate(Transform parent, int seed, CityscapeProfile profile, CityscapeAssetSet assets, List<string> logs = null)
        {
            var localLogs = logs ?? new List<string>();

            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (profile == null)
                return Fail(GenerateStatus.ProfileMissing, "CityscapeProfile is null.", localLogs);
            if (assets == null)
                return Fail(GenerateStatus.AssetSetMissing, "CityscapeAssetSet is null.", localLogs);
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

            BuildRoadsAndPaint(ctx);
            BuildBlocks(ctx);

            sw.Stop();
            ctx.Metrics.GenerateMs = sw.Elapsed.TotalMilliseconds;
            ctx.Metrics.TotalGoCount =
                ctx.Metrics.RoadQuadCount + ctx.Metrics.PaintQuadCount +
                ctx.Metrics.CurbCubeCount + ctx.Metrics.BuildingCubeCount +
                ctx.Metrics.PlanterCubeCount + ctx.Metrics.GrassSphereCount;
            ctx.Metrics.BoundsSize = new Vector3(profile.citySize.x, 0f, profile.citySize.y);

            ctx.Logs.Add($"[Cityscape] BlockMix  Open={ctx.BlocksOpen}  Mixed={ctx.BlocksMixed}  Dense={ctx.BlocksDense}");
            ctx.Logs.Add($"[Cityscape] Done {ctx.Metrics.GenerateMs:F1} ms  blocks={ctx.Metrics.BlockCount}  lots={ctx.Metrics.LotCount}  GO={ctx.Metrics.TotalGoCount}");

            // MCP read_console 로 Claude 가 바로 읽을 수 있도록 한 줄 요약
            Debug.Log($"[Cityscape] seed={seed} ms={ctx.Metrics.GenerateMs:F1} blocks={ctx.Metrics.BlockCount}(O{ctx.BlocksOpen}/M{ctx.BlocksMixed}/D{ctx.BlocksDense}) lots={ctx.Metrics.LotCount} GO={ctx.Metrics.TotalGoCount} road={ctx.Metrics.RoadQuadCount} paint={ctx.Metrics.PaintQuadCount} curb={ctx.Metrics.CurbCubeCount} bld={ctx.Metrics.BuildingCubeCount} plan={ctx.Metrics.PlanterCubeCount} grass={ctx.Metrics.GrassSphereCount}");

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

        // ---------- roads ----------

        private static void BuildRoadsAndPaint(GenContext ctx)
        {
            var p = ctx.Profile;
            float w = p.citySize.x;
            float d = p.citySize.y;
            float bs = p.blockSize;

            for (float z = 0f; z <= d + 0.001f; z += bs)
            {
                SpawnQuadFlat(ctx, ctx.Assets.roadQuadPrefab,  $"Road_H_{z:F0}",
                    new Vector3(w * 0.5f, 0.001f, z), new Vector2(w, p.roadWidth));
                ctx.Metrics.RoadQuadCount++;

                SpawnQuadFlat(ctx, ctx.Assets.paintQuadPrefab, $"Paint_H_{z:F0}",
                    new Vector3(w * 0.5f, 0.001f + p.paintYOffset, z), new Vector2(w * 0.95f, p.laneStripeWidth));
                ctx.Metrics.PaintQuadCount++;
            }

            for (float x = 0f; x <= w + 0.001f; x += bs)
            {
                SpawnQuadFlat(ctx, ctx.Assets.roadQuadPrefab,  $"Road_V_{x:F0}",
                    new Vector3(x, 0.001f, d * 0.5f), new Vector2(p.roadWidth, d));
                ctx.Metrics.RoadQuadCount++;

                SpawnQuadFlat(ctx, ctx.Assets.paintQuadPrefab, $"Paint_V_{x:F0}",
                    new Vector3(x, 0.001f + p.paintYOffset, d * 0.5f), new Vector2(p.laneStripeWidth, d * 0.95f));
                ctx.Metrics.PaintQuadCount++;
            }
        }

        // ---------- blocks / lots ----------

        private static void BuildBlocks(GenContext ctx)
        {
            var p = ctx.Profile;
            float w = p.citySize.x;
            float d = p.citySize.y;
            float bs = p.blockSize;
            float halfRoad = p.roadWidth * 0.5f;

            for (float bx = 0f; bx + bs <= w + 0.001f; bx += bs)
            {
                for (float bz = 0f; bz + bs <= d + 0.001f; bz += bs)
                {
                    var blockRect = new Rect(
                        bx + halfRoad, bz + halfRoad,
                        bs - p.roadWidth, bs - p.roadWidth);
                    if (blockRect.width <= 0f || blockRect.height <= 0f) continue;

                    BuildCurbsAround(ctx, blockRect);

                    var density = PickDensity(ctx);
                    var prm = ParamsFor(density, p);

                    var lotRect = new Rect(
                        blockRect.x + p.sidewalkWidth,
                        blockRect.y + p.sidewalkWidth,
                        blockRect.width - p.sidewalkWidth * 2f,
                        blockRect.height - p.sidewalkWidth * 2f);
                    if (lotRect.width > 0f && lotRect.height > 0f)
                        SubdivideAndBuildLots(ctx, lotRect, depth: 0, prm);

                    ctx.Metrics.BlockCount++;
                }
            }
        }

        private static BlockDensity PickDensity(GenContext ctx)
        {
            double r = ctx.Rng.NextDouble();
            if (r < 0.20) { ctx.BlocksOpen++;  return BlockDensity.Open; }
            if (r < 0.70) { ctx.BlocksMixed++; return BlockDensity.Mixed; }
            ctx.BlocksDense++;
            return BlockDensity.Dense;
        }

        private static BlockParams ParamsFor(BlockDensity d, CityscapeProfile p)
        {
            switch (d)
            {
                case BlockDensity.Open:
                    return new BlockParams { MaxDepth = 0, SplitChance = 0f,
                        BuildingProb = 0.30f, HeightScale = 0.6f };
                case BlockDensity.Dense:
                    return new BlockParams { MaxDepth = p.maxLotSplitDepth + 1,
                        SplitChance = Mathf.Clamp01(p.splitChance + 0.2f),
                        BuildingProb = Mathf.Clamp01(p.buildingProbability + 0.15f),
                        HeightScale = 1.4f };
                default:
                    return new BlockParams { MaxDepth = p.maxLotSplitDepth,
                        SplitChance = p.splitChance,
                        BuildingProb = p.buildingProbability,
                        HeightScale = 1.0f };
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
            bool isBuilding = ctx.Rng.NextDouble() < prm.BuildingProb;

            if (isBuilding)
            {
                float t = Mathf.Pow((float)ctx.Rng.NextDouble(), BuildingHeightPower);
                float h = Mathf.Lerp(p.buildingHeightMin, p.buildingHeightMax, t) * prm.HeightScale;

                float fp = p.buildingFootprint;
                float sx = rect.width  * fp;
                float sz = rect.height * fp;
                float yaw = (float)Lerp(-BuildingYawJitterDeg, BuildingYawJitterDeg, ctx.Rng.NextDouble());

                SpawnCube(ctx, ctx.Assets.buildingCubePrefab, "Building",
                    new Vector3(rect.center.x, h * 0.5f, rect.center.y),
                    new Vector3(sx, h, sz), yaw);
                ctx.Metrics.BuildingCubeCount++;
            }
            else
            {
                SpawnCube(ctx, ctx.Assets.planterCubePrefab, "Planter",
                    new Vector3(rect.center.x, p.planterHeight * 0.5f, rect.center.y),
                    new Vector3(rect.width * 0.95f, p.planterHeight, rect.height * 0.95f), 0f);
                ctx.Metrics.PlanterCubeCount++;

                int targetCount = Mathf.RoundToInt(rect.width * rect.height * p.grassDensityPerSqm);
                for (int i = 0; i < targetCount; i++)
                {
                    float gx = rect.x + (float)ctx.Rng.NextDouble() * rect.width;
                    float gz = rect.y + (float)ctx.Rng.NextDouble() * rect.height;
                    float gr = (float)Lerp(p.grassRadiusMin, p.grassRadiusMax, ctx.Rng.NextDouble());

                    SpawnSphere(ctx, ctx.Assets.grassSpherePrefab, "Grass",
                        new Vector3(gx, p.planterHeight + gr * 0.5f, gz),
                        Vector3.one * gr * 2f);
                    ctx.Metrics.GrassSphereCount++;
                }
            }
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
