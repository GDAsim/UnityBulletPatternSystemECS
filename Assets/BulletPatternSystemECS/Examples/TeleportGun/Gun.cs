namespace TeleportGun
{
    using System;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using static TeleportGun.GunData;

    public class Gun : MonoBehaviour
    {
        [SerializeField] Ammo AmmoPrefab;

        [Header("Optional")]
        [SerializeField] Transform CustomFirePos;

        class Baker : Baker<Gun>
        {
            public override void Bake(Gun authoring)
            {
                DependsOn(authoring.AmmoPrefab);

                var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

                var gunData = new GunData();
                gunData.AmmoPrefab = GetEntity(authoring.AmmoPrefab, TransformUsageFlags.Dynamic);

                if (authoring.CustomFirePos != null)
                {
                    gunData.SpawnPosRot = GetEntity(authoring.CustomFirePos, TransformUsageFlags.Dynamic);
                }
                else
                {
                    gunData.SpawnPosRot = baseEntity;
                }
                gunData.SpawnScale = authoring.AmmoPrefab.transform.localScale.x;

                AddComponent(baseEntity, gunData);
                SetComponentEnabled<GunData>(baseEntity, false);
            }
        }
        public static IAction[] GetBulletPattern(GunPatternSelect select, float power)
        {
            IAction[] bulletPattern = null;

            switch (select)
            {
                case GunPatternSelect.InstantAction:
                    bulletPattern = new IAction[4]
                    {
                        new TransformAction
                        {
                            Duration = 1,
                            StartTime = 0,

                            Action = TransformAction.MoveForward,
                            ActionSpeed = power,
                            IsDeltaAction = true,
                        },
                        new TransformAction
                        {
                            Action = InstantTeleportRight,
                            ActionSpeed = power,
                            IsDeltaAction = false,
                        },
                        new TransformAction
                        {
                            Duration = 1,
                            StartTime = 0,

                            Action = TransformAction.MoveForward,
                            ActionSpeed = power,
                            IsDeltaAction = true,
                        },
                        new TransformAction
                        {
                            Action = InstantTeleportLeft,
                            ActionSpeed = power,
                            IsDeltaAction = false,
                        },
                    };
                    return bulletPattern;
                case GunPatternSelect.JumpAction:
                    bulletPattern = new IAction[2]
                {
                    new TransformAction
                    {
                        Duration = 1,
                        StartTime = 0,

                        Action = TransformAction.MoveForward,
                        ActionSpeed = power,
                        IsDeltaAction = true,
                    },
                    new TransformAction
                    {
                        Duration = 1,
                        StartTime = 1,

                        Action = TransformAction.MoveForward,
                        ActionSpeed = power,
                        IsDeltaAction = false,
                    },
                };
                    return bulletPattern;

                default:
                    throw new NotImplementedException();
            }

            TransformData InstantTeleportRight(TransformData startData, float speed, float time)
            {
                var right = math.mul(startData.Rotation, Vector3.right);

                startData.Position = startData.Position + right;

                return startData;
            }
            TransformData InstantTeleportLeft(TransformData startData, float speed, float time)
            {
                var left = math.mul(startData.Rotation, Vector3.left);

                startData.Position = startData.Position + left;

                return startData;
            }
        }
    }

    public struct GunData : IComponentData, IEnableableComponent
    {
        // Set by Baker
        public Entity AmmoPrefab;
        public Entity SpawnPosRot;
        public float SpawnScale;

        // Set by Init System
        public GunStatsStruct GunStats;
        public GunPatternSelect PatternSelect;

        public int CurrentMagazineCount;
        public int CurrentAmmoCount;
        public float ShootTimer;
        public float ReloadTimer;

        public void Setup(GunStatsStruct GunStats, GunPatternSelect PatternSelect)
        {
            this.GunStats = GunStats;
            this.PatternSelect = PatternSelect;

            CurrentMagazineCount = GunStats.MagazineCount;
            CurrentAmmoCount = GunStats.MagazineCapacity;
            ShootTimer = -GunStats.StartShootDelay;
            ReloadTimer = 0;
        }
        public enum GunPatternSelect
        {
            InstantAction,
            JumpAction,
        }
    }
}
