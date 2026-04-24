using System.Collections.Generic;
using Proto.Stage.PCG.Wfc;

namespace Proto.TD.Level.Wfc
{
    /// <summary>
    /// 기본 TD 타일셋 빌더. 순수 C#. 테스트는 이 빌더로 타일셋을 만들어
    /// Unity SO 없이 솔버를 검증할 수 있다.
    /// 소켓은 2종으로 단순화: "road" 와 "open". "open" 은 grass/building/void/edge 등
    /// 비도로 면 전부. 같은 ID 끼리만 매칭.
    /// 가중치 0 타일은 fixed-cell 전용(관측 대상에서 제외).
    /// </summary>
    public static class TdDefaultTileSet
    {
        public const string Road = "road";
        public const string Open = "open";

        public const string T_RoadStraightNS = "road-straight-ns";
        public const string T_RoadStraightEW = "road-straight-ew";
        public const string T_RoadCornerNE = "road-corner-ne";
        public const string T_RoadT_N = "road-t-n";
        public const string T_RoadCross = "road-cross";
        public const string T_RoadEndN = "road-end-n";
        public const string T_GrassFlat = "grass-flat";
        public const string T_GrassBush = "grass-bush";
        public const string T_Sidewalk = "sidewalk";
        public const string T_Planter = "planter";
        public const string T_BuildingSmall = "building-small";
        public const string T_PowerPad = "power-pad";
        public const string T_TowerPad = "tower-pad";
        public const string T_CoreBase = "core-base";
        public const string T_BorderVoid = "border-void";

        public static WfcTileSetData Build()
        {
            var r = new Socket(Road);
            var o = new Socket(Open);
            var originals = new List<WfcTileData>
            {
                // road, path role
                new WfcTileData(T_RoadStraightNS, TileRole.Path, r, o, r, o, weight: 5, allowRotation: false),
                new WfcTileData(T_RoadStraightEW, TileRole.Path, o, r, o, r, weight: 5, allowRotation: false),
                new WfcTileData(T_RoadCornerNE,   TileRole.Path, r, r, o, o, weight: 3, allowRotation: true),
                // T-N: open on north (stem goes up), road on E, S, W
                new WfcTileData(T_RoadT_N,        TileRole.Path, o, r, r, r, weight: 2, allowRotation: true),
                new WfcTileData(T_RoadCross,      TileRole.Path, r, r, r, r, weight: 1, allowRotation: false),
                // End-N: road on north only
                new WfcTileData(T_RoadEndN,       TileRole.Path, r, o, o, o, weight: 1, allowRotation: true),
                // environment
                new WfcTileData(T_GrassFlat,      TileRole.Grass, o, o, o, o, weight: 10, allowRotation: false),
                new WfcTileData(T_GrassBush,      TileRole.Grass, o, o, o, o, weight: 2,  allowRotation: false),
                // sidewalk / planter 는 fixed-only (weight 0) — BlockLayoutPlanner 에서만 배치.
                new WfcTileData(T_Sidewalk,       TileRole.Grass, o, o, o, o, weight: 0, allowRotation: false),
                new WfcTileData(T_Planter,        TileRole.Grass, o, o, o, o, weight: 0, allowRotation: false),
                new WfcTileData(T_BuildingSmall,  TileRole.Building, o, o, o, o, weight: 4, allowRotation: false),
                // fixed-only (weight 0 → only appear via FixedCells)
                new WfcTileData(T_PowerPad,       TileRole.PowerPad, o, o, o, o, weight: 0, allowRotation: false),
                new WfcTileData(T_TowerPad,       TileRole.TowerPad, o, o, o, o, weight: 0, allowRotation: false),
                new WfcTileData(T_CoreBase,       TileRole.CoreBase, r, r, r, r, weight: 0, allowRotation: false),
                new WfcTileData(T_BorderVoid,     TileRole.Border, o, o, o, o, weight: 0, allowRotation: false),
            };
            return WfcTileSetData.BuildWithRotations(originals, T_BorderVoid, T_GrassFlat);
        }
    }
}
