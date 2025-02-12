using System;
using Unity.Mathematics;
using Unity.Transforms;
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

    public TransformData(LocalTransform transform)
    {
        Position = transform.Position;
        Rotation = transform.Rotation;
        Scale = Vector3.one * transform.Scale;
    }

    public void ApplyTo(ref LocalTransform transform)
    {
        transform.Position = Position;
        transform.Rotation = Rotation;
        transform.Scale = Scale.x;
    }
    public void AddTo(ref LocalTransform transform)
    {
        transform.Position += (float3)Position;
        transform.Rotation = Rotation * transform.Rotation;
        transform.Scale += Scale.x;
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