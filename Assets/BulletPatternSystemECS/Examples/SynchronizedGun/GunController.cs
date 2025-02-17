namespace SynchronizedGun
{
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] SyncType snycType;

        [SerializeField] Gun Gun;

        [SerializeField] Transform Pos1;
        [SerializeField] Transform Pos2;
        [SerializeField] Transform Pos3;
        [SerializeField] Transform Pos4;

        enum SyncType { ShootMoveSync, BulletMoveSync }
        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                if (authoring.snycType == SyncType.ShootMoveSync)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.ShootMoveSync,
                        GunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.snycType == SyncType.BulletMoveSync)
                {
                    
                }
            }
        }
    }
    public struct GunSetupData : IComponentData
    {
        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public Entity GunEntity;
    }
}