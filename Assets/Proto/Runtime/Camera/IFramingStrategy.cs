using System.Collections.Generic;
using UnityEngine;

namespace Proto.Camera
{
    public interface IFramingStrategy
    {
        (Vector3 anchor, float distance) Compute(
            IReadOnlyList<IFramingTarget> targets,
            CameraModeConfig modeConfig);
    }
}
