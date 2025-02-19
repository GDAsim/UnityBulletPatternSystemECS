namespace HomingGun
{
    using System;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] HomingType shootmode;

        [SerializeField] Gun Gun;

        [Header("Homing Properties")]
        [SerializeField] Transform HomingTarget;
        [SerializeField] float HomingRate = 15f;

        [Header("Distance Proximity Properties")]
        [SerializeField] float ProximityDistance = 10;

        [Header("Limited Proximity Properties")]
        [SerializeField] float LimitedProximityFactor = 1.1f;

        [Header("Accelerated Properties")]
        [SerializeField] float AcceleratedRadius = 10;
        [SerializeField] float AccelerationMulti = 3;

        enum HomingType { Simple, DistanceProximity, LimitedProximity, Accelerated }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                GunPatternSelect PatternSelect;
                switch (authoring.shootmode)
                {
                    case HomingType.Simple:
                        PatternSelect = GunPatternSelect.Simple;
                        break;
                    case HomingType.DistanceProximity:
                        PatternSelect = GunPatternSelect.DistanceProximity;
                        break;
                    case HomingType.LimitedProximity:
                        PatternSelect = GunPatternSelect.LimitedProximity;
                        break;
                    case HomingType.Accelerated:
                        PatternSelect = GunPatternSelect.Accelerated;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                var gunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic);
                AddComponent(EntityA, new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = PatternSelect,
                    GunEntity = gunEntity,

                    GunHomingData = new HomingData()
                    {
                        HomingEntity = GetEntity(authoring.HomingTarget, TransformUsageFlags.Dynamic),
                        HomingRate = authoring.HomingRate,

                        ProximityDistance = authoring.ProximityDistance,

                        LimitedProximityFactor = authoring.LimitedProximityFactor,
                        GunEntity = gunEntity,

                        AcceleratedRadius = authoring.AcceleratedRadius,
                        AccelerationMulti = authoring.AccelerationMulti,
                    }
                });
            }
        }
    }

    [BurstCompile]
    public struct GunSetupData : IComponentData
    {
        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public Entity GunEntity;

        public HomingData GunHomingData;
    }
}