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

        new BulletSpawnerJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,

            Ecb = ecb,

            localTransformLU = localTransformLU,
        }.Schedule();
    }
}

[BurstCompile]
public partial struct BulletSpawnerJob : IJobEntity
{
    public float DeltaTime;

    public EntityCommandBuffer Ecb;

    public ComponentLookup<LocalTransform> localTransformLU;

    void Execute([ChunkIndexInQuery] int chunkIndex, ref ShootData data)
    {
        bool HaveAmmo = data.CurrentAmmoCount > 0;
        bool HaveMag = data.CurrentMagazineCount > 0;

        // Update()
        {
            if (HaveAmmo)
            {
                // AttemptShoot()
                {
                    data.ShootTimer += DeltaTime;

                    if (data.ShootTimer >= data.ShootDelay)
                    {
                        data.ShootTimer = 0;

                        // Fire()
                        {
                            Entity newBulletEntity = Ecb.Instantiate(data.AmmoPrefab);

                            var spawnTransform = localTransformLU.GetRefRO(data.SpawnTransformEntity).ValueRO;
                            var bulletTransform = localTransformLU.GetRefRW(data.AmmoPrefab).ValueRW;

                            Ecb.SetComponent(newBulletEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, bulletTransform.Scale));

                            //Ecb.addc
                            //ammo.Setup(bulletPattern);

                            data.CurrentAmmoCount--;
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
                        data.ReloadTimer += DeltaTime;

                        if (data.ReloadTimer >= data.ReloadDelay)
                        {
                            data.ReloadTimer -= data.ReloadDelay;

                            // Reload()
                            {
                                data.CurrentMagazineCount--;
                                data.CurrentAmmoCount = data.MagazineCapacity;
                            }
                        }
                    }
                }
            }
        }
    }
}
