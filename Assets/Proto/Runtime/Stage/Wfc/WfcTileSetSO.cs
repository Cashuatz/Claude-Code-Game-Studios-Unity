using System.Collections.Generic;
using UnityEngine;

namespace Proto.Stage.PCG.Wfc
{
    /// <summary>
    /// 타일 묶음. 런타임에는 <see cref="ToData"/> 로 <see cref="WfcTileSetData"/> 빌드.
    /// 빈 상태(tiles==null/empty) 이면 null 반환 — 호출부가 자체 기본 tileset 을 주입한다.
    /// </summary>
    [CreateAssetMenu(menuName = "Proto/Stage/Wfc/TileSet", fileName = "Wfc_TileSet_New")]
    public class WfcTileSetSO : ScriptableObject
    {
        public List<WfcTileSO> tiles = new List<WfcTileSO>();
        public string borderTileId = "border-void";
        public string defaultGrassTileId = "grass-flat";

        public WfcTileSetData ToData()
        {
            if (tiles == null || tiles.Count == 0) return null;

            var list = new List<WfcTileData>();
            foreach (var t in tiles)
            {
                if (t == null) continue;
                list.Add(t.ToData());
            }
            if (list.Count == 0) return null;

            return WfcTileSetData.BuildWithRotations(list, borderTileId, defaultGrassTileId);
        }
    }
}
