using UnityEngine;

namespace Proto.Camera
{
    public interface IFramingTarget
    {
        Vector3 Position { get; }
        float Weight { get; }
    }
}
