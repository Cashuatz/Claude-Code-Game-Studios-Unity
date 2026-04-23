using UnityEngine;

namespace Proto.Stage.PCG
{
    /// <summary>
    /// 시가지 PCG 가 인스턴스화할 프리팹 6종 세트.
    /// Generator 는 이 SO 의 프리팹을 그대로 Instantiate 하므로,
    /// 프리팹을 실제 에셋(빌딩 메시 등)으로 교체하면 재컴파일 없이 시각이 바뀐다.
    ///
    /// 각 프리팹의 기본 방향 전제:
    ///  - Quad 프리팹: Unity Primitive 기본(정면 -Z, rotation identity).
    ///    Generator 가 Euler(90,0,0) 로 눕혀 지면에 배치한다.
    ///  - Cube 프리팹: rotation identity. Generator 가 Y 축 jitter 를 부여.
    ///  - Sphere 프리팹: rotation identity. 회전 고정 없음.
    /// </summary>
    [CreateAssetMenu(menuName = "Proto/Stage/CityscapeAssetSet", fileName = "Cityscape_AssetSet_Default")]
    public class CityscapeAssetSet : ScriptableObject
    {
        [Header("Road Surfaces (Quad 권장, 평면 메시)")]
        public GameObject roadQuadPrefab;
        public GameObject paintQuadPrefab;

        [Header("Blocks (Cube 권장, 수직 매스)")]
        public GameObject curbCubePrefab;
        public GameObject buildingCubePrefab;
        public GameObject planterCubePrefab;

        [Header("Vegetation (Sphere 권장)")]
        public GameObject grassSpherePrefab;

        public bool IsComplete()
        {
            return roadQuadPrefab && paintQuadPrefab && curbCubePrefab &&
                   buildingCubePrefab && planterCubePrefab && grassSpherePrefab;
        }

        public string DescribeMissing()
        {
            var sb = new System.Text.StringBuilder();
            if (!roadQuadPrefab)     sb.Append("roadQuadPrefab ");
            if (!paintQuadPrefab)    sb.Append("paintQuadPrefab ");
            if (!curbCubePrefab)     sb.Append("curbCubePrefab ");
            if (!buildingCubePrefab) sb.Append("buildingCubePrefab ");
            if (!planterCubePrefab)  sb.Append("planterCubePrefab ");
            if (!grassSpherePrefab)  sb.Append("grassSpherePrefab ");
            return sb.Length == 0 ? "none" : sb.ToString().Trim();
        }
    }
}
