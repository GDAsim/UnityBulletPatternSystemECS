using System;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Action that does nothing but waste time
/// </summary>
public struct DelayAction : IAction
{
    public Func<bool> DelayUntil;

    public float Duration;

    #region Interface
    float IAction.Duration { get => Duration; }
    float IAction.ActionSpeed { get => 1; }
    float IAction.StartTimer { get => 0; }
    #endregion

    public void ReadyAction()
    {
        
    }

    public void DoAction()
    {
        if (DelayUntil != null)
        {
            if (DelayUntil() == true)
            {
                Duration = 0;
            }
            else
            {
                Duration = Mathf.Infinity;
            }
        }
    }

    public void EndAction(ref LocalTransform localTransform)
    {

    }
}
