namespace HomingGun
{
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;
    using static HomingGun.GunData;

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

            foreach ((var shootDataRef, var homingDataShared) in SystemAPI.Query<RefRW<GunData>, HomingData>())
            {
                var shootDataRO = shootDataRef.ValueRO;

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

                                    AmmoData ammoData = new()
                                    {
                                        Patterns = GetBulletPattern(shootDataRO.PatternSelect, shootDataRO.GunStats.Power),
                                        CurrentIndex = 0,
                                        CurrentActionTimer = 0
                                    };

                                    ecb.AddComponent(ammoEntity, ammoData);
                                    ecb.AddComponent(ammoEntity, new AmmoInit());
                                    ecb.AddSharedComponent(ammoEntity, homingDataShared);
                                    ecb.AddSharedComponent(ammoEntity, new DelayData());

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

                IAction[] GetBulletPattern(GunPatternSelect select, float power)
                {
                    IAction[] bulletPattern;
                    switch (select)
                    {
                        case GunPatternSelect.Simple:
                            bulletPattern = new IAction[]
                            {
                                new TransformWithEntitiesAction
                                {
                                    Duration = 0.1f,
                                    StartTime = 0,

                                    Action = SimpleHoming,
                                    ActionSpeed = power,
                                    IsDeltaAction = true
                                },
                            };
                            return bulletPattern;
                        case GunPatternSelect.DistanceProximity:
                            bulletPattern = new IAction[]
                            {
                                new TransformWithEntitiesAction
                                {
                                    Duration = 0.1f,
                                    StartTime = 0,

                                    Action = DistanceProximityHoming,
                                    ActionSpeed = power,
                                    IsDeltaAction = true
                                },
                            };
                            return bulletPattern;
                        case GunPatternSelect.LimitedProximity:
                            bulletPattern = new IAction[]
                            {
                                new TransformWithEntitiesAction
                                {
                                    Duration = 0.1f,
                                    StartTime = 0,

                                    Action = LimitedProximityHoming,
                                    ActionSpeed = power,
                                    IsDeltaAction = true
                                },
                            };
                            return bulletPattern;
                        case GunPatternSelect.Accelerated:
                            bulletPattern = new IAction[]
                            {
                                new TransformWithEntitiesAction
                                {
                                    Duration = 0.1f,
                                    StartTime = 0,

                                    Action = AcceleratedHoming,
                                    ActionSpeed = power,
                                    IsDeltaAction = true
                                },
                            };
                            return bulletPattern;
                        default:
                            throw new NotImplementedException();
                    }

                    TransformData SimpleHoming(TransformData startData, float speed, float time, LocalTransform[] entities)
                    {
                        var hasHoming = entities.Length > 0;
                        if (hasHoming)
                        {
                            var homingTransform = entities[0];

                            var dirToPlayer = math.normalize(homingTransform.Position - startData.Position);

                            startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingDataShared.HomingRate * (speed * time));
                        }

                        float3 forward = math.mul(startData.Rotation, Vector3.forward) * (speed * time);

                        startData.Position = startData.Position + forward;

                        return startData;
                    }

                    TransformData DistanceProximityHoming(TransformData startData, float speed, float time, LocalTransform[] entities)
                    {
                        var hasHoming = entities.Length > 0;
                        if (hasHoming)
                        {
                            var homingTransform = entities[0];

                            bool targetInRange = Vector3.Distance(startData.Position, homingTransform.Position) <= homingDataShared.ProximityDistance;

                            if (targetInRange)
                            {
                                var dirToPlayer = math.normalize(homingTransform.Position - startData.Position);

                                startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingDataShared.HomingRate * (speed * time));
                            }
                        }

                        float3 forward = math.mul(startData.Rotation, Vector3.forward) * (speed * time);

                        startData.Position = startData.Position + forward;

                        return startData;
                    }

                    TransformData LimitedProximityHoming(TransformData startData, float speed, float time, LocalTransform[] entities)
                    {
                        var hasHoming = entities.Length > 0;
                        if (hasHoming)
                        {
                            var homingTransform = entities[0];
                            var gunTransform = entities[1];

                            var dirToPlayer = math.normalize(homingTransform.Position - startData.Position);

                            bool targetInRange = Quaternion.Angle(gunTransform.Rotation, Quaternion.LookRotation(dirToPlayer)) <= homingDataShared.HomingRate * homingDataShared.LimitedProximityFactor;

                            if (targetInRange)
                            {
                                startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingDataShared.HomingRate * (speed * time));
                            }
                        }

                        float3 forward = math.mul(startData.Rotation, Vector3.forward) * (speed * time);

                        startData.Position = startData.Position + forward;

                        return startData;
                    }

                    TransformData AcceleratedHoming(TransformData startData, float speed, float time, LocalTransform[] entities)
                    {
                        var hasHoming = entities.Length > 0;
                        if (hasHoming)
                        {
                            var homingTransform = entities[0];

                            var dirToPlayer = math.normalize(homingTransform.Position - startData.Position);

                            startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingDataShared.HomingRate * (speed * time));

                            // Linear Acceleration growth based on distance
                            speed = speed * Mathf.Clamp(homingDataShared.AcceleratedRadius / Vector3.Distance(startData.Position, homingTransform.Position), 1, homingDataShared.AccelerationMulti);
                        }

                        float3 forward = math.mul(startData.Rotation, Vector3.forward) * (speed * time);

                        startData.Position = startData.Position + forward;

                        return startData;
                    }
                }
            }
        }
    }
}