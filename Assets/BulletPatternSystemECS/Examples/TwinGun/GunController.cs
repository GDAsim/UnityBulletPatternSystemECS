namespace TwinGun
{
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] ShootMode shootmode = ShootMode.Cycle;

        [SerializeField] Gun RightGun;
        [SerializeField] Gun LeftGun;

        enum ShootMode { Normal, Cycle, Helix }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.shootmode == ShootMode.Normal)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.RightGun, TransformUsageFlags.Dynamic),
                    });

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityB, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.LeftGun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.shootmode == ShootMode.Cycle)
                {
                    
                }
                else if (authoring.shootmode == ShootMode.Helix)
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