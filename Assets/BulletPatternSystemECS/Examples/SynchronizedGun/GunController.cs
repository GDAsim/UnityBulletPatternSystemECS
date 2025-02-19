namespace SynchronizedGun
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    public class GunController : MonoBehaviour
    {
        [SerializeField] GunStats baseStats;
        [SerializeField] Gun Gun;

        [SerializeField] PatternSelect patternSelect;

        [SerializeField] Transform Pos1;
        [SerializeField] Transform Pos2;
        [SerializeField] Transform Pos3;
        [SerializeField] Transform Pos4;

        enum PatternSelect { ShootMoveSync, BulletMoveSync }
        class Baker : Baker<GunController>
        {
            public override void Bake(GunController authoring)
            {
                GunData.GunPatternSelect gunPatternSelect;
                switch (authoring.patternSelect)
                {
                    case PatternSelect.ShootMoveSync:
                        gunPatternSelect = GunData.GunPatternSelect.ShootMoveSync;
                        break;
                    case PatternSelect.BulletMoveSync:
                        gunPatternSelect = GunData.GunPatternSelect.BulletMoveSync;
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

                    WithEntities = new Entity[]
                    {
                        GetEntity(authoring.Pos1, TransformUsageFlags.Dynamic),
                        GetEntity(authoring.Pos2, TransformUsageFlags.Dynamic),
                        GetEntity(authoring.Pos3, TransformUsageFlags.Dynamic),
                        GetEntity(authoring.Pos4, TransformUsageFlags.Dynamic),
                    }
                });
            }
        }
    }
}