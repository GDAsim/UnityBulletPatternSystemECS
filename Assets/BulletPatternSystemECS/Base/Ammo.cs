using System;
using Unity.Entities;
using UnityEngine;
using static GunData;

public class Ammo : MonoBehaviour
{
    public IAction[] patterns { get; private set; }
    int currentIndex;

    ActionTypes currentActionType;
    TransformAction currentTransformAction;
    DelayAction currentDelayAction;
    SplitAction currentSplitAction;

    float currentActionTimer;

    public void Setup(Ammo ammo)
    {
        patterns = ammo.patterns;

        currentIndex = ammo.currentIndex;
        currentActionTimer = ammo.currentActionTimer;

        GetNextAction();

        ReadyAction();
    }
    public void Setup(IAction[] patterns)
    {
        this.patterns = patterns;

        currentIndex = 0;
        currentActionTimer = 0;

        GetNextAction();

        ReadyAction();
    }

    void Update()
    {
        if (patterns == null || patterns.Length == 0)
        {
            //Debug.LogWarning("No Pattern Set", this);
            return;
        }

        currentActionTimer += Time.deltaTime;

        if (DoAction())
        {
            EndAction();

            GetNextAction();

            ReadyAction();

            currentActionTimer = 0;
        }
    }

    void GetNextAction()
    {
        switch (patterns[currentIndex])
        {
            case TransformAction action:
                currentTransformAction = action;
                currentActionType = ActionTypes.TransformAction;
                break;
            case DelayAction action:
                currentDelayAction = action;
                currentActionType = ActionTypes.DelayAction;
                break;
            case SplitAction action:
                currentSplitAction = action;
                currentActionType = ActionTypes.SplitAction;
                break;
        }

        if (++currentIndex == patterns.Length) currentIndex = 0;
    }

    void ReadyAction()
    {
        switch (currentActionType)
        {
            case ActionTypes.TransformAction:
                //currentTransformAction.ReadyAction(transform);
                return;
            case ActionTypes.DelayAction:
                currentDelayAction.ReadyAction();
                return;
            case ActionTypes.SplitAction:
                currentSplitAction.ReadyAction(this);
                return;
        }
    }
    bool DoAction()
    {
        switch (currentActionType)
        {
            case ActionTypes.TransformAction:
                //currentTransformAction.DoAction(Time.deltaTime);
                return currentActionTimer >= currentTransformAction.Duration;
            case ActionTypes.DelayAction:
                currentDelayAction.DoAction();
                return currentActionTimer >= currentDelayAction.Duration;
            case ActionTypes.SplitAction:
                currentSplitAction.DoAction();
                return true;
            default:
                throw new System.Exception("Not Implemented");
        }
    }
    void EndAction()
    {
        switch (currentActionType)
        {
            case ActionTypes.TransformAction:
                //currentTransformAction.EndAction();
                return;
            case ActionTypes.DelayAction:
                //currentDelayAction.EndAction();
                return;
            case ActionTypes.SplitAction:
                //currentSplitAction.EndAction();
                return;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }




    
}

public class AmmoData : IComponentData
{
    public IAction[] Patterns;
    public int CurrentIndex;

    public ActionTypes CurrentActionType;
    public TransformAction CurrentTransformAction;
    public DelayAction CurrentDelayAction;
    public SplitAction CurrentSplitAction;

    public float CurrentActionTimer;

    public static IAction[] GetPattern(GunPatternSelect select, float power)
    {
        switch (select)
        {
            case GunPatternSelect.One:
                return BulletPatterns.Straight(power);
            case GunPatternSelect.Two:
                return BulletPatterns.Sine(power, Vector3.left, 1f);
            case GunPatternSelect.Three:
                var pattern = new IAction[]
                {
                    new TransformAction
                    {
                        Duration = MathF.PI * 2,
                        StartTime = 0,

                        Action = SineMove,
                        ActionSpeed = power,
                        IsDeltaAction = true
                    },
                    new TransformAction
                    {
                        Duration = 1,
                        StartTime = 0,

                        Action = TransformAction.MoveForward,
                        ActionSpeed = power,
                        IsDeltaAction = true,
                    }
                };
                return pattern;
            case GunPatternSelect.Four:
                return BulletPatterns.Straight(power);
            default:
                return BulletPatterns.Straight(power);
        }
    }

    static TransformData SineMove(TransformData startData, float speed, float time)
    {
        var SineUpDown = Vector3.right * Mathf.Sin(time) * 1 * speed;
        var forward = startData.Rotation * Vector3.forward * (speed * time);

        startData.Position = startData.Position + forward + SineUpDown;

        return startData;
    }
}
public struct AmmoInit : IComponentData { }