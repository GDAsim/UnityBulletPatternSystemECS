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

    public Entity thisEntity;

    void Start()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.SetName(thisEntity, "HAHA123");

    }
    class Baker : Baker<Shoot>
    {
        public override void Bake(Shoot authoring)
        {
            DependsOn(authoring.stats);

            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

            //var shootData2 = new ShootData2();
            //shootData2.Patterns = BulletPatterns.Straight(2);
            //AddComponentObject<(baseEntity, shootData2);

            authoring.thisEntity = baseEntity;
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