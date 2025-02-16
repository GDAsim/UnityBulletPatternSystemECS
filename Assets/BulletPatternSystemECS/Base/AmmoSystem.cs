using HomingGun;
using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
public partial class AmmoSystem : SystemBase
{
    ComponentLookup<LocalToWorld> localTransformLU;

    protected override void OnCreate() 
    {
        localTransformLU = GetComponentLookup<LocalToWorld>(true);
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
                ref LocalTransform localTransform, in AmmoData ammoData, in GunHomingData homingData) =>
            {
                if (ammoData.Patterns == null || ammoData.Patterns.Length == 0)
                {
                    return;
                }

                LocalToWorld homingTransform = default;
                if (localTransformLU.HasComponent(homingData.HomingEntity))
                {
                    homingTransform = localTransformLU.GetRefRO(homingData.HomingEntity).ValueRO;
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
                    case ActionTypes.TransformWithEntities:
                        ammoData.CurrentTransformWithEntitiesAction.DoAction(DeltaTime, ref localTransform, new LocalToWorld[] { homingTransform });
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
                        case ActionTypes.TransformWithEntities:
                            ammoData.CurrentTransformWithEntitiesAction.EndAction(ref localTransform, new LocalToWorld[] { homingTransform });
                            break;
                    }

                    // GetNextAction();
                    switch (ammoData.Patterns[ammoData.CurrentIndex])
                    {
                        case TransformAction action:
                            ammoData.CurrentTransformAction = action;
                            ammoData.CurrentActionType = ActionTypes.TransformAction;
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
                        case ActionTypes.TransformWithEntities:
                            ammoData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, new LocalToWorld[] { homingTransform });
                            break;
                    }

                    ammoData.CurrentActionTimer = 0;
                }
            })
            .WithoutBurst().Run();
    }
}