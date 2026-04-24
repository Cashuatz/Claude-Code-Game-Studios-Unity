using System.Collections.Generic;
using System.IO;
using Proto.Stage.PCG.Wfc;
using UnityEngine;

namespace Proto.TD.Level.Wfc
{
    /// <summary>
    /// WFC 결과를 Unity 씬에 가시화. v1 은 프리팹 미술 에셋 없이 색상별 큐브 placeholder 로 스폰.
    /// HR-1 준수 (Rigidbody 미부착). 컴포넌트는 static visual 만.
    /// 저장된 JSON 이 있으면 비교 검증 로그를 남긴다.
    /// </summary>
    public class TdWfcLevelSpawner : MonoBehaviour
    {
        [Header("Generation")]
        public int gridSize = 40;
        public string seedHex = "C0FFEE";
        public int laneCount = 2;
        public Vector2Int coreBase = new Vector2Int(20, 20);
        public Vector2Int[] powerSources =
        {
            new Vector2Int(5, 5),
            new Vector2Int(34, 34)
        };
        public double linkBudget = 100;

        [Header("Output")]
        public bool writeJsonToAssets = true;
        [Tooltip("상대 경로: Assets/Proto/Data/TD/Levels/")]
        public string jsonFileName = "Level_Wfc_Sample_Medium.json";

        [Header("Scene")]
        public Transform spawnRoot;
        public bool centerAtOrigin = true;
        public float cellSize = 1f;
        public bool autoGenerateOnStart = true;

        private void Start()
        {
            if (autoGenerateOnStart) Generate();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            ulong seed = ParseSeed(seedHex);
            var spec = new TdLevelWfcAdapter.Spec
            {
                GridSize = gridSize,
                Seed = seed,
                LaneCount = laneCount,
                CoreBase = new AStarGrid.Cell(coreBase.x, coreBase.y),
                PowerSources = ToAstarCells(powerSources),
                TileSet = null, // use TdDefaultTileSet
                LinkBudget = linkBudget,
                LevelId = $"level-wfc-{gridSize}-{seedHex.ToLowerInvariant()}"
            };

            var result = TdLevelWfcAdapter.Build(spec);
            if (!result.Success)
            {
                Debug.LogError($"[TdWfcLevelSpawner] generation failed: {result.WfcStatus} — {result.Reason} @ ({result.ContradictionX},{result.ContradictionY})");
                return;
            }
            Debug.Log($"[TdWfcLevelSpawner] generated grid {gridSize}x{gridSize}, {result.Iterations} iterations, {result.Paths.Count} lanes");

            if (writeJsonToAssets)
            {
                string dir = Path.Combine(Application.dataPath, "Proto/Data/TD/Levels");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, jsonFileName);
                File.WriteAllText(path, result.LevelJson);
                Debug.Log($"[TdWfcLevelSpawner] wrote {path}");
            }

            Spawn(result.Grid);
        }

        private void Spawn(string[,] grid)
        {
            if (spawnRoot == null)
            {
                var go = new GameObject("TdLevelRoot");
                go.transform.SetParent(transform, false);
                spawnRoot = go.transform;
            }
            else
            {
                ClearChildren(spawnRoot);
            }

            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            Vector3 origin = centerAtOrigin
                ? new Vector3(-(w - 1) * 0.5f * cellSize, 0, -(h - 1) * 0.5f * cellSize)
                : Vector3.zero;

            var materialCache = new Dictionary<string, Material>();
            var baseShader = Shader.Find("Universal Render Pipeline/Lit");
            if (baseShader == null) baseShader = Shader.Find("Standard");

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    string tileId = grid[x, y];
                    if (string.IsNullOrEmpty(tileId)) continue;

                    float yHeight = HeightForTile(tileId);
                    Color col = ColorForTile(tileId);
                    if (!materialCache.TryGetValue(tileId, out var mat))
                    {
                        mat = new Material(baseShader);
                        mat.color = col;
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                        materialCache[tileId] = mat;
                    }

                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var col3d = cube.GetComponent<Collider>();
                    if (col3d != null) DestroyImmediate(col3d);
                    cube.name = $"Tile_{x}_{y}_{tileId}";
                    cube.transform.SetParent(spawnRoot, false);
                    cube.transform.localPosition = origin + new Vector3(x * cellSize, yHeight * 0.5f, y * cellSize);
                    cube.transform.localScale = new Vector3(cellSize * 0.98f, yHeight, cellSize * 0.98f);
                    cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(c); else DestroyImmediate(c);
            }
        }

        private static Color ColorForTile(string tileId)
        {
            if (tileId.StartsWith("road-")) return new Color(0.38f, 0.38f, 0.41f);
            if (tileId == "sidewalk") return new Color(0.72f, 0.72f, 0.70f);
            if (tileId == "planter") return new Color(0.25f, 0.50f, 0.22f);
            if (tileId.StartsWith("grass-bush")) return new Color(0.20f, 0.55f, 0.25f);
            if (tileId.StartsWith("grass-")) return new Color(0.32f, 0.68f, 0.34f);
            if (tileId == "building-small") return new Color(0.65f, 0.55f, 0.45f);
            if (tileId == "power-pad") return new Color(0.25f, 0.55f, 0.95f);
            if (tileId == "tower-pad") return new Color(0.95f, 0.85f, 0.25f);
            if (tileId == "core-base") return new Color(0.95f, 0.25f, 0.25f);
            if (tileId == "border-void") return new Color(0.12f, 0.12f, 0.14f);
            return Color.magenta;
        }

        private static float HeightForTile(string tileId)
        {
            if (tileId == "building-small") return 3.5f;
            if (tileId == "grass-bush") return 0.55f;
            if (tileId == "planter") return 0.5f;
            if (tileId == "sidewalk") return 0.28f;
            if (tileId == "core-base") return 2.0f;
            if (tileId == "power-pad") return 1.0f;
            if (tileId == "tower-pad") return 0.6f;
            if (tileId == "border-void") return 0.2f;
            // road tiles a touch lower than grass/sidewalk so roads look "worn in"
            if (tileId.StartsWith("road-")) return 0.18f;
            return 0.3f;
        }

        private static ulong ParseSeed(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return 1UL;
            hex = hex.Trim();
            if (hex.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
                hex = hex.Substring(2);
            if (ulong.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            return 1UL;
        }

        private static List<AStarGrid.Cell> ToAstarCells(Vector2Int[] pts)
        {
            var list = new List<AStarGrid.Cell>();
            if (pts != null)
            {
                for (int i = 0; i < pts.Length; i++)
                    list.Add(new AStarGrid.Cell(pts[i].x, pts[i].y));
            }
            return list;
        }
    }
}
