namespace SynchronizedGun
{
    using System;
    using Unity.Entities;
    using Unity.Transforms;
    using static SynchronizedGun.GunData;

    [DisableAutoCreation]
    public partial class GunSystem : SystemBase
    {
        ComponentLookup<LocalTransform> localTransformLU;
        ComponentLookup<LocalToWorld> transformLU;

        protected override void OnCreate()
        {
            localTransformLU = GetComponentLookup<LocalTransform>(true);
            transformLU = GetComponentLookup<LocalToWorld>(true);
        }
        protected override void OnDestroy() { }
        protected override void OnUpdate()
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

            localTransformLU.Update(this);
            transformLU.Update(this);

            var DeltaTime = SystemAPI.Time.DeltaTime;

            Entities.WithName("GunUpdate")
                .WithAll<GunData>()
                .ForEach((
                    ref LocalTransform localTransform, in GunData gunData) =>
                {
                    bool HaveAmmo = gunData.CurrentAmmoCount > 0;
                    bool HaveMag = gunData.CurrentMagazineCount > 0;

                    // Update()
                    {
                        //PreShootAction()
                        {
                            if (gunData.Patterns != null && gunData.Patterns.Length > 0)
                            {
                                LocalTransform[] entitiesTransform = new LocalTransform[gunData.WithEntities.Length];
                                for (int i = 0; i < entitiesTransform.Length; i++)
                                {
                                    entitiesTransform[i] = localTransformLU[gunData.WithEntities[i]];
                                }

                                gunData.CurrentActionTimer += DeltaTime;

                                // DoAction
                                bool DoAction;
                                switch (gunData.CurrentActionType)
                                {
                                    case ActionTypes.TransformAction:
                                        gunData.CurrentTransformAction.DoAction(DeltaTime, ref localTransform);
                                        DoAction = gunData.CurrentActionTimer >= gunData.CurrentTransformAction.Duration;
                                        break;
                                    case ActionTypes.TransformWithEntities:
                                        gunData.CurrentTransformWithEntitiesAction.DoAction(DeltaTime, ref localTransform, entitiesTransform);
                                        DoAction = gunData.CurrentActionTimer >= gunData.CurrentTransformWithEntitiesAction.Duration;
                                        break;
                                    default:
                                        throw new System.Exception("Not Implemented");
                                }

                                if (DoAction)
                                {
                                    // EndAction();
                                    switch (gunData.CurrentActionType)
                                    {
                                        case ActionTypes.TransformAction:
                                            gunData.CurrentTransformAction.EndAction(ref localTransform);
                                            break;
                                        case ActionTypes.TransformWithEntities:
                                            gunData.CurrentTransformWithEntitiesAction.EndAction(ref localTransform, entitiesTransform);
                                            break;
                                    }

                                    // GetNextAction();
                                    switch (gunData.Patterns[gunData.CurrentIndex])
                                    {
                                        case TransformAction action:
                                            gunData.CurrentTransformAction = action;
                                            gunData.CurrentActionType = ActionTypes.TransformAction;
                                            break;
                                        case TransformWithEntitiesAction action:
                                            gunData.CurrentTransformWithEntitiesAction = action;
                                            gunData.CurrentActionType = ActionTypes.TransformWithEntities;
                                            break;
                                    }

                                    if (++gunData.CurrentIndex == gunData.Patterns.Length) gunData.CurrentIndex = 0;

                                    // ReadyAction();
                                    switch (gunData.CurrentActionType)
                                    {
                                        case ActionTypes.TransformAction:
                                            gunData.CurrentTransformAction.ReadyAction(localTransform);
                                            break;
                                        case ActionTypes.TransformWithEntities:
                                            gunData.CurrentTransformWithEntitiesAction.ReadyAction(localTransform, entitiesTransform);
                                            break;
                                    }

                                    gunData.CurrentActionTimer = 0;
                                }
                            }
                        }

                        //gunData.ShootTimer += DeltaTime;

                        //if (HaveAmmo)
                        //{
                        //    // AttemptShoot()
                        //    {
                        //        if (gunData.ShootTimer >= gunData.GunStats.ShootDelay)
                        //        {
                        //            gunData.ShootTimer = 0;

                        //            // Fire()
                        //            {
                        //                Entity ammoEntity = ecb.Instantiate(gunData.AmmoPrefab);

                        //                var spawnTransform = transformLU.GetRefRO(gunData.SpawnPosRot).ValueRO;

                        //                ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, gunData.SpawnScale));

                        //                AmmoData ammoData = new()
                        //                {
                        //                    Patterns = GetBulletPattern(gunData.PatternSelect, gunData.GunStats.Power),
                        //                    CurrentIndex = 0,
                        //                    CurrentActionTimer = 0
                        //                };

                        //                ecb.AddComponent(ammoEntity, ammoData);
                        //                ecb.AddComponent(ammoEntity, new AmmoInit());

                        //                gunData.CurrentAmmoCount--;
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    if (HaveMag)
                        //    {
                        //        // AttemptReload()
                        //        {
                        //            gunData.ReloadTimer += DeltaTime;

                        //            if (gunData.ReloadTimer >= gunData.GunStats.ReloadDelay)
                        //            {
                        //                gunData.ReloadTimer -= gunData.GunStats.ReloadDelay;

                        //                // Reload()
                        //                {
                        //                    gunData.CurrentMagazineCount--;
                        //                    gunData.CurrentAmmoCount = gunData.GunStats.MagazineCapacity;
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                })
                .WithoutBurst().Run();

            Dependency.Complete();
        }

        IAction[] GetBulletPattern(GunPatternSelect select, float power)
        {
            switch (select)
            {
                case GunPatternSelect.ShootMoveSync:
                    return BulletPatterns.Straight(power);
                case GunPatternSelect.BulletMoveSync:
                    return BulletPatterns.Straight(power);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}