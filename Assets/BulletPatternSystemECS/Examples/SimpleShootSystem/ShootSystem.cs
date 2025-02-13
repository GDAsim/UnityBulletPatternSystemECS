using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[DisableAutoCreation]
public partial class ShootSystem : SystemBase
{
    ComponentLookup<LocalTransform> localTransformLU;

    protected override void OnCreate() 
    {
        localTransformLU = GetComponentLookup<LocalTransform>(false);
    }
    protected override void OnDestroy() { }
    protected override void OnUpdate()
    {
        //CompleteDependency();

        //var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        //EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        //localTransformLU.Update(this);

        //DoJobs(ref ecb, ref localTransformLU);

        Dependency.Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    void DoJobs(
        ref EntityCommandBuffer ecb,
        ref ComponentLookup<LocalTransform> localTransformLU)
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

    public ComponentLookup<LocalTransform> localTransformLU;

    void Execute(ShootData shootData)
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

                            var spawnTransform = localTransformLU.GetRefRO(shootData.SpawnTransform).ValueRO;
                            var bulletTransform = localTransformLU.GetRefRW(shootData.AmmoPrefab).ValueRW;

                            Ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, bulletTransform.Scale));

                            AmmoData ammoData = new()
                            {
                                //Patterns = BulletPatterns.Straight(2),
                                Patterns = shootData.Patterns,

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
