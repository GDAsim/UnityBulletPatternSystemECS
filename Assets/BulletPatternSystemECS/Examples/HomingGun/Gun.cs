namespace HomingGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;
    using static HomingGun.GunData;

    public class Gun : MonoBehaviour
    {
        [SerializeField] Ammo AmmoPrefab;

        [Header("Optional")]
        [SerializeField] Transform CustomFirePos;

        class Baker : Baker<Gun>
        {
            public override void Bake(Gun authoring)
            {
                DependsOn(authoring.AmmoPrefab);

                var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

                var gunData = new GunData();
                gunData.AmmoPrefab = GetEntity(authoring.AmmoPrefab, TransformUsageFlags.Dynamic);

                if (authoring.CustomFirePos != null)
                {
                    gunData.SpawnPosRot = GetEntity(authoring.CustomFirePos, TransformUsageFlags.Dynamic);
                }
                else
                {
                    gunData.SpawnPosRot = baseEntity;
                }
                gunData.SpawnScale = authoring.AmmoPrefab.transform.localScale.x;

                AddComponent(baseEntity, gunData);
                SetComponentEnabled<GunData>(baseEntity, false);
            }
        }
        public static IAction[] GetPattern(GunPatternSelect select, float power)
        {
            IAction[] bulletPattern;
            switch (select)
            {
                case GunPatternSelect.Simple:
                    bulletPattern = new IAction[1]
                    {
                        new TransformAction
                        {
                            Duration = 0.1f,
                            StartTime = 0,

                            Action = null,
                            ActionSpeed = power,
                            IsDeltaAction = true
                        },
                    };

                    return bulletPattern;
                case GunPatternSelect.DistanceProximity:
                    bulletPattern = new IAction[1]
                    {
                        new TransformAction
                        {
                            Duration = 0.1f,
                            StartTime = 0,

                            Action = null,
                            ActionSpeed = power,
                            IsDeltaAction = true
                        },
                    };

                    return bulletPattern;
                case GunPatternSelect.LimitedProximity:
                    return BulletPatterns.Straight(power);
                case GunPatternSelect.Accelerated:
                    return BulletPatterns.Straight(power);
                default:
                    throw new NotImplementedException();
            }
        }
        
    }

    public struct GunData : IComponentData, IEnableableComponent
    {
        // Set by Baker
        public Entity AmmoPrefab;
        public Entity SpawnPosRot;
        public float SpawnScale;

        // Set by Init System
        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public int CurrentMagazineCount;
        public int CurrentAmmoCount;
        public float ShootTimer;
        public float ReloadTimer;

        public void Setup(GunStatsStruct GunStats, GunPatternSelect PatternSelect)
        {
            this.GunStats = GunStats;
            this.PatternSelect = PatternSelect;

            CurrentMagazineCount = GunStats.MagazineCount;
            CurrentAmmoCount = GunStats.MagazineCapacity;
            ShootTimer = -GunStats.StartShootDelay;
            ReloadTimer = 0;
        }

        public enum GunPatternSelect
        {
            Simple,
            DistanceProximity,
            LimitedProximity,
            Accelerated
        }
    }

    public struct GunHomingData : IComponentData
    {
        [Header("Homing Properties")]
        public Entity HomingEntity;
        public float HomingRate;

        [Header("Distance Proximity Properties")]
        public float ProximityDistance;

        [Header("Limited Proximity Properties")]
        public float LimitedProximityFactor;

        [Header("Accelerated Properties")]
        public float AcceleratedRadius;
        public float AccelerationMulti;
    }
}
