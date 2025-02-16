namespace HomingGun
{
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

                if (authoring.shootmode == HomingType.Simple)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.Simple,
                        GunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic),

                        GunHomingData = new GunHomingData()
                        {
                            HomingEntity = GetEntity(authoring.HomingTarget, TransformUsageFlags.Dynamic),
                            HomingRate = authoring.HomingRate,

                            ProximityDistance = authoring.ProximityDistance,

                            LimitedProximityFactor = authoring.LimitedProximityFactor,

                            AcceleratedRadius = authoring.AcceleratedRadius,
                            AccelerationMulti = authoring.AccelerationMulti,
                        }
                    });
                }
                else if (authoring.shootmode == HomingType.DistanceProximity)
                {
                    
                }
                else if (authoring.shootmode == HomingType.LimitedProximity)
                {
                    
                }
                else if (authoring.shootmode == HomingType.Accelerated)
                {

                }
            }
        }
    }

    [BurstCompile]
    public struct GunSetupData : IComponentData
    {
        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public Entity GunEntity;

        public GunHomingData GunHomingData;
    }
}