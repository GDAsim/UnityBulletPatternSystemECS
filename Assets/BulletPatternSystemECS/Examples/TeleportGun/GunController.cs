namespace TeleportGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun Gun;

        [SerializeField] PatternSelect patternSelect;

        enum PatternSelect
        {
            InstantAction, // Apply One Time
            JumpAction // Start Action with a StartTime
        }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                GunData.GunPatternSelect gunPatternSelect;
                switch (authoring.patternSelect)
                {
                    case PatternSelect.InstantAction:
                        gunPatternSelect = GunData.GunPatternSelect.InstantAction;
                        break;
                    case PatternSelect.JumpAction:
                        gunPatternSelect = GunData.GunPatternSelect.JumpAction;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                
                var EntityA = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(EntityA, new GunSetupData
                {
                    GunStats = authoring.baseStats.GetStruct(),
                    PatternSelect = gunPatternSelect,
                    GunEntity = GetEntity(authoring.Gun, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}