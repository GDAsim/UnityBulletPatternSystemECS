using System;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct TransformWithEntitiesAction : IAction
{
    public Func<TransformData, float, float, LocalToWorld[], TransformData> Action;

    /// <summary>
    /// Used when the Performing Certain Action has a StartTimer != 0
    /// </summary>
    public bool IsDeltaAction;

    TransformData startData;
    TransformData prevData;
    float timer;

    public float Duration;
    public float ActionSpeed;
    public float StartTime;

    #region Interface
    float IAction.Duration { get => Duration; }
    float IAction.ActionSpeed { get => ActionSpeed; }
    float IAction.StartTimer { get => StartTime; }
    #endregion

    /// <summary>
    /// Prep Function - Call Once Before Update
    /// </summary>
    public void ReadyAction(LocalTransform transform, LocalToWorld[] entities)
    {
        if(transform.Position.x == float.NaN)
        {

        }
        startData = new(transform);

        timer = StartTime;

        prevData = Action.Invoke(startData, ActionSpeed, StartTime, entities);
    }

    /// <summary>
    /// Update Function
    /// Delta time is required in case for a custom time implementation
    /// </summary>
    public void DoAction(float deltatime, ref LocalTransform localTransform, LocalToWorld[] entities)
    {
        timer += deltatime;

        var transformData = Action.Invoke(startData, ActionSpeed, timer, entities);

        if (IsDeltaAction)
        {
            var delta = transformData - prevData;
            delta.AddTo(ref localTransform);
        }
        else
        {
            transformData.ApplyTo(ref localTransform);
        }

        prevData = transformData;
    }

    public void EndAction(ref LocalTransform localTransform, LocalToWorld[] entities)
    {
        if (Action == null) return;

        var transformData = Action.Invoke(startData, ActionSpeed, StartTime + Duration, entities);

        if (IsDeltaAction)
        {
            var delta = transformData - prevData;
            delta.AddTo(ref localTransform);
        }
        else
        {
            transformData.ApplyTo(ref localTransform);
        }
    }
}