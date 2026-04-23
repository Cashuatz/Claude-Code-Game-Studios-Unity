#if UNITY_EDITOR
using System.IO;
using Proto.Stage.PCG;
using UnityEditor;
using UnityEngine;

namespace Proto.EditorTools.Stage.PCG
{
    /// <summary>
    /// 시가지 PCG 기본 프리팹 6종 + 머티리얼 6종 + AssetSet SO 를 자동 생성.
    /// 프리팹은 Unity Primitive(Quad/Cube/Sphere) 에 카테고리 머티리얼을 바인딩한 그레이박싱 버전.
    /// 제작자는 프리팹을 **원하는 실 에셋으로 교체**하기만 하면 Generator 가 곧바로 새 시각을 사용한다.
    /// </summary>
    public static class CityscapeAssetBuilder
    {
        private const string ConfigRoot    = "Assets/Proto/Data/Config";
        private const string MaterialsRoot = "Assets/Proto/Materials/PCG";
        private const string PrefabsRoot   = "Assets/Proto/Prefabs/Stage/PCG";
        private const string AssetSetPath  = ConfigRoot + "/Cityscape_AssetSet_Default.asset";

        private static readonly Color ColorRoad     = new Color(0.18f, 0.18f, 0.20f);
        private static readonly Color ColorPaint    = new Color(0.95f, 0.85f, 0.20f);
        private static readonly Color ColorCurb     = new Color(0.55f, 0.55f, 0.55f);
        private static readonly Color ColorBuilding = new Color(0.78f, 0.74f, 0.66f);
        private static readonly Color ColorPlanter  = new Color(0.45f, 0.30f, 0.18f);
        private static readonly Color ColorGrass    = new Color(0.30f, 0.55f, 0.25f);

        [MenuItem("Proto/Stage/Cityscape — Build Default Assets", priority = 10)]
        public static CityscapeAssetSet BuildDefaults()
        {
            EnsureFolder(ConfigRoot);
            EnsureFolder(MaterialsRoot);
            EnsureFolder(PrefabsRoot);

            // 1) Materials
            var roadMat     = EnsureMaterial($"{MaterialsRoot}/Cityscape_Road_Mat.mat",     ColorRoad);
            var paintMat    = EnsureMaterial($"{MaterialsRoot}/Cityscape_Paint_Mat.mat",    ColorPaint);
            var curbMat     = EnsureMaterial($"{MaterialsRoot}/Cityscape_Curb_Mat.mat",     ColorCurb);
            var buildingMat = EnsureMaterial($"{MaterialsRoot}/Cityscape_Building_Mat.mat", ColorBuilding);
            var planterMat  = EnsureMaterial($"{MaterialsRoot}/Cityscape_Planter_Mat.mat",  ColorPlanter);
            var grassMat    = EnsureMaterial($"{MaterialsRoot}/Cityscape_Grass_Mat.mat",    ColorGrass);

            // 2) Prefabs (Unity Primitive + material, Collider 제거)
            var roadPrefab     = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Road_Quad.prefab",     PrimitiveType.Quad,   roadMat);
            var paintPrefab    = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Paint_Quad.prefab",    PrimitiveType.Quad,   paintMat);
            var curbPrefab     = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Curb_Cube.prefab",     PrimitiveType.Cube,   curbMat);
            var buildingPrefab = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Building_Cube.prefab", PrimitiveType.Cube,   buildingMat);
            var planterPrefab  = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Planter_Cube.prefab",  PrimitiveType.Cube,   planterMat);
            var grassPrefab    = EnsurePrimitivePrefab($"{PrefabsRoot}/PCG_Grass_Sphere.prefab",  PrimitiveType.Sphere, grassMat);

            // 3) AssetSet SO
            var set = AssetDatabase.LoadAssetAtPath<CityscapeAssetSet>(AssetSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<CityscapeAssetSet>();
                AssetDatabase.CreateAsset(set, AssetSetPath);
            }
            set.roadQuadPrefab     = roadPrefab;
            set.paintQuadPrefab    = paintPrefab;
            set.curbCubePrefab     = curbPrefab;
            set.buildingCubePrefab = buildingPrefab;
            set.planterCubePrefab  = planterPrefab;
            set.grassSpherePrefab  = grassPrefab;
            EditorUtility.SetDirty(set);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CityscapeAssetBuilder] Built {MaterialsRoot}(6), {PrefabsRoot}(6), {AssetSetPath}");
            return set;
        }

        public static CityscapeAssetSet LoadDefaultAssetSet()
            => AssetDatabase.LoadAssetAtPath<CityscapeAssetSet>(AssetSetPath);

        // ---------- helpers ----------

        private static Material EnsureMaterial(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject EnsurePrimitivePrefab(string path, PrimitiveType primitive, Material mat)
        {
            // 기존 프리팹이 있으면 머티리얼 참조만 갱신
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                var mr = existing.GetComponentInChildren<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != mat)
                {
                    mr.sharedMaterial = mat;
                    EditorUtility.SetDirty(existing);
                }
                return existing;
            }

            // 새 프리팹 생성
            var go = GameObject.CreatePrimitive(primitive);
            go.name = Path.GetFileNameWithoutExtension(path);

            // HR-1: Collider 제거 (그레이박싱 시각 전용)
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            var mrenderer = go.GetComponent<MeshRenderer>();
            if (mrenderer != null) mrenderer.sharedMaterial = mat;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
