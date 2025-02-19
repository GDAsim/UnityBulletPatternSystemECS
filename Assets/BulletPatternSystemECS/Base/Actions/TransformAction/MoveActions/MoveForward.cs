using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Basic Transform Actions
/// </summary>
public partial struct TransformAction
{
    public static TransformData MoveForward(TransformData startData, float speed, float time)
    {
        var forward = math.mul(startData.Rotation, Vector3.forward) * (speed * time);

        startData.Position = startData.Position + forward;

        return startData;
    }
}
