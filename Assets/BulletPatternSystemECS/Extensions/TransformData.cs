using Unity.Mathematics;
using Unity.Transforms;

public struct TransformData
{
    public float3 Position;
    public quaternion Rotation;
    public float Scale;

    public TransformData(LocalTransform transform)
    {
        Position = transform.Position;
        Rotation = transform.Rotation;
        Scale = transform.Scale;
    }

    public void ApplyTo(ref LocalTransform transform)
    {
        transform.Position = Position;
        transform.Rotation = Rotation;
        transform.Scale = Scale;
    }
    public void AddTo(ref LocalTransform transform)
    {
        transform.Position += Position;
        transform.Rotation = math.mul(transform.Rotation, Rotation);
        transform.Scale += Scale;
    }
    public LocalTransform ToLocalTransform()
    {
        return LocalTransform.FromPositionRotationScale(Position, Rotation, Scale);
    }


    public static TransformData operator -(TransformData left, TransformData right)
    {
        return new TransformData
        {
            Position = left.Position - right.Position,
            Rotation = math.mul(left.Rotation, math.inverse(right.Rotation)),
            Scale = left.Scale - right.Scale
        };
    }
    public static TransformData operator +(TransformData left, TransformData right)
    {
        return new TransformData
        {
            Position = left.Position + right.Position,
            Rotation = math.mul(left.Rotation, right.Rotation),
            Scale = left.Scale + right.Scale
        };
    }
}