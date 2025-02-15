namespace SimpleGun
{
    using Unity.Entities;

    [DisableAutoCreation]
    public partial struct GunInitSystem : ISystem
    {
        ComponentLookup<GunData> gunDataLU;
        public void OnCreate(ref SystemState state)
        {
            gunDataLU = state.GetComponentLookup<GunData>(false);
        }
        public void OnDestroy(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            gunDataLU.Update(ref state);

            state.Dependency.Complete();

            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach ((var gunSetupData, var entity) in SystemAPI.Query<RefRO<GunSetupData>>().WithEntityAccess())
            {
                var setupData_RO = gunSetupData.ValueRO;

                if (!gunDataLU.HasComponent(setupData_RO.GunEntity)) continue;

                var gunDataRef = gunDataLU.GetRefRW(setupData_RO.GunEntity);
                gunDataRef.ValueRW.Setup(setupData_RO.GunStats, setupData_RO.PatternSelect);

                state.EntityManager.SetComponentEnabled<GunData>(setupData_RO.GunEntity, true);

                ecb.DestroyEntity(entity);
            }
        }
    }
}