namespace HomingGun
{
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;

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

    [BurstCompile]
    public struct HomingData : ISharedComponentData
    {
        [Header("Homing Properties")]
        public Entity HomingEntity;
        public float HomingRate;

        [Header("Distance Proximity Properties")]
        public float ProximityDistance;

        [Header("Limited Proximity Properties")]
        public Entity GunEntity;
        public float LimitedProximityFactor;

        [Header("Accelerated Properties")]
        public float AcceleratedRadius;
        public float AccelerationMulti;
    }

    [BurstCompile]
    public struct DelayData : ISharedComponentData
    {
        public bool DelayUntil;
    }
}
