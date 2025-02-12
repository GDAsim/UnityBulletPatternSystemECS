using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[DisableAutoCreation]
public partial struct AmmoSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //DoJobs(ref state, ref ecb, ref localTransformLU);
    }

    void DoJobs(ref SystemState state,
        ref EntityCommandBuffer ecb,
        ref ComponentLookup<LocalTransform> localTransformLU)
    {
        localTransformLU.Update(ref state);

        new AmmmoJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,

            Ecb = ecb,

        }.Schedule();
    }
}

public partial struct AmmmoJob : IJobEntity
{
    public float DeltaTime;

    public EntityCommandBuffer Ecb;

    public ComponentLookup<LocalTransform> localTransformLU;

    void Execute(AmmoData data, ref LocalTransform localTransform)
    {
        if (data.Patterns == null || data.Patterns.Length == 0)
        {
            return;
        }

        data.CurrentActionTimer += DeltaTime;

        // DoAction
        bool DoAction;
        switch (data.CurrentActionType)
        {
            case ActionTypes.TransformAction:
                data.CurrentTransformAction.DoAction(DeltaTime);
                DoAction = data.CurrentActionTimer >= data.CurrentTransformAction.Duration;
                break;
            case ActionTypes.DelayAction:
                data.CurrentDelayAction.DoAction();
                DoAction = data.CurrentActionTimer >= data.CurrentDelayAction.Duration;
                break;
            case ActionTypes.SplitAction:
                data.CurrentSplitAction.DoAction();
                DoAction = true;
                break;
            default:
                throw new System.Exception("Not Implemented");
        }

        if (DoAction)
        {
            // EndAction();
            switch (data.CurrentActionType)
            {
                case ActionTypes.TransformAction:
                    data.CurrentTransformAction.EndAction();
                    break;
                case ActionTypes.DelayAction:
                    //data.CurrentDelayAction.EndAction();
                    break;
                case ActionTypes.SplitAction:
                    //data.CurrentSplitAction.EndAction();
                    break;
            }

            // GetNextAction();
            switch (data.Patterns[data.CurrentIndex])
            {
                case TransformAction action:
                    data.CurrentTransformAction = action;
                    data.CurrentActionType = ActionTypes.TransformAction;
                    break;
                case DelayAction action:
                    //data.CurrentDelayAction = action;
                    //data.CurrentActionType = ActionTypes.DelayAction;
                    break;
                case SplitAction action:
                    //data.CurrentSplitAction = action;
                    //data.CurrentActionType = ActionTypes.SplitAction;
                    break;
            }

            if (++data.CurrentIndex == data.Patterns.Length) data.CurrentIndex = 0;

            // ReadyAction();
            switch (data.CurrentActionType)
            {
                case ActionTypes.TransformAction:
                    data.CurrentTransformAction.ReadyAction(transform);
                    break;
                case ActionTypes.DelayAction:
                    //data.CurrentDelayAction.ReadyAction();
                    break;
                case ActionTypes.SplitAction:
                    //data.CurrentSplitAction.ReadyAction(this);
                    break;
            }

            data.CurrentActionTimer = 0;
        }
    }
}