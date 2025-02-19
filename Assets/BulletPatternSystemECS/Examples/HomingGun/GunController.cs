namespace HomingGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun Gun;

        [SerializeField] PatternSelect patternSelect;

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

        enum PatternSelect { Simple, DistanceProximity, LimitedProximity, Accelerated }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                GunData.GunPatternSelect gunPatternSelect;
                switch (authoring.patternSelect)
                {
                    case PatternSelect.Simple:
                        gunPatternSelect = GunData.GunPatternSelect.Simple;
                        break;
                    case PatternSelect.DistanceProximity:
                        gunPatternSelect = GunData.GunPatternSelect.DistanceProximity;
                        break;
                    case PatternSelect.LimitedProximity:
                        gunPatternSelect = GunData.GunPatternSelect.LimitedProximity;
                        break;
                    case PatternSelect.Accelerated:
                        gunPatternSelect = GunData.GunPatternSelect.Accelerated;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                var gunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic);
                AddComponentObject(EntityA, new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = gunPatternSelect,
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
}