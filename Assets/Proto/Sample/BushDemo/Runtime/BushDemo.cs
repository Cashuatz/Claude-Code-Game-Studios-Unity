using UnityEngine;
using UCamera = UnityEngine.Camera;
using ULight = UnityEngine.Light;

namespace Proto.Environment.Bush
{
    /// <summary>
    /// 부쉬 빌보드 시각 검증 씬 셋업.
    /// 좌측: 일반 Lit 구 (비교군)
    /// 우측 3개: quad 수 다른 BushBillboard (20 / 50 / 100)
    /// </summary>
    public class BushDemo : MonoBehaviour
    {
        [SerializeField] private int[] quadCounts = { 20, 50, 100 };
        [SerializeField] private float spacing = 2.2f;

        private void Start()
        {
            EnsureCameraAndLight();
            BuildGround();
            BuildRow();
        }

        private void Update()
        {
            // 카메라를 부드럽게 회전 — 빌보드 유지 확인
            var cam = UCamera.main;
            if (cam != null)
            {
                float t = Time.unscaledTime * 0.25f;
                float radius = 5.5f;
                cam.transform.position = new Vector3(Mathf.Sin(t) * radius, 1.8f, -Mathf.Cos(t) * radius);
                cam.transform.LookAt(new Vector3(0f, 0.7f, 0f));
            }
        }

        private void EnsureCameraAndLight()
        {
            if (UCamera.main == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                var cam = go.AddComponent<UCamera>();
                cam.backgroundColor = new Color(0.55f, 0.72f, 0.85f, 1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.transform.position = new Vector3(0f, 2f, -5.5f);
                cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            }

            var lights = FindObjectsByType<ULight>(FindObjectsSortMode.None);
            bool hasDir = false;
            foreach (var l in lights) if (l.type == LightType.Directional) { hasDir = true; break; }
            if (!hasDir)
            {
                var lgo = new GameObject("Directional Light");
                var l = lgo.AddComponent<ULight>();
                l.type = LightType.Directional;
                l.color = new Color(1f, 0.97f, 0.88f, 1f);
                l.intensity = 1.25f;
                lgo.transform.rotation = Quaternion.Euler(55f, 35f, 0f);
            }
        }

        private void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            var r = ground.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.32f, 0.34f, 0.25f, 1f));
            r.sharedMaterial = mat;
        }

        private void BuildRow()
        {
            // 비교군: 일반 Lit 구
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Sphere_Reference_Lit";
            sphere.transform.SetParent(transform, false);
            sphere.transform.localPosition = new Vector3(-spacing * 1.5f, 0.9f, 0f);
            Destroy(sphere.GetComponent<Collider>());
            var sr = sphere.GetComponent<Renderer>();
            var sm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sm.SetColor("_BaseColor", new Color(0.35f, 0.55f, 0.22f, 1f));
            sr.sharedMaterial = sm;

            AddLabel(sphere.transform, "Lit Sphere\n(reference)");

            // 3 개 부쉬 (quad count 차등)
            for (int i = 0; i < quadCounts.Length; i++)
            {
                var go = new GameObject($"Bush_q{quadCounts[i]}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(-spacing * 0.5f + i * spacing, 0.9f, 0f);

                var bush = go.AddComponent<BushBillboard>();
                bush.quadCount = quadCounts[i];
                bush.sphereRadius = 0.9f;
                bush.quadSize = Mathf.Lerp(0.75f, 0.45f, i / (float)(quadCounts.Length - 1));
                bush.Build();

                AddLabel(go.transform, $"quads={quadCounts[i]}");
            }
        }

        private static void AddLabel(Transform parent, string text)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 1.4f, 0f);

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.06f;
            tm.fontSize = 48;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            // 라벨은 카메라를 바라보게 (간이 빌보드)
            go.AddComponent<FaceCameraLabel>();

            var mr = go.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        private class FaceCameraLabel : MonoBehaviour
        {
            private void LateUpdate()
            {
                var cam = UCamera.main;
                if (cam == null) return;
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
        }
    }
}
