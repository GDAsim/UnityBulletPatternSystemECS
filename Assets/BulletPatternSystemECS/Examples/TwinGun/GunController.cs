namespace TwinGun
{
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun rightGun;
        [SerializeField] Gun leftGun;

        [SerializeField] PatternSelect patternSelect;

        enum PatternSelect { Normal, Cycle, Helix }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                if (authoring.patternSelect == PatternSelect.Normal)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.rightGun, TransformUsageFlags.Dynamic),
                    });

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityB, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.leftGun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.patternSelect == PatternSelect.Cycle)
                {
                    var stats = authoring.baseStats.GetStruct();
                    stats.ShootDelay *= 2;

                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityA, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.rightGun, TransformUsageFlags.Dynamic),
                    });

                    stats.StartShootDelay = 1;

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityB, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.Straight,
                        GunEntity = GetEntity(authoring.leftGun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.patternSelect == PatternSelect.Helix)
                {
                    var stats = authoring.baseStats.GetStruct();

                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityA, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.SineRight,
                        GunEntity = GetEntity(authoring.rightGun, TransformUsageFlags.Dynamic),
                    });

                    var EntityB = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponentObject(EntityB, new GunSetupData
                    {
                        GunStats = stats,
                        PatternSelect = GunPatternSelect.SineLeft,
                        GunEntity = GetEntity(authoring.leftGun, TransformUsageFlags.Dynamic),
                    });
                }
            }
        }
    }
}