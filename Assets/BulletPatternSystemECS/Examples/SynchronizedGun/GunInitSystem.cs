namespace SynchronizedGun
{
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [DisableAutoCreation]
    [UpdateBefore(typeof(GunSystem))]
    public partial class GunInitSystem : SystemBase
    {
        ComponentLookup<LocalTransform> transformLU;

        protected override void OnCreate() 
        {
            transformLU = GetComponentLookup<LocalTransform>(true);
        }
        protected override void OnDestroy() { }
        protected override void OnUpdate()
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

            transformLU.Update(this);

            foreach ((var gunSetupData, var entity) in SystemAPI.Query<GunSetupData>().WithEntityAccess())
            {
                var setupData_RO = gunSetupData;

                if (!EntityManager.HasComponent<GunData>(setupData_RO.GunEntity)) continue;
                
                var gunData = EntityManager.GetComponentObject<GunData>(setupData_RO.GunEntity);
                gunData.Setup(setupData_RO.GunStats, setupData_RO.PatternSelect);
                gunData.WithEntities = setupData_RO.WithEntities;



                var systemPattern = new IAction[]
                {
                    new TransformWithEntitiesAction
                    {
                        Duration = gunData.GunStats.ShootDelay,
                        StartTime = 0,

                        Action = TranslateMove1,
                        ActionSpeed = gunData.GunStats.Power,
                        IsDeltaAction = false
                    },
                    new TransformWithEntitiesAction
                    {
                        Duration = gunData.GunStats.ShootDelay,
                        StartTime = 0,

                        Action = TranslateMove2,
                        ActionSpeed = gunData.GunStats.Power,
                        IsDeltaAction = false
                    },
                    new TransformWithEntitiesAction
                    {
                        Duration = gunData.GunStats.ShootDelay,
                        StartTime = 0,

                        Action = TranslateMove3,
                        ActionSpeed = gunData.GunStats.Power,
                        IsDeltaAction = false
                    },
                    new TransformWithEntitiesAction
                    {
                        Duration = gunData.GunStats.ShootDelay,
                        StartTime = 0,

                        Action = TranslateMove4,
                        ActionSpeed = gunData.GunStats.Power,
                        IsDeltaAction = false
                    },
                };

                gunData.Patterns = systemPattern;

                if (gunData.Patterns != null && gunData.Patterns.Length >= 0)
                {
                    var localTransform = transformLU[setupData_RO.GunEntity];

                    LocalTransform[] entitiesTransform = new LocalTransform[gunData.WithEntities.Length];
                    for (int i = 0; i < entitiesTransform.Length; i++)
                    {
                        entitiesTransform[i] = transformLU[gunData.WithEntities[i]];
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
                }

                ecb.SetComponent(setupData_RO.GunEntity, gunData);

                ecb.DestroyEntity(entity);

                TransformData TranslateMove1(TransformData startData, float speed, float time, LocalTransform[] entities)
                {
                    float lerpTime;
                    if (time == 0) lerpTime = 1;
                    else lerpTime = time / gunData.GunStats.ShootDelay;

                    startData.Position = Vector3.Lerp(startData.Position, entities[0].Position, lerpTime);

                    return startData;
                }
                TransformData TranslateMove2(TransformData startData, float speed, float time, LocalTransform[] entities)
                {
                    float lerpTime;
                    if (time == 0) lerpTime = 1;
                    else lerpTime = time / gunData.GunStats.ShootDelay;

                    startData.Position = Vector3.Lerp(startData.Position, entities[1].Position, lerpTime);

                    return startData;
                }
                TransformData TranslateMove3(TransformData startData, float speed, float time, LocalTransform[] entities)
                {
                    float lerpTime;
                    if (time == 0) lerpTime = 1;
                    else lerpTime = time / gunData.GunStats.ShootDelay;

                    startData.Position = Vector3.Lerp(startData.Position, entities[2].Position, lerpTime);

                    return startData;
                }
                TransformData TranslateMove4(TransformData startData, float speed, float time, LocalTransform[] entities)
                {
                    float lerpTime;
                    if (time == 0) lerpTime = 1;
                    else lerpTime = time / gunData.GunStats.ShootDelay;

                    startData.Position = Vector3.Lerp(startData.Position, entities[3].Position, lerpTime);

                    return startData;
                }
            }
        }
    }
}