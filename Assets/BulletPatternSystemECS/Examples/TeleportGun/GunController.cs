namespace TeleportGun
{
    using Unity.Entities;
    using UnityEngine;
    using static TeleportGun.GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] ShootMode shootmode;

        [SerializeField] Gun Gun;

        enum ShootMode
        {
            InstantAction, // Apply One Time
            JumpAction // Start Action with a StartTime
        }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                if (authoring.shootmode == ShootMode.InstantAction)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.InstantAction,
                        GunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic),
                    });
                }
                else if (authoring.shootmode == ShootMode.JumpAction)
                {
                    var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(EntityA, new GunSetupData
                    {
                        GunStats = authoring.baseStats.GetStruct(),
                        PatternSelect = GunPatternSelect.JumpAction,
                        GunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic),
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