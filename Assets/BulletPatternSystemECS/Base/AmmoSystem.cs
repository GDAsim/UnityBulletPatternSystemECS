using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
public partial class AmmoSystem : SystemBase
{
    protected override void OnCreate() { }
    protected override void OnDestroy() { }
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        var DeltaTime = SystemAPI.Time.DeltaTime;

        Entities.WithName("AmmoUpdate")
            .ForEach((
                ref LocalTransform localTransform, in AmmoData ammoData) =>
            {
                if (ammoData.Patterns == null || ammoData.Patterns.Length == 0)
                {
                    return;
                }

                ammoData.CurrentActionTimer += DeltaTime;

                // DoAction
                bool DoAction;
                switch (ammoData.CurrentActionType)
                {
                    case ActionTypes.TransformAction:
                        ammoData.CurrentTransformAction.DoAction(DeltaTime, ref localTransform);
                        DoAction = ammoData.CurrentActionTimer >= ammoData.CurrentTransformAction.Duration;
                        break;
                    case ActionTypes.DelayAction:
                        ammoData.CurrentDelayAction.DoAction();
                        DoAction = ammoData.CurrentActionTimer >= ammoData.CurrentDelayAction.Duration;
                        break;
                    case ActionTypes.SplitAction:
                        ammoData.CurrentSplitAction.DoAction();
                        DoAction = true;
                        break;
                    default:
                        throw new System.Exception("Not Implemented");
                }

                if (DoAction)
                {
                    // EndAction();
                    switch (ammoData.CurrentActionType)
                    {
                        case ActionTypes.TransformAction:
                            ammoData.CurrentTransformAction.EndAction(ref localTransform);
                            break;
                        case ActionTypes.DelayAction:
                            //data.CurrentDelayAction.EndAction();
                            break;
                        case ActionTypes.SplitAction:
                            //data.CurrentSplitAction.EndAction();
                            break;
                    }

                    // GetNextAction();
                    switch (ammoData.Patterns[ammoData.CurrentIndex])
                    {
                        case TransformAction action:
                            ammoData.CurrentTransformAction = action;
                            ammoData.CurrentActionType = ActionTypes.TransformAction;
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

                    if (++ammoData.CurrentIndex == ammoData.Patterns.Length) ammoData.CurrentIndex = 0;

                    // ReadyAction();
                    switch (ammoData.CurrentActionType)
                    {
                        case ActionTypes.TransformAction:
                            ammoData.CurrentTransformAction.ReadyAction(localTransform);
                            break;
                        case ActionTypes.DelayAction:
                            //data.CurrentDelayAction.ReadyAction();
                            break;
                        case ActionTypes.SplitAction:
                            //data.CurrentSplitAction.ReadyAction(this);
                            break;
                    }

                    ammoData.CurrentActionTimer = 0;
                }
            })
            .WithoutBurst().Run();
    }
}