using System.Collections.Generic;
using UnityEngine;
using UCamera = UnityEngine.Camera;
using ULight = UnityEngine.Light;

namespace Proto.Shared.ShaderFx
{
    /// <summary>
    /// 6개 + 디졸브3종 = 8개 큐브를 가로로 배치, 각각 하나의 레시피를 계속 순환 시연.
    /// Scene 에 없으면 자동 생성, 조명/카메라도 보조 배치.
    /// </summary>
    public class ShaderFxDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private float cubeSpacing = 1.8f;
        [SerializeField] private float oscillationPeriod = 2.5f;
        [SerializeField] private bool rotateCubes = true;

        private static readonly ShaderFxRecipe[] Recipes =
        {
            ShaderFxRecipe.DissolveIn,
            ShaderFxRecipe.DissolveOut,
            ShaderFxRecipe.DissolveEdgeGlow,
            ShaderFxRecipe.RimLight,
            ShaderFxRecipe.Hologram,
            ShaderFxRecipe.Scanline,
            ShaderFxRecipe.HitFlash,
            ShaderFxRecipe.Outline,
        };

        private readonly List<Renderer> _renderers = new();

        private void Start()
        {
            EnsureCameraAndLight();
            BuildCubes();
        }

        private void Update()
        {
            if (_renderers.Count == 0) return;
            float phase = Mathf.Sin(Time.unscaledTime * (2f * Mathf.PI) / oscillationPeriod) * 0.5f + 0.5f;
            for (int i = 0; i < _renderers.Count; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;
                // 큐브마다 위상을 살짝 어긋나게
                float local = Mathf.Repeat(phase + i * 0.13f, 1f);
                ShaderFxHub.SetProgress(r, Recipes[i], local);
                if (rotateCubes)
                {
                    r.transform.Rotate(Vector3.up, 18f * Time.unscaledDeltaTime);
                }
            }
        }

        private void EnsureCameraAndLight()
        {
            if (UCamera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<UCamera>();
                cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.transform.position = new Vector3(0f, 2.2f, -8f);
                cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            }

            var lights = FindObjectsByType<ULight>(FindObjectsSortMode.None);
            bool hasDir = false;
            foreach (var l in lights) if (l.type == LightType.Directional) { hasDir = true; break; }
            if (!hasDir)
            {
                var lgo = new GameObject("Directional Light");
                var l = lgo.AddComponent<ULight>();
                l.type = LightType.Directional;
                l.color = new Color(1f, 0.96f, 0.9f, 1f);
                l.intensity = 1.2f;
                lgo.transform.rotation = Quaternion.Euler(45f, 35f, 0f);
            }
        }

        private void BuildCubes()
        {
            // 기존 자식 큐브 제거 (재시작 대비)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var c = transform.GetChild(i);
                if (c.name.StartsWith("FxCube_")) DestroyImmediate(c.gameObject);
            }

            float total = (Recipes.Length - 1) * cubeSpacing;
            float startX = -total * 0.5f;

            for (int i = 0; i < Recipes.Length; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"FxCube_{i:00}_{Recipes[i]}";
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(startX + i * cubeSpacing, 0f, 0f);
                // Collider 제거 (렌더만)
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);

                var r = go.GetComponent<Renderer>();
                _renderers.Add(r);

                // 초기 머티리얼 할당 (레시피 고정)
                r.sharedMaterial = ShaderFxHub.GetSharedMaterial(Recipes[i]);

                // 라벨
                CreateLabel(go.transform, Recipes[i].ToString());
            }
        }

        private static void CreateLabel(Transform parent, string text)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.06f;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.9f, 0.9f, 0.95f, 1f);

            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

    }
}
