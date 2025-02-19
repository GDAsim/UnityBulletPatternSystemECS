using HomingGun;
using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
public partial class AmmoSystem : SystemBase
{
    ComponentLookup<LocalTransform> localTransformLU;

    protected override void OnCreate()
    {
        localTransformLU = GetComponentLookup<LocalTransform>(true);
    }
    protected override void OnDestroy() { }
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        localTransformLU.Update(this);

        var DeltaTime = SystemAPI.Time.DeltaTime;

        Entities.WithName("AmmoUpdate")
            .ForEach((
                ref LocalTransform localTransform, in AmmoData ammoData, 
                in HomingData homingData, in DelayData delayData) =>
            {
                if (ammoData.Patterns == null || ammoData.Patterns.Length == 0)
                {
                    return;
                }

                LocalTransform homingTransform = default;
                if (localTransformLU.HasComponent(homingData.HomingEntity))
                {
                    homingTransform = localTransformLU[homingData.HomingEntity];
                }
                LocalTransform gunTransform = default;
                if (localTransformLU.HasComponent(homingData.GunEntity))
                {
                    gunTransform = localTransformLU[homingData.GunEntity];
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
                        ammoData.CurrentDelayAction.DoAction(delayData.DelayUntil);
                        DoAction = ammoData.CurrentActionTimer >= ammoData.CurrentDelayAction.Duration;
                        break;
                    case ActionTypes.TransformWithEntities:
                        ammoData.CurrentTransformWithEntitiesAction.DoAction(DeltaTime, ref localTransform, new LocalTransform[] { homingTransform, gunTransform });
                        DoAction = ammoData.CurrentActionTimer >= ammoData.CurrentTransformWithEntitiesAction.Duration;
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
                            ammoData.CurrentDelayAction.EndAction();
                            break;
                        case ActionTypes.TransformWithEntities:
                            ammoData.CurrentTransformWithEntitiesAction.EndAction(ref localTransform, new LocalTransform[] { homingTransform, gunTransform });
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
                            ammoData.CurrentDelayAction = action;
                            ammoData.CurrentActionType = ActionTypes.DelayAction;
                            break;
                        case TransformWithEntitiesAction action:
                            ammoData.CurrentTransformWithEntitiesAction = action;
                            ammoData.CurrentActionType = ActionTypes.TransformWithEntities;
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
                            ammoData.CurrentDelayAction.ReadyAction();
                            return;
                        case ActionTypes.TransformWithEntities:
                            ammoData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, new LocalTransform[] { homingTransform , gunTransform });
                            break;
                    }

                    ammoData.CurrentActionTimer = 0;
                }
            })
            .WithoutBurst().Run();
    }
}