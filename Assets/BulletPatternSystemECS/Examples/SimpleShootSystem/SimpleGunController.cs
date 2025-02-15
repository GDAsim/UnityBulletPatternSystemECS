namespace SimpleGun
{
    using Unity.Entities;
    using UnityEngine;
    using static GunData;

    public class SimpleGunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun gun;
        [SerializeField] GunPatternSelect PatternSelect;

        class Baker : Baker<SimpleGunController>
        {
            public override void Bake(SimpleGunController authoring)
            {
                DependsOn(authoring.baseStats);

                var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

                var gunSetupData = new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = authoring.PatternSelect,
                    GunEntity = GetEntity(authoring.gun, TransformUsageFlags.Dynamic),
                };

                AddComponent(baseEntity, gunSetupData);
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