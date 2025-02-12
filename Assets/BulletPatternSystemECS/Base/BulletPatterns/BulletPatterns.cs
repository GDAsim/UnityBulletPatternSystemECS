using System;
using UnityEngine;

/// <summary>
/// Basic bullet Patterns
/// </summary>
public static class BulletPatterns
{
    public static IAction[] Straight(float actionSpeed) => new IAction[1]
    {
        new TransformAction
        {
            Duration = 1111111,
            StartTime = 0,

            Action = TransformAction.MoveForward,
            ActionSpeed = actionSpeed,
            IsDeltaAction = true,
        }
    };
    public static IAction[] Sine(float actionSpeed, Vector3 axis, float height)
    {
        var actions = new IAction[1]
        {
            new TransformAction
            {
                Duration = MathF.PI * 2,
                StartTime = 0,

                Action = SineMove,
                ActionSpeed = actionSpeed,
                IsDeltaAction = true
            }
        };
            
        return actions;

        TransformData SineMove(TransformData startData, float speed, float time)
        {
            var SineUpDown = axis * Mathf.Sin(time) * height * speed;
            var forward = startData.Rotation * Vector3.forward * (speed * time);

            startData.Position = startData.Position + forward + SineUpDown;

            return startData;
        }
    }
}
