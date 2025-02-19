namespace TeleportGun
{
    using HomingGun;
    using System;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;
    using static TeleportGun.GunData;

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

            foreach ((var localTransformRef, var gunData, var gunEntity) in SystemAPI.Query<RefRW<LocalTransform>, GunData>().WithEntityAccess())
            {
                bool HaveAmmo = gunData.CurrentAmmoCount > 0;
                bool HaveMag = gunData.CurrentMagazineCount > 0;

                PreShootAction();

                gunData.ShootTimer += DeltaTime;

                if (HaveAmmo)
                {
                    AttemptShoot();
                }
                else
                {
                    if (HaveMag)
                    {
                        AttemptReload();
                    }
                }

                void PreShootAction()
                {
                    if (gunData.Patterns == null || gunData.Patterns.Length == 0) return;

                    LocalTransform[] entitiesTransform = new LocalTransform[gunData.WithEntities.Length];
                    for (int i = 0; i < entitiesTransform.Length; i++)
                    {
                        entitiesTransform[i] = localTransformLU[gunData.WithEntities[i]];
                    }

                    gunData.CurrentActionTimer += DeltaTime;

                    if (DoAction())
                    {
                        EndAction();

                        GetNextAction();

                        ReadyAction();

                        gunData.CurrentActionTimer = 0;
                    }

                    bool DoAction()
                    {
                        switch (gunData.CurrentActionType)
                        {
                            case ActionTypes.TransformAction:
                                gunData.CurrentTransformAction.DoAction(DeltaTime, ref localTransformRef.ValueRW);
                                return gunData.CurrentActionTimer >= gunData.CurrentTransformAction.Duration;
                            case ActionTypes.DelayAction:
                                gunData.CurrentDelayAction.DoAction(gunData.DelayUntil);
                                return gunData.CurrentActionTimer >= gunData.CurrentDelayAction.Duration;
                            case ActionTypes.TransformWithEntities:
                                gunData.CurrentTransformWithEntitiesAction.DoAction(DeltaTime, ref localTransformRef.ValueRW, entitiesTransform);
                                return gunData.CurrentActionTimer >= gunData.CurrentTransformWithEntitiesAction.Duration;
                            default:
                                throw new System.Exception("Not Implemented");
                        }
                    }
                    void EndAction()
                    {
                        switch (gunData.CurrentActionType)
                        {
                            case ActionTypes.TransformAction:
                                gunData.CurrentTransformAction.EndAction(ref localTransformRef.ValueRW);
                                break;
                            case ActionTypes.DelayAction:
                                gunData.CurrentDelayAction.EndAction();
                                break;
                            case ActionTypes.TransformWithEntities:
                                gunData.CurrentTransformWithEntitiesAction.EndAction(ref localTransformRef.ValueRW, entitiesTransform);
                                break;
                        }
                    }
                    void GetNextAction()
                    {
                        switch (gunData.Patterns[gunData.CurrentIndex])
                        {
                            case TransformAction action:
                                gunData.CurrentTransformAction = action;
                                gunData.CurrentActionType = ActionTypes.TransformAction;
                                break;
                            case DelayAction action:
                                gunData.CurrentDelayAction = action;
                                gunData.CurrentActionType = ActionTypes.DelayAction;
                                break;
                            case TransformWithEntitiesAction action:
                                gunData.CurrentTransformWithEntitiesAction = action;
                                gunData.CurrentActionType = ActionTypes.TransformWithEntities;
                                break;
                        }

                        if (++gunData.CurrentIndex == gunData.Patterns.Length) gunData.CurrentIndex = 0;
                    }
                    void ReadyAction()
                    {
                        switch (gunData.CurrentActionType)
                        {
                            case ActionTypes.TransformAction:
                                gunData.CurrentTransformAction.ReadyAction(localTransformRef.ValueRW);
                                break;
                            case ActionTypes.DelayAction:
                                gunData.CurrentDelayAction.ReadyAction();
                                return;
                            case ActionTypes.TransformWithEntities:
                                gunData.CurrentTransformWithEntitiesAction.ReadyAction(localTransformRef.ValueRW, entitiesTransform);
                                break;
                        }
                    }
                }
                void AttemptShoot()
                {
                    if (gunData.ShootTimer >= gunData.GunStats.ShootDelay)
                    {
                        gunData.ShootTimer = 0;

                        Fire();
                    }
                }
                void Fire()
                {
                    Entity ammoEntity = ecb.Instantiate(gunData.AmmoPrefab);

                    var spawnTransform = transformLU.GetRefRO(gunData.SpawnPosRot).ValueRO;

                    ecb.SetComponent(ammoEntity, LocalTransform.FromPositionRotationScale(spawnTransform.Position, spawnTransform.Rotation, gunData.SpawnScale));

                    AmmoData ammoData = new()
                    {
                        Patterns = GetBulletPattern(gunData.PatternSelect, gunData.GunStats.Power),
                        CurrentIndex = 0,
                        CurrentActionTimer = 0
                    };

                    ecb.AddComponent(ammoEntity, ammoData);
                    ecb.AddComponent(ammoEntity, new AmmoInit());

                    ecb.AddSharedComponent(ammoEntity, new AmmoDataShared() { FiredFrom = gunEntity });
                    ecb.AddSharedComponent(ammoEntity, new HomingData());
                    ecb.AddSharedComponent(ammoEntity, new DelayData());

                    gunData.CurrentAmmoCount--;
                    gunData.TotalShootCount++;
                }
                void AttemptReload()
                {
                    gunData.ReloadTimer += DeltaTime;

                    if (gunData.ReloadTimer >= gunData.GunStats.ReloadDelay)
                    {
                        gunData.ReloadTimer -= gunData.GunStats.ReloadDelay;

                        Reload();
                    }
                }
                void Reload()
                {
                    gunData.CurrentMagazineCount--;
                    gunData.CurrentAmmoCount = gunData.GunStats.MagazineCapacity;
                }

                IAction[] GetBulletPattern(GunPatternSelect select, float power)
                {
                    IAction[] bulletPattern = null;

                    switch (select)
                    {
                        case GunPatternSelect.InstantAction:
                            bulletPattern = new IAction[4]
                            {
                                new TransformAction
                                {
                                    Duration = 1,
                                    StartTime = 0,

                                    Action = TransformAction.MoveForward,
                                    ActionSpeed = power,
                                    IsDeltaAction = true,
                                },
                                new TransformAction
                                {
                                    Action = InstantTeleportRight,
                                    ActionSpeed = power,
                                    IsDeltaAction = false,
                                },
                                new TransformAction
                                {
                                    Duration = 1,
                                    StartTime = 0,

                                    Action = TransformAction.MoveForward,
                                    ActionSpeed = power,
                                    IsDeltaAction = true,
                                },
                                new TransformAction
                                {
                                    Action = InstantTeleportLeft,
                                    ActionSpeed = power,
                                    IsDeltaAction = false,
                                },
                            };
                            return bulletPattern;
                        case GunPatternSelect.JumpAction:
                            bulletPattern = new IAction[2]
                            {
                                new TransformAction
                                {
                                    Duration = 1,
                                    StartTime = 0,

                                    Action = TransformAction.MoveForward,
                                    ActionSpeed = power,
                                    IsDeltaAction = true,
                                },
                                new TransformAction
                                {
                                    Duration = 1,
                                    StartTime = 1,

                                    Action = TransformAction.MoveForward,
                                    ActionSpeed = power,
                                    IsDeltaAction = false,
                                },
                            };
                            return bulletPattern;
                        default:
                            throw new NotImplementedException();
                    }

                    TransformData InstantTeleportRight(TransformData startData, float speed, float time)
                    {
                        var right = math.mul(startData.Rotation, Vector3.right);

                        startData.Position = startData.Position + right;

                        return startData;
                    }
                    TransformData InstantTeleportLeft(TransformData startData, float speed, float time)
                    {
                        var left = math.mul(startData.Rotation, Vector3.left);

                        startData.Position = startData.Position + left;

                        return startData;
                    }
                }
            }

            // Foreach gun, Update Delay Data (if any)
            foreach ((var gunData, var gunEntity) in SystemAPI.Query<GunData>().WithEntityAccess())
            {
                foreach ((var localTransformRef, var _, var ammoData, var e) in SystemAPI.Query<RefRW<LocalTransform>, DelayData, AmmoData>()
                    .WithSharedComponentFilter(new AmmoDataShared() { FiredFrom = gunEntity })
                    .WithEntityAccess())
                {
                    var Has4ShootCycleEnd = gunData.TotalShootCount % gunData.GunStats.MagazineCapacity == 0;
                    var delayData = new DelayData() { DelayUntil = Has4ShootCycleEnd };
                    ecb.SetSharedComponent(e, delayData);
                }
            }

            Dependency.Complete();
        }
    }
}