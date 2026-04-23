using UnityEngine;

namespace Proto.Stage.PCG
{
    [CreateAssetMenu(menuName = "Proto/Stage/CityscapeProfile", fileName = "Cityscape_Default")]
    public class CityscapeProfile : ScriptableObject
    {
        [Header("City Area")]
        [Tooltip("전체 도시 영역 (미터, X / Z)")]
        public Vector2Int citySize = new Vector2Int(120, 120);

        [Header("Block Grid")]
        [Tooltip("블록 한 변 길이 (도로 중심선 간 기본 간격)")]
        public float blockSize = 30f;
        [Tooltip("인도 폭 (연석과 lot 사이)")]
        public float sidewalkWidth = 2f;
        [Tooltip("연석 높이")]
        public float curbHeight = 0.15f;

        [Header("Road Hierarchy — 도로 위계")]
        [Tooltip("Local (2차선) 도로 폭")]
        public float roadWidthLocal = 5f;
        [Tooltip("Collector (4차선) 도로 폭")]
        public float roadWidthCollector = 10f;
        [Tooltip("Arterial (8차선) 도로 폭")]
        public float roadWidthArterial = 20f;
        [Range(0f, 1f)] public float weightLocal     = 0.55f;
        [Range(0f, 1f)] public float weightCollector = 0.30f;
        [Range(0f, 1f)] public float weightArterial  = 0.15f;

        [Header("Grid Jitter")]
        [Tooltip("블록 간격 편차 비율 (0.2 = ±20%)")]
        [Range(0f, 0.5f)] public float blockSpacingJitter = 0.20f;

        [Header("Block Skip")]
        [Tooltip("블록을 완전히 비울 확률 (도로만 보임, 사거리 광장 효과)")]
        [Range(0f, 0.5f)] public float emptyBlockProbability = 0.15f;

        [Header("Lot Subdivision")]
        [Range(0, 5)] public int maxLotSplitDepth = 2;
        [Tooltip("재분할 시도할 lot 의 최소 변 길이")]
        public float minLotEdge = 6f;
        [Range(0f, 1f)] public float splitChance = 0.65f;

        [Header("Building")]
        public float buildingHeightMin = 4f;
        public float buildingHeightMax = 18f;
        [Range(0.5f, 1.0f)] public float buildingFootprint = 0.85f;

        [Header("Planter / Grass (Open 블록 공원용)")]
        public float planterHeight = 0.2f;
        [Tooltip("공원 블록 풀 밀도 (개/㎡)")]
        public float grassDensityPerSqm = 0.3f;
        public float grassRadiusMin = 0.2f;
        public float grassRadiusMax = 0.5f;

        [Header("Sidewalk Grass Strip (차도-인도 사이 녹지 띠)")]
        [Tooltip("연석 안쪽 풀 띠 밀도 (개/m)")]
        public float sidewalkGrassDensityPerM = 0.6f;
        [Tooltip("연석에서 인도 방향으로 들어간 거리")]
        public float sidewalkGrassOffset = 0.7f;
        [Tooltip("띠를 따라 수직 방향 위치 jitter (m)")]
        public float sidewalkGrassJitter = 0.3f;

        [Header("Paint (Lane)")]
        public float laneStripeWidth = 0.15f;
        public float paintYOffset = 0.005f;
    }
}
