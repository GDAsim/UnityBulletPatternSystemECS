namespace SplitterGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun gun;

        [SerializeField] PatternSelect patternSelect;

        [Header("Extra")]
        [SerializeField] Ammo ExtraAmmoPrefab;

        [Header("Split")]
        [SerializeField] Ammo SplitAmmoPrefab;

        [Header("Clone")]
        [SerializeField] Ammo CloneAmmoPrefab;

        enum PatternSelect
        {
            Extra, // Spawn a new predefined object
            Split, // Spawn a new predefined object and Destroy Itself
            Clone, // Spawn a new object as itself
            Copy // Spawn a new object as itself and copy current state over
        }

        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                DependsOn(authoring.baseStats);

                GunData.GunPatternSelect gunPatternSelect;
                switch (authoring.patternSelect)
                {
                    case PatternSelect.Extra:
                        gunPatternSelect = GunData.GunPatternSelect.Extra;
                        break;
                    case PatternSelect.Split:
                        gunPatternSelect = GunData.GunPatternSelect.Split;
                        break;
                    case PatternSelect.Clone:
                        gunPatternSelect = GunData.GunPatternSelect.Clone;
                        break;
                    case PatternSelect.Copy:
                        gunPatternSelect = GunData.GunPatternSelect.Copy;
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

                    WithEntities = new Entity[]
                    {
                        GetEntity(authoring.ExtraAmmoPrefab, TransformUsageFlags.Dynamic),
                        GetEntity(authoring.SplitAmmoPrefab, TransformUsageFlags.Dynamic),
                        GetEntity(authoring.CloneAmmoPrefab, TransformUsageFlags.Dynamic),
                    }
                });
            }
        }
    }
}