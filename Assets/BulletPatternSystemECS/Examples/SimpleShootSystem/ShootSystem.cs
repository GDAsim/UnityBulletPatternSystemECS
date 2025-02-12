using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[DisableAutoCreation]
public partial struct ShootSystem : ISystem
{
    ComponentLookup<LocalTransform> localTransformLU;

    [BurstCompile] public void OnCreate(ref SystemState state) 
    {
        localTransformLU = state.GetComponentLookup<LocalTransform>(false);
    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile] public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        DoJobs(ref state, ref ecb, ref localTransformLU);
    }

    void DoJobs(ref SystemState state,
        ref EntityCommandBuffer ecb,
        ref ComponentLookup<LocalTransform> localTransformLU)
    {
        localTransformLU.Update(ref state);

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

    public ComponentLookup<LocalTransform> localTransformLU;

    void Execute(ref ShootData shootData)
    {
        bool HaveAmmo = shootData.CurrentAmmoCount > 0;
        bool HaveMag = shootData.CurrentMagazineCount > 0;

        // Update()
        {
            if (HaveAmmo)
            {
                // AttemptShoot()
                {
                    shootData.ShootTimer += DeltaTime;

                    if (shootData.ShootTimer >= shootData.ShootDelay)
                    {
                        shootData.ShootTimer = 0;

                        // Fire()
                        {
                            Entity ammoEntity = Ecb.Instantiate(shootData.AmmoPrefab);

                            var spawnTransform = localTransformLU.GetRefRO(shootData.SpawnTransformEntity).ValueRO;
                            var bulletTransform = localTransformLU.GetRefRW(shootData.AmmoPrefab).ValueRW;

                            Ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, bulletTransform.Scale));

                            AmmoData ammoData = new()
                            {
                                Patterns = BulletPatterns.Straight(2),

                                CurrentIndex = 0,
                                CurrentActionTimer = 0
                            };

                            Ecb.AddComponent(ammoEntity, ammoData);
                            Ecb.AddComponent(ammoEntity, new AmmoInit());

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

                        if (shootData.ReloadTimer >= shootData.ReloadDelay)
                        {
                            shootData.ReloadTimer -= shootData.ReloadDelay;

                            // Reload()
                            {
                                shootData.CurrentMagazineCount--;
                                shootData.CurrentAmmoCount = shootData.MagazineCapacity;
                            }
                        }
                    }
                }
            }
        }
    }
}
