namespace SimpleGun
{
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun gun;
        [SerializeField] GunPatternSelect PatternSelect;

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);
                
                var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                AddComponent(EntityA, new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = authoring.PatternSelect,
                    GunEntity = GetEntity(authoring.gun, TransformUsageFlags.Dynamic),
                });
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