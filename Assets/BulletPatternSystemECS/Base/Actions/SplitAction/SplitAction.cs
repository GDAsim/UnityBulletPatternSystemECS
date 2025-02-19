using HomingGun;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SplitAction : IAction
{
    public bool DestroyOnEnd;
    public bool IsCopy; // Copy Current State/Vars Over

    public Entity splitPrefab;
    public IAction[] splitActions;
    public TransformData splitDelta;

    Entity main;
    AmmoData mainAmmoData;

    #region Interface
    float IAction.Duration { get => 0; }
    float IAction.ActionSpeed { get => 1; }
    float IAction.StartTimer { get => 0; }
    #endregion

    public void ReadyAction(Entity main, AmmoData mainAmmoData)
    {
        this.main = main;
        this.mainAmmoData = mainAmmoData;

        if (splitPrefab == Entity.Null) throw new System.Exception();
        if (splitActions == null)
        {
            // Fix an error with action not being copyied over
            // Copy
            splitActions = new IAction[mainAmmoData.Patterns.Length];
            for (int i = 0; i < splitActions.Length; i++)
            {
                if (mainAmmoData.Patterns[i] is TransformAction)
                {
                    var action = ((TransformAction)mainAmmoData.Patterns[i]);

                    splitActions[i] = new TransformAction
                    {
                        Duration = action.Duration,
                        StartTime = action.StartTime,

                        Action = action.Action,
                        ActionSpeed = action.ActionSpeed,
                        IsDeltaAction = action.IsDeltaAction,
                    };
                }
                else if (mainAmmoData.Patterns[i] is SplitAction)
                {
                    var action = ((SplitAction)mainAmmoData.Patterns[i]);

                    splitActions[i] = new SplitAction
                    {
                        splitPrefab = action.splitPrefab,
                        splitDelta = action.splitDelta,

                        IsCopy = action.IsCopy,
                        DestroyOnEnd = action.DestroyOnEnd,
                    };
                }
            }
        }
        if (splitDelta.Rotation.Equals(default)) splitDelta.Rotation = Quaternion.identity;
    }

    public void DoAction(ref LocalTransform localTransform, ref EntityCommandBuffer ecb)
    {
        Entity ammoEntity = ecb.Instantiate(splitPrefab);

        var ammoTransform = new TransformData(localTransform) + splitDelta;
        ammoTransform.ToLocalTransform();

        ecb.SetComponent(ammoEntity, ammoTransform.ToLocalTransform());

        if (!IsCopy)
        {
            AmmoData ammoData = new()
            {
                Patterns = splitActions,

                CurrentIndex = 0,
                CurrentActionTimer = 0
            };

            ecb.AddComponent(ammoEntity, ammoData);
            ecb.AddComponent(ammoEntity, new AmmoInit());

            ecb.AddSharedComponent(ammoEntity, new AmmoDataShared() { FiredFrom = main });
            ecb.AddSharedComponent(ammoEntity, new HomingData());
            ecb.AddSharedComponent(ammoEntity, new DelayData());
        }
        else
        {
            AmmoData ammoData = new()
            {
                Patterns = splitActions,

                CurrentIndex = mainAmmoData.CurrentIndex,
                CurrentActionTimer = mainAmmoData.CurrentActionTimer
            };

            ecb.AddComponent(ammoEntity, ammoData);
            ecb.AddComponent(ammoEntity, new AmmoInit());

            ecb.AddSharedComponent(ammoEntity, new AmmoDataShared() { FiredFrom = main });
            ecb.AddSharedComponent(ammoEntity, new HomingData());
            ecb.AddSharedComponent(ammoEntity, new DelayData());
        }
    }

    public void EndAction(ref EntityCommandBuffer ecb)
    {
        if (DestroyOnEnd)
        {
            ecb.DestroyEntity(main);
        }
    }
}
