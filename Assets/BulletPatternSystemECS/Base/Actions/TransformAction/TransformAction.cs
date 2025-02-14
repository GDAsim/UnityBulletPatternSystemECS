using System;
using Unity.Transforms;
using UnityEngine;

public partial struct TransformAction : IAction
{
    public Func<TransformData, float, float, TransformData> Action;
    
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
    float IAction.Duration { get => Duration;}
    float IAction.ActionSpeed { get => ActionSpeed; }
    float IAction.StartTimer { get => StartTime; }
    #endregion

    /// <summary>
    /// Prep Function - Call Once Before Update
    /// </summary>
    public void ReadyAction(LocalTransform transform)
    {
        startData = new(transform);

        timer = StartTime;

        prevData = Action.Invoke(startData, ActionSpeed, StartTime);
    }

    /// <summary>
    /// Update Function
    /// Delta time is required in case for a custom time implementation
    /// </summary>
    public void DoAction(float deltatime, ref LocalTransform localTransform)
    {
        timer += deltatime;

        var transformData = Action.Invoke(startData, ActionSpeed, timer);

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

    public void EndAction(ref LocalTransform localTransform)
    {
        if (Action == null) return;

        var transformData = Action.Invoke(startData, ActionSpeed, StartTime + Duration);

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