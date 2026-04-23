using UnityEngine;

namespace Proto.Environment.Bush
{
    /// <summary>
    /// 부쉬 빌보드 구(球) 프로시저.
    ///  - 피보나치 구 분포로 N 개 쿼드 센터 생성.
    ///  - 각 쿼드는 4 버텍스가 센터에 겹쳐 있고, UV0 은 (0,0)~(1,1) 풀 셀, UV1 은 로컬 2D 오프셋.
    ///  - NORMAL 은 (센터 - 원점).normalized — 구 노멀 보존.
    ///  - 셰이더 (Proto/Env/BushBillboard) 가 UV1 을 받아 카메라 right/up 으로 확장하여 빌보드화.
    ///  - 라이팅은 원본 구 노멀로 계산 → 매끈한 볼륨감.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BushBillboard : MonoBehaviour
    {
        [Header("Shape")]
        [Range(4, 256)] public int quadCount = 48;
        [Min(0.05f)] public float sphereRadius = 0.9f;
        [Min(0.05f)] public float quadSize = 0.5f;

        [Header("Material (optional — 비우면 자동 생성)")]
        public Material materialOverride;
        public Color baseColor = new Color(0.35f, 0.55f, 0.22f, 1f);
        public Color ambientColor = new Color(0.22f, 0.28f, 0.18f, 1f);
        [Range(0f, 2f)] public float backlightIntensity = 0.35f;

        [Header("Rebuild on change (editor)")]
        [Tooltip("인스펙터에서 quadCount / sphereRadius 수정 시 메시 재빌드. runtime 에선 무관 (runtime 에는 매 프레임 파라미터 동기화 별도 수행).")]
        public bool rebuildOnValidate = true;

        private static readonly int IdQuadSize         = Shader.PropertyToID("_QuadSize");
        private static readonly int IdBaseColor        = Shader.PropertyToID("_BaseColor");
        private static readonly int IdAmbientColor     = Shader.PropertyToID("_AmbientColor");
        private static readonly int IdBacklight        = Shader.PropertyToID("_BacklightIntensity");
        private static readonly int IdBaseMap          = Shader.PropertyToID("_BaseMap");

        private static Texture2D s_sharedLeafTex;

        // 마지막으로 메시를 빌드한 파라미터 — 변경 감지용
        private int _lastQuadCount = -1;
        private float _lastSphereRadius = -1f;
        private Material _cachedMat;

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            // 메시 재빌드가 필요한 파라미터가 바뀌었으면 메시 다시 생성
            if (_lastQuadCount != quadCount || !Mathf.Approximately(_lastSphereRadius, sphereRadius))
            {
                Build();
                return;
            }
            // 머티리얼 파라미터는 매 프레임 push (quadSize / 색상 / 백라이트 조정 실시간 반영)
            PushMaterialParams();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!rebuildOnValidate) return;
            if (!isActiveAndEnabled) return;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Build();
            };
        }
