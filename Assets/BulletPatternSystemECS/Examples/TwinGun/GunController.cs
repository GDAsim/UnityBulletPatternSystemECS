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
                    var stats = authoring.baseStats.GetStruct();
                    stats.ShootDelay *= 2;

                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.RightGun, TransformUsageFlags.Dynamic),
                    });

                    stats.StartShootDelay = 1;

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityB, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.LeftGun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.shootmode == ShootMode.Helix)
                {
                    var stats = authoring.baseStats.GetStruct();

                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.SineRight,
                        GunEntity = GetEntity(authoring.RightGun, TransformUsageFlags.Dynamic),
                    });

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityB, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.SineLeft,
                        GunEntity = GetEntity(authoring.LeftGun, TransformUsageFlags.Dynamic),
                    });
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