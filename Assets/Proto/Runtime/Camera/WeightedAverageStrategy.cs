using System.Collections.Generic;
using UnityEngine;

namespace Proto.Camera
{
    public sealed class WeightedAverageStrategy : IFramingStrategy
    {
        public (Vector3 anchor, float distance) Compute(
            IReadOnlyList<IFramingTarget> targets, CameraModeConfig modeConfig)
        {
            if (targets == null || targets.Count == 0 || modeConfig == null)
            {
                return (Vector3.zero, modeConfig != null ? modeConfig.minDistance : 5f);
            }

            Vector3 sum = Vector3.zero;
            float weightSum = 0f;
            for (int i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null) continue;
                float w = Mathf.Max(0f, t.Weight);
                if (w <= 0f) continue;
                sum += t.Position * w;
                weightSum += w;
            }

            Vector3 anchor = weightSum > 0f ? sum / weightSum : Vector3.zero;
            float distance = Mathf.Clamp(modeConfig.offset.magnitude, modeConfig.minDistance, modeConfig.maxDistance);
            return (anchor, distance);
        }
    }
}
