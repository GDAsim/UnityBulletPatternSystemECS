namespace TeleportGun
{
    using HomingGun;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [DisableAutoCreation]
    public partial struct GunSystem : ISystem
    {
        ComponentLookup<LocalToWorld> localTransformLU;

        public void OnCreate(ref SystemState state)
        {
            localTransformLU = state.GetComponentLookup<LocalToWorld>(true);
        }
        public void OnDestroy(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            localTransformLU.Update(ref state);

            DoJobs(ref state, ref ecb, ref localTransformLU);
        }

        void DoJobs(
            ref SystemState state,
            ref EntityCommandBuffer ecb,
            ref ComponentLookup<LocalToWorld> localTransformLU)
        {
            new ShootJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,

                Ecb = ecb,

                localTransformLU = localTransformLU,
            }.Schedule();
        }
    }

    public partial struct ShootJob : IJobEntity
    {
        public float DeltaTime;

        public EntityCommandBuffer Ecb;

        [ReadOnly] public ComponentLookup<LocalToWorld> localTransformLU;

        void Execute(ref GunData shootData)
        {
            bool HaveAmmo = shootData.CurrentAmmoCount > 0;
            bool HaveMag = shootData.CurrentMagazineCount > 0;

            // Update()
            {
                shootData.ShootTimer += DeltaTime;

                if (HaveAmmo)
                {
                    // AttemptShoot()
                    {
                        if (shootData.ShootTimer >= shootData.GunStats.ShootDelay)
                        {
                            shootData.ShootTimer = 0;

                            // Fire()
                            {
                                Entity ammoEntity = Ecb.Instantiate(shootData.AmmoPrefab);

                                var spawnTransform = localTransformLU.GetRefRO(shootData.SpawnPosRot).ValueRO;

                                Ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, shootData.SpawnScale));

                                AmmoData ammoData = new()
                                {
                                    Patterns = Gun.GetBulletPattern(shootData.PatternSelect, shootData.GunStats.Power),
                                    CurrentIndex = 0,
                                    CurrentActionTimer = 0
                                };

                                Ecb.AddComponent(ammoEntity, ammoData);
                                Ecb.AddComponent(ammoEntity, new AmmoInit());
                                Ecb.AddSharedComponent(ammoEntity, new HomingData());
                                Ecb.AddSharedComponent(ammoEntity, new DelayData());

                                shootData.CurrentAmmoCount--;
                            }
                        }
                    }
                }
                else
                {
                    if (HaveMag)
                    {
                        // AttemptReload()
                        {
                            shootData.ReloadTimer += DeltaTime;

                            if (shootData.ReloadTimer >= shootData.GunStats.ReloadDelay)
                            {
                                shootData.ReloadTimer -= shootData.GunStats.ReloadDelay;

                                // Reload()
                                {
                                    shootData.CurrentMagazineCount--;
                                    shootData.CurrentAmmoCount = shootData.GunStats.MagazineCapacity;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}