namespace SimpleGun
{
    using Unity.Entities;
    using Unity.Transforms;

    [DisableAutoCreation]
    [UpdateBefore(typeof(AmmoSystem))]
    public partial class AmmoInitSystem : SystemBase
    {
        protected override void OnCreate() { }
        protected override void OnDestroy() { }
        protected override void OnUpdate()
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

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
                       case DelayAction action:
                           //data.CurrentDelayAction = action;
                           //data.CurrentActionType = ActionTypes.DelayAction;
                           break;
                       case SplitAction action:
                           //data.CurrentSplitAction = action;
                           //data.CurrentActionType = ActionTypes.SplitAction;
                           break;
                   }

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

                   ecb.RemoveComponent<AmmoInit>(e);
               })
               .WithoutBurst().Run();
        }
    }
}