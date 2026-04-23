using System.Collections.Generic;
using UnityEngine;

namespace Proto.Camera
{
    public sealed class BoundingBoxStrategy : IFramingStrategy
    {
        public (Vector3 anchor, float distance) Compute(
            IReadOnlyList<IFramingTarget> targets, CameraModeConfig modeConfig)
        {
            if (targets == null || targets.Count == 0 || modeConfig == null)
            {
                return (Vector3.zero, modeConfig != null ? modeConfig.minDistance : 5f);
            }

            bool hasAny = false;
            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;

            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null || t.Weight <= 0f) continue;
                Vector3 p = t.Position;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
                hasAny = true;
            }

            if (!hasAny) return (Vector3.zero, modeConfig.minDistance);

            Vector3 anchor = (min + max) * 0.5f;
            Vector3 extents = max - min;
            float span = Mathf.Max(extents.x, extents.y, extents.z);

            float fovRad = modeConfig.fov * Mathf.Deg2Rad;
            float half = Mathf.Max(span * 0.5f, 0.1f);
            float requiredDistance = half / Mathf.Tan(fovRad * 0.5f);
            float distance = Mathf.Clamp(requiredDistance, modeConfig.minDistance, modeConfig.maxDistance);

            return (anchor, distance);
        }
    }
}
