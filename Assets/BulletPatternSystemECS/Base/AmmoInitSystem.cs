using HomingGun;
using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
[UpdateBefore(typeof(AmmoSystem))]
public partial class AmmoInitSystem : SystemBase
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

        Entities.WithName("AmmoInit")
           .WithAll<AmmoInit>()
           .ForEach((
               Entity e,
               ref LocalTransform localTransform, in AmmoData ammoData) =>
           {
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

               // ReadyAction();
               switch (ammoData.CurrentActionType)
               {
                   case ActionTypes.TransformAction:
                       ammoData.CurrentTransformAction.ReadyAction(localTransform);
                       break;
                   case ActionTypes.TransformWithEntities:
                       ammoData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, new LocalToWorld[] { });
                       break;
               }

               ecb.RemoveComponent<AmmoInit>(e);
           })
           .WithoutBurst().Run();

        Entities.WithName("AmmoInitWithHoming")
           .WithAll<AmmoInit>()
           .ForEach((
               Entity e,
               ref LocalTransform localTransform, in AmmoData ammoData, in GunHomingData homingData) =>
           {
               LocalToWorld homingTransform = default;
               if (localTransformLU.HasComponent(homingData.HomingEntity))
               {
                   homingTransform = localTransformLU.GetRefRO(homingData.HomingEntity).ValueRO;
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

               ecb.RemoveComponent<AmmoInit>(e);
           })
           .WithoutBurst().Run();
    }
}