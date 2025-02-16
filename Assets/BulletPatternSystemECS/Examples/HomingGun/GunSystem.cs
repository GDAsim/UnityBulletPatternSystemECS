namespace HomingGun
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [DisableAutoCreation]
    [UpdateBefore(typeof(AmmoSystem))]
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

            var DeltaTime = SystemAPI.Time.DeltaTime;

            foreach ((var shootDataRef, var homingDataRef) in SystemAPI.Query<RefRW<GunData>, RefRO<GunHomingData>>())
            {
                var shootDataRO = shootDataRef.ValueRO;
                var homingData = homingDataRef.ValueRO;

                bool HaveAmmo = shootDataRO.CurrentAmmoCount > 0;
                bool HaveMag = shootDataRO.CurrentMagazineCount > 0;

                // Update()
                {
                    shootDataRef.ValueRW.ShootTimer += DeltaTime;

                    if (HaveAmmo)
                    {
                        // AttemptShoot()
                        {
                            if (shootDataRO.ShootTimer >= shootDataRO.GunStats.ShootDelay)
                            {
                                shootDataRef.ValueRW.ShootTimer = 0;

                                // Fire()
                                {
                                    Entity ammoEntity = ecb.Instantiate(shootDataRO.AmmoPrefab);

                                    var spawnTransform = localTransformLU.GetRefRO(shootDataRO.SpawnPosRot).ValueRO;

                                    ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, shootDataRO.SpawnScale));

                                    var p = new IAction[1]
                                    {
                                        new TransformWithEntitiesAction
                                        {
                                            Duration = 0.1f,
                                            StartTime = 0,

                                            Action = SimpleHoming,
                                            ActionSpeed = shootDataRO.GunStats.Power,
                                            IsDeltaAction = true
                                        },
                                    };
                                    AmmoData ammoData = new()
                                    {
                                        Patterns = p,
                                        CurrentIndex = 0,
                                        CurrentActionTimer = 0
                                    };

                                    ecb.AddComponent(ammoEntity, ammoData);
                                    ecb.AddComponent(ammoEntity, new AmmoInit());
                                    ecb.AddComponent(ammoEntity, homingDataRef.ValueRO);

                                    shootDataRef.ValueRW.CurrentAmmoCount--;
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
                                shootDataRef.ValueRW.ReloadTimer += DeltaTime;

                                if (shootDataRO.ReloadTimer >= shootDataRO.GunStats.ReloadDelay)
                                {
                                    shootDataRef.ValueRW.ReloadTimer -= shootDataRO.GunStats.ReloadDelay;

                                    // Reload()
                                    {
                                        shootDataRef.ValueRW.CurrentMagazineCount--;
                                        shootDataRef.ValueRW.CurrentAmmoCount = shootDataRO.GunStats.MagazineCapacity;
                                    }
                                }
                            }
                        }
                    }
                }

                TransformData SimpleHoming(TransformData startData, float speed, float time, LocalToWorld[] entities)
                {
                    var hasHoming = entities.Length > 0;
                    if (hasHoming)
                    {
                        var homingTransform = entities[0];

                        var pos = homingTransform.Position;
                        var posVector = new Vector3(pos.x, pos.y, pos.z);

                        var dirToPlayer = (posVector - startData.Position).normalized;

                        startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingData.HomingRate * (speed * time));
                    }

                    Vector3 forward = startData.Rotation * Vector3.forward * (speed * time);

                    startData.Position = startData.Position + forward;

                    return startData;
                }
            }
        }
    }
}