using UnityEngine;

namespace Proto.Stage.PCG.Wfc
{
    /// <summary>
    /// 디자이너가 타일을 에디터로 저작할 때 쓰는 SO 래퍼. 런타임에는 <see cref="WfcTileData"/> 로 변환.
    /// </summary>
    [CreateAssetMenu(menuName = "Proto/Stage/Wfc/Tile", fileName = "Wfc_Tile_New")]
    public class WfcTileSO : ScriptableObject
    {
        public string tileId = "new-tile";
        public TileRole role = TileRole.Grass;

        [Tooltip("렌더용 프리팹. 없어도 됨 (void / placeholder).")]
        public GameObject prefab;

        [Header("Sockets (4방향)")]
        public string socketN = "open";
        public string socketE = "open";
        public string socketS = "open";
        public string socketW = "open";

        [Min(0)] public int weight = 1;
        public bool allowRotation;

        public WfcTileData ToData()
        {
            return new WfcTileData(
                tileId: tileId,
                role: role,
                north: new Socket(socketN),
                east: new Socket(socketE),
                south: new Socket(socketS),
                west: new Socket(socketW),
                weight: weight,
                allowRotation: allowRotation);
        }
    }
}
