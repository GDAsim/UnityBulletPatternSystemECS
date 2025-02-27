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

        Entities.WithName("AmmoInitWith")
           .WithAll<AmmoInit>()
           .ForEach((
               Entity e,
               ref LocalTransform localTransform, in AmmoData ammoData,
               in HomingData homingData, in DelayData delayData) =>
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
                   case SplitAction action:
                       ammoData.CurrentSplitAction = action;
                       ammoData.CurrentActionType = ActionTypes.SplitAction;
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
                       break;
                   case ActionTypes.TransformWithEntities:
                       ammoData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, new LocalTransform[] { homingTransform, gunTransform });
                       break;
                   case ActionTypes.SplitAction:
                       ammoData.CurrentSplitAction.ReadyAction(e, ammoData);
                       return;
               }

               ecb.RemoveComponent<AmmoInit>(e);
           })
           .WithoutBurst().Run();
    }
}