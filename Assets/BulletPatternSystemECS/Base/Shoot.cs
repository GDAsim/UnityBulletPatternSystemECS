using Unity.Entities;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Ammo AmmoPrefab;

    [Header("Optional")]
    [SerializeField] Transform CustomFirePos;

    [Header("Shoot Properties")]
    [SerializeField] ShootStats stats;
    [SerializeField] float ShootPower = 2;

    //void Start()
    //{
    //    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

    //    var thisEntity = entityManager.CreateEntity();
    //    entityManager.SetName(thisEntity, name);

    //    // Add basic Components
    //    entityManager.AddComponent<LocalTransform>(thisEntity);

    //    var shootData = new ShootData();
    //    shootData.SetStats(stats, 0);
    //    entityManager.AddComponentObject(thisEntity, shootData);

    //    var shootData2 = new ShootData2();
    //    shootData2.Patterns = BulletPatterns.Straight(2);
    //    entityManager.AddComponentObject(thisEntity, shootData2);
    //}
    class Baker : Baker<Shoot>
    {
        public override void Bake(Shoot authoring)
        {
            DependsOn(authoring.stats);

            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

            var shootData = new ShootData();
            shootData.AmmoPrefab = GetEntity(authoring.AmmoPrefab, TransformUsageFlags.Dynamic);
            if (authoring.CustomFirePos != null)
            {
                shootData.SpawnTransform = GetEntity(authoring.CustomFirePos, TransformUsageFlags.Dynamic);
            }
            else
            {
                shootData.SpawnTransform = baseEntity;
            }

            ////HELP DOESNT WORK
            //shootData.Patterns = BulletPatterns.Straight(2);

            shootData.SetStats(authoring.stats, 0);

            AddComponentObject(baseEntity, shootData);

            //var shootData2 = new ShootData2();
            //shootData2.Patterns = Straight(2);
            //AddComponentObject(baseEntity, shootData2);
        }

        public IAction[] Straight(float actionSpeed) => new IAction[1]
        {
            new TransformAction
            {
                Duration = 1111111,
                StartTime = 0,

                Action = MoveForward,
                ActionSpeed = actionSpeed,
                IsDeltaAction = true,
            }
        };

        public static TransformData MoveForward(TransformData startData, float speed, float time)
        {
            var forward = startData.Rotation * Vector3.forward * (speed * time);

            startData.Position = startData.Position + forward;

            return startData;
        }
    }
}
public class ShootData2 : IComponentData
{
    public IAction[] Patterns;
}

public class ShootData : IComponentData
{
    public Entity AmmoPrefab;
    public Entity SpawnTransform;

    public IAction[] Patterns;

    public int MagazineCount; // How many times can this weapon reload
    public int MagazineCapacity; // How many Ammo per reload 
    public float ReloadDelay; // How long the reload takes
    public float ShootDelay; // How long the shooting takes

    public int CurrentMagazineCount;
    public int CurrentAmmoCount;
    public float ShootTimer;
    public float ReloadTimer;

    public void SetStats(ShootStats stats, float StartShootDelay)
    {
        MagazineCount = stats.MagazineCount;
        MagazineCapacity = stats.MagazineCapacity;
        ReloadDelay = stats.ReloadDelay;
        ShootDelay = stats.ShootDelay;

        CurrentMagazineCount = stats.MagazineCount;
        CurrentAmmoCount = stats.MagazineCapacity;
        ShootTimer = -StartShootDelay;
        ReloadTimer = 0;
    }
}