using UnityEngine;

/// <summary>
/// Basic Transform Action
/// </summary>
public partial struct TransformAction
{
    public static TransformData MoveForward(TransformData startData, float speed, float time)
    {
        var forward = startData.Rotation * Vector3.forward * (speed * time);

        startData.Position = startData.Position + forward;

        return startData;
    }
}
