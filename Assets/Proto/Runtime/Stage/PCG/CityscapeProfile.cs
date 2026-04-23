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
        [Tooltip("블록 한 변 길이 (도로 중심선 간 간격)")]
        public float blockSize = 30f;
        [Tooltip("도로 폭")]
        public float roadWidth = 6f;
        [Tooltip("인도 폭 (도로와 lot 사이)")]
        public float sidewalkWidth = 2f;
        [Tooltip("연석 높이")]
        public float curbHeight = 0.15f;

        [Header("Lot Subdivision")]
        [Range(0, 5)] public int maxLotSplitDepth = 2;
        [Tooltip("재분할 시도할 lot 의 최소 변 길이")]
        public float minLotEdge = 6f;
        [Range(0f, 1f)] public float splitChance = 0.65f;

        [Header("Building")]
        [Range(0f, 1f)] public float buildingProbability = 0.7f;
        public float buildingHeightMin = 4f;
        public float buildingHeightMax = 18f;
        [Range(0.5f, 1.0f)] public float buildingFootprint = 0.85f;

        [Header("Planter / Grass")]
        public float planterHeight = 0.2f;
        [Tooltip("화단 위 풀 밀도 (개/㎡)")]
        public float grassDensityPerSqm = 0.3f;
        public float grassRadiusMin = 0.2f;
        public float grassRadiusMax = 0.5f;

        [Header("Paint (Lane)")]
        [Tooltip("도로 중앙 차선 페인트 너비")]
        public float laneStripeWidth = 0.15f;
        [Tooltip("페인트 표면 Y 오프셋 (z-fight 방지)")]
        public float paintYOffset = 0.005f;
    }
}
