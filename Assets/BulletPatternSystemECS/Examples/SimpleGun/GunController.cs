namespace SimpleGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun gun;

        [SerializeField] PatternSelect patternSelect;

        enum PatternSelect { Straight, Sine }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                GunData.GunPatternSelect gunPatternSelect;
                switch (authoring.patternSelect)
                {
                    case PatternSelect.Straight:
                        gunPatternSelect = GunData.GunPatternSelect.Straight;
                        break;
                    case PatternSelect.Sine:
                        gunPatternSelect = GunData.GunPatternSelect.Sine;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(EntityA, new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = gunPatternSelect,
                    GunEntity = GetEntity(authoring.gun, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}