#endif

        private void PushMaterialParams()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr == null) return;
            var mat = _cachedMat != null ? _cachedMat : mr.sharedMaterial;
            if (mat == null) return;
            mat.SetFloat(IdQuadSize, quadSize);
            mat.SetColor(IdBaseColor, baseColor);
            mat.SetColor(IdAmbientColor, ambientColor);
            mat.SetFloat(IdBacklight, backlightIntensity);
        }

        public void Build()
        {
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();

            var mesh = mf.sharedMesh;
            if (mesh == null || !mesh.name.StartsWith("BushBillboard_"))
            {
                mesh = new Mesh { name = $"BushBillboard_{quadCount}" };
                mf.sharedMesh = mesh;
            }
            else
            {
                mesh.Clear();
                mesh.name = $"BushBillboard_{quadCount}";
            }

            BuildBushMesh(mesh, quadCount, sphereRadius);

            // Material — 이미 _cachedMat 이 있으면 재사용해 누수 방지
            Material mat = materialOverride;
            if (mat == null) mat = _cachedMat;
            if (mat == null)
            {
                var sh = Shader.Find("Proto/Env/BushBillboard");
                if (sh == null)
                {
                    Debug.LogError("[BushBillboard] Shader 'Proto/Env/BushBillboard' not found.");
                    return;
                }
                mat = new Material(sh) { name = "M_Env_BushBillboard" };
            }
            if (mat.GetTexture(IdBaseMap) == null)
            {
                if (s_sharedLeafTex == null) s_sharedLeafTex = BuildLeafTexture(128);
                mat.SetTexture(IdBaseMap, s_sharedLeafTex);
            }
            mat.SetFloat(IdQuadSize, quadSize);
            mat.SetColor(IdBaseColor, baseColor);
            mat.SetColor(IdAmbientColor, ambientColor);
            mat.SetFloat(IdBacklight, backlightIntensity);
            mr.sharedMaterial = mat;
            _cachedMat = mat;

            _lastQuadCount = quadCount;
            _lastSphereRadius = sphereRadius;
        }

        /// <summary>쿼드 카드 N 개를 구 표면에 배치한 메시 생성.</summary>
        private static void BuildBushMesh(Mesh mesh, int quadCount, float radius)
        {
            quadCount = Mathf.Max(1, quadCount);
            var verts   = new Vector3[quadCount * 4];
            var norms   = new Vector3[quadCount * 4];
            var uv0     = new Vector2[quadCount * 4];
            var uv1     = new Vector2[quadCount * 4];  // 로컬 2D 오프셋
            var tris    = new int[quadCount * 6];

            const float goldenAngle = 2.399963229728f; // ≈ π(3 - √5)

            for (int i = 0; i < quadCount; i++)
            {
                // Fibonacci sphere point
                float y = 1f - (i / (float)Mathf.Max(1, quadCount - 1)) * 2f;
                float r = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float theta = i * goldenAngle;
                var dir = new Vector3(Mathf.Cos(theta) * r, y, Mathf.Sin(theta) * r);

                Vector3 center = dir * radius;
                Vector3 normal = dir; // 이미 유닛 벡터

                int v = i * 4;
                // 4 버텍스 모두 센터에 겹쳐놓고, UV1 로 코너 오프셋 지정
                verts[v + 0] = center; uv0[v + 0] = new Vector2(0f, 0f); uv1[v + 0] = new Vector2(-0.5f, -0.5f);
                verts[v + 1] = center; uv0[v + 1] = new Vector2(1f, 0f); uv1[v + 1] = new Vector2( 0.5f, -0.5f);
                verts[v + 2] = center; uv0[v + 2] = new Vector2(1f, 1f); uv1[v + 2] = new Vector2( 0.5f,  0.5f);
                verts[v + 3] = center; uv0[v + 3] = new Vector2(0f, 1f); uv1[v + 3] = new Vector2(-0.5f,  0.5f);

                for (int k = 0; k < 4; k++) norms[v + k] = normal; // 구 노멀 보존

                int t = i * 6;
                tris[t + 0] = v + 0; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
                tris[t + 3] = v + 0; tris[t + 4] = v + 3; tris[t + 5] = v + 2;
            }

            mesh.indexFormat = quadCount * 4 > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv1);
            mesh.SetTriangles(tris, 0);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius * 2f + 1f)); // 바운드 넉넉히
        }

        /// <summary>리프 클러스터 프로시저 텍스처 (RGBA). 알파는 방사 감쇠 + Perlin 노이즈 경계.</summary>
        public static Texture2D BuildLeafTexture(int N)
        {
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, true, false)
            {
                name = "T_Env_LeafCluster_Procedural",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 2,
            };
            var px = new Color32[N * N];
            float cx = N * 0.5f, cy = N * 0.5f;
            float rMax = N * 0.48f;

            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    float radial = 1f - Mathf.Clamp01(d / rMax);
                    float boundaryNoise = Mathf.PerlinNoise(x * 0.18f, y * 0.18f);
                    float microNoise    = Mathf.PerlinNoise(x * 0.55f + 100f, y * 0.55f + 100f);
                    float shape = radial * Mathf.Lerp(0.55f, 1.2f, boundaryNoise);

                    float a = Mathf.Clamp01((shape - 0.4f) * 2.5f);

                    // 초록 톤 변주
                    float g = Mathf.Lerp(0.45f, 0.85f, microNoise);
                    float r = g * Mathf.Lerp(0.35f, 0.55f, microNoise);
                    float b = g * Mathf.Lerp(0.20f, 0.35f, 1f - microNoise);

                    // 중앙부 약간 어둡게
                    float shade = Mathf.Lerp(0.7f, 1f, radial);
                    r *= shade; g *= shade; b *= shade;

                    px[y * N + x] = new Color32(
                        (byte)(Mathf.Clamp01(r) * 255f),
                        (byte)(Mathf.Clamp01(g) * 255f),
                        (byte)(Mathf.Clamp01(b) * 255f),
                        (byte)(a * 255f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply(true, false);
            return tex;
        }
    }
}
