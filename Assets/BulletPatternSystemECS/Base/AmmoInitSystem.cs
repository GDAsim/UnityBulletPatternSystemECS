using HomingGun;
using Unity.Entities;
using Unity.Transforms;

[DisableAutoCreation]
[UpdateBefore(typeof(AmmoSystem))]
public partial class AmmoInitSystem : SystemBase
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

        Entities.WithName("AmmoInit")
           .WithAll<AmmoInit>()
           .ForEach((
               Entity e,
               ref LocalTransform localTransform, in AmmoData ammoData) =>
           {
               if (ammoData.Patterns != null && ammoData.Patterns.Length >= 0)
               {
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
                           ammoData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, new LocalTransform[] { });
                           break;
                   }

                   ecb.RemoveComponent<AmmoInit>(e);
               }
           })
           .WithoutBurst().Run();

        Entities.WithName("AmmoInitWithHoming")
           .WithAll<AmmoInit>()
           .ForEach((
               Entity e,
               ref LocalTransform localTransform, in AmmoData ammoData, in GunHomingData homingData) =>
           {
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

               ecb.RemoveComponent<AmmoInit>(e);
           })
           .WithoutBurst().Run();
    }
}