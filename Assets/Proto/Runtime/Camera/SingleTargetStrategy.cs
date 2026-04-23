using System.Collections.Generic;
using UnityEngine;

namespace Proto.Camera
{
    public sealed class SingleTargetStrategy : IFramingStrategy
    {
        public (Vector3 anchor, float distance) Compute(
            IReadOnlyList<IFramingTarget> targets, CameraModeConfig modeConfig)
        {
            if (targets == null || targets.Count == 0 || modeConfig == null)
            {
                return (Vector3.zero, modeConfig != null ? modeConfig.minDistance : 5f);
            }

            IFramingTarget best = null;
            float bestWeight = float.NegativeInfinity;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                if (t.Weight > bestWeight)
                {
                    bestWeight = t.Weight;
                    best = t;
                }
            }

            Vector3 anchor = best != null ? best.Position : Vector3.zero;
            float distance = Mathf.Clamp(modeConfig.offset.magnitude, modeConfig.minDistance, modeConfig.maxDistance);
            return (anchor, distance);
        }
    }
}
