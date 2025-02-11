using System;
using UnityEngine;

public struct TransformData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public TransformData(Transform transform)
    {
        transform.GetLocalPositionAndRotation(out Position, out Rotation);
        Scale = transform.localScale;
    }

    public void ApplyTo(Transform transform)
    {
        transform.localPosition = Position;
        transform.localRotation = Rotation;
        transform.localScale = Scale;
    }
    public void AddTo(Transform transform)
    {
        transform.localPosition += Position;
        transform.localRotation = Rotation * transform.localRotation;
        transform.localScale += Scale;
    }

    public static TransformData operator -(TransformData left, TransformData right)
    {
        left.Position -= right.Position;
        left.Rotation = left.Rotation * Quaternion.Inverse(right.Rotation);
        left.Scale -= right.Scale;
        return left;
    }
    public static TransformData operator +(TransformData left, TransformData right)
    {
        left.Position += right.Position;
        left.Rotation *= right.Rotation;
        left.Scale += right.Scale;
        return left;
    }
}