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

            //state.Dependency.Complete();

            var DeltaTime = SystemAPI.Time.DeltaTime;

            foreach ((var shootDataRef, var homingDataRef) in SystemAPI.Query<RefRW<GunData>, RefRO<GunHomingData>>())
            {
                var shootDataRO = shootDataRef.ValueRO;
                var homingData = homingDataRef.ValueRO;

                LocalToWorld homingTransform = default;
                bool homingTarget = false;
                if (localTransformLU.HasComponent(homingData.HomingEntity))
                {
                    homingTransform = localTransformLU.GetRefRO(homingData.HomingEntity).ValueRO;
                    homingTarget = true;
                }

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
                                    new TransformAction
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

                TransformData SimpleHoming(TransformData startData, float speed, float time)
                {
                    if (homingTarget)
                    {
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


            //DoJobs(ref state, ref ecb, ref localTransformLU);
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
            }.Run();
        }
    }

    public partial struct ShootJob : IJobEntity
    {
        public float DeltaTime;

        public EntityCommandBuffer Ecb;

        [ReadOnly] public ComponentLookup<LocalToWorld> localTransformLU;

        void Execute(ref GunData shootData, GunHomingData homingData)
        {
            LocalToWorld homingTransform = default;
            bool homingTarget = false;
            if (localTransformLU.HasComponent(homingData.HomingEntity))
            {
                homingTransform = localTransformLU.GetRefRO(homingData.HomingEntity).ValueRO;
                homingTarget = true;
            }

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

                                var p = new IAction[1]
                                {
                                    new TransformAction
                                    {
                                        Duration = 0.1f,
                                        StartTime = 0,

                                        Action = SimpleHoming,
                                        ActionSpeed = shootData.GunStats.Power,
                                        IsDeltaAction = true
                                    },
                                };
                                AmmoData ammoData = new()
                                {
                                    Patterns = p,
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

            TransformData SimpleHoming(TransformData startData, float speed, float time)
            {
                if (homingTarget)
                {
                    var pos = homingTransform.Position;
                    var posVector = new Vector3(pos.x, pos.y, pos.z);

                    var dirToPlayer = (posVector - startData.Position).normalized;

                    startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingData.HomingRate * (speed * time));
                }

                Vector3 forward = startData.Rotation * Vector3.forward * (speed * time);

                startData.Position = startData.Position + forward;

                return startData;
            }
            TransformData DistanceProximityHoming(TransformData startData, float speed, float time)
            {
                if (homingTarget)
                {
                    var pos = homingTransform.Position;
                    var posVector = new Vector3(pos.x, pos.y, pos.z);

                    bool targetInRange = Vector3.Distance(startData.Position, posVector) <= homingData.ProximityDistance;

                    if (targetInRange)
                    {
                        var dirToPlayer = (posVector - startData.Position).normalized;

                        startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingData.HomingRate * (speed * time));
                    }
                }

                Vector3 forward = startData.Rotation * Vector3.forward * (speed * time);

                startData.Position = startData.Position + forward;

                return startData;
            }

            //TransformData LimitedProximityHoming(TransformData startData, float speed, float time)
            //{
            //    if (homingTarget)
            //    {
            //        var pos = homingTransform.Position;
            //        var posVector = new Vector3(pos.x, pos.y, pos.z);

            //        var dirToPlayer = (posVector - startData.Position).normalized;

            //        bool targetInRange = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dirToPlayer)) <= homingData.HomingRate * homingData.LimitedProximityFactor;

            //        if (targetInRange)
            //        {
            //            startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingData.HomingRate * (speed * time));
            //        }
            //    }

            //    Vector3 forward = startData.Rotation * Vector3.forward * (speed * time);

            //    startData.Position = startData.Position + forward;

            //    return startData;
            //}

            TransformData AcceleratedHoming(TransformData startData, float speed, float time)
            {
                if (homingTarget)
                {
                    var pos = homingTransform.Position;
                    var posVector = new Vector3(pos.x, pos.y, pos.z);

                    var dirToPlayer = (posVector - startData.Position).normalized;

                    startData.Rotation = Quaternion.RotateTowards(startData.Rotation, Quaternion.LookRotation(dirToPlayer), homingData.HomingRate * (speed * time));

                    // Linear Acceleration growth based on distance
                    var forwardSpeed = speed * Mathf.Clamp(homingData.AcceleratedRadius / Vector3.Distance(startData.Position, posVector), 1, homingData.AccelerationMulti);
                    Vector3 forward = startData.Rotation * Vector3.forward * (forwardSpeed * time);

                    startData.Position = startData.Position + forward;
                }

                return startData;
            }

        }
    }
}