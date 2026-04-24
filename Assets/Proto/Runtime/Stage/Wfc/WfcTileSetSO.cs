using System.Collections.Generic;
using UnityEngine;

namespace Proto.Stage.PCG.Wfc
{
    /// <summary>
    /// 타일 묶음. 런타임에는 <see cref="ToData"/> 로 <see cref="WfcTileSetData"/> 빌드.
    /// 빈 상태(tiles==null/empty) 일 때 <c>useDefaultProcedural</c> 를 켜면
    /// <see cref="Proto.TD.Level.Wfc.TdDefaultTileSet.Build"/> 으로 대체.
    /// </summary>
    [CreateAssetMenu(menuName = "Proto/Stage/Wfc/TileSet", fileName = "Wfc_TileSet_New")]
    public class WfcTileSetSO : ScriptableObject
    {
        public List<WfcTileSO> tiles = new List<WfcTileSO>();
        public string borderTileId = "border-void";
        public string defaultGrassTileId = "grass-flat";

        [Tooltip("tiles 가 비었을 때 TdDefaultTileSet.Build() 을 호출해 대신 사용.")]
        public bool useDefaultProceduralIfEmpty = true;

        public WfcTileSetData ToData()
        {
            if ((tiles == null || tiles.Count == 0) && useDefaultProceduralIfEmpty)
                return Proto.TD.Level.Wfc.TdDefaultTileSet.Build();

            var list = new List<WfcTileData>();
            foreach (var t in tiles)
            {
                if (t == null) continue;
                list.Add(t.ToData());
            }
            if (list.Count == 0)
                return Proto.TD.Level.Wfc.TdDefaultTileSet.Build();

            return WfcTileSetData.BuildWithRotations(list, borderTileId, defaultGrassTileId);
        }
    }
}
