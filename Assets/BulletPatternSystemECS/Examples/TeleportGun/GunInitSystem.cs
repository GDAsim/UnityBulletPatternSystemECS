namespace TeleportGun
{
    using Unity.Entities;
    using Unity.Transforms;

    [DisableAutoCreation]
    public partial class GunInitSystem : SystemBase
    {
        ComponentLookup<LocalTransform> transformLU;

        protected override void OnCreate()
        {
            transformLU = GetComponentLookup<LocalTransform>(true);
        }
        protected override void OnDestroy() { }
        protected override void OnUpdate()
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

            transformLU.Update(this);

            foreach ((var gunSetupData, var entity) in SystemAPI.Query<GunSetupData>().WithEntityAccess())
            {
                var setupData_RO = gunSetupData;

                if (!EntityManager.HasComponent<GunData>(setupData_RO.GunEntity)) continue;

                var gunData = EntityManager.GetComponentObject<GunData>(setupData_RO.GunEntity);

                SetupShoot();

                ecb.DestroyEntity(entity);

                void SetupShoot()
                {
                    gunData.Setup(setupData_RO.GunStats, setupData_RO.PatternSelect);
                    gunData.WithEntities = setupData_RO.WithEntities;

                    ecb.SetComponent(setupData_RO.GunEntity, gunData);
                }
            }
        }
    }
}