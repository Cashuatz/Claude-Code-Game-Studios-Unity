#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Proto.Environment.Bush;

namespace Proto.Editor.Environment
{
    /// <summary>
    /// 부쉬 빌보드 메시 / 리프 텍스처 에셋 베이커.
    ///
    /// - Tools > Proto > Bush > Bake Default Meshes  : 기본 3종 (q=20/50/100) 메시 + 리프 텍스처를 에셋으로 저장.
    /// - 컴포넌트 컨텍스트 메뉴에서 "Save Current Mesh As Asset..." 실행 시 현재 컴포넌트의 메시만 저장.
    /// </summary>
    public static class BushAssetBaker
    {
        private const string MeshDir = "Assets/Proto/VFX/Environment/Meshes";
        private const string TexDir  = "Assets/Proto/VFX/Environment/Textures";

        [MenuItem("Tools/Proto/Bush/Bake Default Meshes")]
        public static void BakeDefaultMeshes()
        {
            EnsureDir(MeshDir);
            EnsureDir(TexDir);

            BakeMesh(20,  0.9f);
            BakeMesh(50,  0.9f);
            BakeMesh(100, 0.9f);

            BakeLeafTexture(128);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BushAssetBaker] Default bush meshes + leaf texture baked.");
        }

        [MenuItem("CONTEXT/BushBillboard/Save Current Mesh As Asset...")]
        public static void SaveCurrentMeshAsset(MenuCommand command)
        {
            var bush = command.context as BushBillboard;
            if (bush == null) return;

            var mf = bush.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                EditorUtility.DisplayDialog("Save Mesh",
                    "No mesh found. Click 'Build' or enter Play mode once to generate a mesh first.",
                    "OK");
                return;
            }

            EnsureDir(MeshDir);
            string assetName = $"Bush_q{bush.quadCount}_r{bush.sphereRadius:F2}.asset";
            string path = $"{MeshDir}/{assetName}";

            // 중복 시 유니크 이름
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            // 원본 메시를 복제해 에셋으로 저장 (원본은 런타임 참조 유지)
            var saved = Object.Instantiate(mf.sharedMesh);
            saved.name = Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(saved, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 컴포넌트의 메시를 에셋 참조로 교체
            mf.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            EditorUtility.SetDirty(mf);

            Debug.Log($"[BushAssetBaker] Saved mesh asset: {path}");
            EditorGUIUtility.PingObject(mf.sharedMesh);
        }

        private static void BakeMesh(int quadCount, float radius)
        {
            var mesh = new Mesh { name = $"Bush_q{quadCount}" };
            BushBillboard.BuildBushMesh(mesh, quadCount, radius);

            string path = $"{MeshDir}/Bush_q{quadCount}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                // 기존 에셋 내용을 덮어쓰기 위해 복사 후 재저장
                existing.Clear();
                BushBillboard.BuildBushMesh(existing, quadCount, radius);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        private static void BakeLeafTexture(int size)
        {
            string path = $"{TexDir}/T_Env_LeafCluster.asset";
            var tex = BushBillboard.BuildLeafTexture(size);
            tex.name = "T_Env_LeafCluster";

            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
            AssetDatabase.CreateAsset(tex, path);
        }

        private static void EnsureDir(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir)) return;
            var parts = dir.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(cur, parts[i]);
                }
                cur = next;
            }
        }
    }
}
#endif
