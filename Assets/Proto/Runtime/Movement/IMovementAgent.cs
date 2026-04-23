using System;
using UnityEngine;

namespace Proto.Movement
{
    public interface IMovementAgent
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        MovementProfile Profile { get; }

        MoveResult MoveBy(Vector3 delta);
        MoveResult MoveTo(Vector3 worldTarget);
        MoveResult Teleport(Vector3 worldTarget);
        void Halt();

        event Action<Vector3> Moved;
        event Action<Collider> Collided;
    }
}
