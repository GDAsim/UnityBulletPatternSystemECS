namespace SynchronizedGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;
    using static SynchronizedGun.GunData;

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

                AddComponentObject(baseEntity, gunData);
            }
        }
    }

    public class GunData : IComponentData, IEnableableComponent
    {
        // Set by Baker
        public Entity AmmoPrefab;
        public Entity SpawnPosRot;
        public float SpawnScale;

        // Set by Init System
        public IAction[] Patterns;
        public int CurrentIndex;

        public ActionTypes CurrentActionType;
        public TransformAction CurrentTransformAction;
        public TransformWithEntitiesAction CurrentTransformWithEntitiesAction;
        public Entity[] WithEntities;

        public float CurrentActionTimer;

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
            ShootMoveSync, 
            BulletMoveSync
        }
    }
}
