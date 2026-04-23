using System.Text;
using UnityEngine;

namespace Proto.Stage.PCG
{
    public struct CityscapeMetrics
    {
        public int Seed;
        public double GenerateMs;
        public int BlockCount;
        public int LotCount;
        public int RoadQuadCount;
        public int PaintQuadCount;
        public int CurbCubeCount;
        public int BuildingCubeCount;
        public int PlanterCubeCount;
        public int GrassSphereCount;
        public int TotalGoCount;
        public Vector3 BoundsSize;

        public string ToReport()
        {
            var sb = new StringBuilder(256);
            sb.AppendLine($"Seed       : {Seed}");
            sb.AppendLine($"Time       : {GenerateMs:F2} ms");
            sb.AppendLine($"Blocks/Lots: {BlockCount} / {LotCount}");
            sb.AppendLine($"Roads      : {RoadQuadCount}   Paints: {PaintQuadCount}");
            sb.AppendLine($"Curbs      : {CurbCubeCount}");
            sb.AppendLine($"Buildings  : {BuildingCubeCount}");
            sb.AppendLine($"Planters   : {PlanterCubeCount}");
            sb.AppendLine($"Grass      : {GrassSphereCount}");
            sb.AppendLine($"Total GOs  : {TotalGoCount}");
            sb.Append    ($"Bounds     : {BoundsSize.x:F1} x {BoundsSize.z:F1} m");
            return sb.ToString();
        }
    }
}
