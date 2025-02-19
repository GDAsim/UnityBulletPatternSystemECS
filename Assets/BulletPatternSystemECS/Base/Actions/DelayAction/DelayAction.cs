using System;
using UnityEngine;

/// <summary>
/// Action that does nothing but waste time
/// </summary>
public struct DelayAction : IAction
{
    public bool UseDelayUntil;
    public float Duration;

    #region Interface
    float IAction.Duration { get => Duration; }
    float IAction.ActionSpeed { get => 1; }
    float IAction.StartTimer { get => 0; }
    #endregion

    public void ReadyAction() { }
    public void DoAction(bool DelayUntil)
    {
        if (!UseDelayUntil) return;

        if (DelayUntil)
        {
            Duration = 0;
        }
        else
        {
            Duration = Mathf.Infinity;
        }
    }

    public void EndAction() { }
}
