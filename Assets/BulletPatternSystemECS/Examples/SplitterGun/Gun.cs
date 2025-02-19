namespace SplitterGun
{
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

                AddComponentObject(baseEntity, gunData);
            }
        }
    }
    public class GunSetupData : IComponentData
    {
        public GunStatsStruct GunStats;
        public GunData.GunPatternSelect PatternSelect;
        public Entity[] WithEntities;

        public Entity GunEntity;
    }
    public class GunData : IComponentData
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
        public DelayAction CurrentDelayAction;
        public Entity[] WithEntities;
        public bool DelayUntil;

        public float CurrentActionTimer;

        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public int CurrentMagazineCount;
        public int CurrentAmmoCount;
        public float ShootTimer;
        public float ReloadTimer;

        public int TotalShootCount;

        public void Setup(GunStatsStruct gunStats, GunPatternSelect gunPatternSelect)
        {
            GunStats = gunStats;
            PatternSelect = gunPatternSelect;

            CurrentMagazineCount = gunStats.MagazineCount;
            CurrentAmmoCount = gunStats.MagazineCapacity;
            ShootTimer = -gunStats.StartShootDelay;
            ReloadTimer = 0;
        }

        public enum GunPatternSelect
        {
            Extra, // Spawn a new predefined object
            Split, // Spawn a new predefined object and Destroy Itself
            Clone, // Spawn a new object as itself
            Copy // Spawn a new object as itself and copy current state over
        }
    }
}
