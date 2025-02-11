using TemplateSpawner;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Ammo AmmoPrefab;

    [Header("Shoot Properties")]
    [SerializeField] ShootStats stats;
    [SerializeField] float ShootPower = 2;

    int currentMagazineCount;
    int currentAmmoCount;
    float shootTimer;
    float reloadTimer;
    bool HaveAmmo => currentAmmoCount > 0;
    bool HaveMag => currentMagazineCount > 0;
    public int TotalShootCount { get; set; }

    TransformAction[] systemPattern;
    int currentIndex = 0;

    TransformAction currentAction;
    float actionTimer = 0;

    IAction[] bulletPattern;
    Transform firetransform;

    void Awake()
    {
        var bulletPattern = BulletPatterns.Straight(ShootPower);
        SetupShoot(bulletPattern, stats);
    }

    public void SetupPreShoot(TransformAction[] systemPattern,
        float StartSystemDelay = 0)
    {
        this.systemPattern = systemPattern;

        currentAction = systemPattern[currentIndex++];
        currentAction.ReadyAction(transform);
        actionTimer = -StartSystemDelay;
        if (currentIndex == systemPattern.Length) currentIndex = 0;
    }
    public void SetupShoot(IAction[] bulletPattern, ShootStats shootStats,
        float StartShootDelay = 0)
    {
        this.bulletPattern = bulletPattern;
        this.stats = shootStats;

        firetransform = transform;

        currentMagazineCount = stats.MagazineCount;
        currentAmmoCount = stats.MagazineCapacity;
        shootTimer = -StartShootDelay;
        reloadTimer = 0;
    }
    void Update()
    {
        PreShootAction();

        if (HaveAmmo)
        {
            AttemptShoot();
        }
        else
        {
            if (HaveMag)
            {
                AttemptReload();
            }
        }
    }

    void PreShootAction()
    {
        if (systemPattern == null || systemPattern.Length == 0)
        {
            //Debug.LogWarning("No System Pattern Set", this);
            return;
        }

        var dt = Time.deltaTime;

        if (actionTimer >= currentAction.Duration && currentAmmoCount > 0)
        {
            currentAction.EndAction();

            currentAction = systemPattern[currentIndex++];
            currentAction.ReadyAction(transform);
            actionTimer = 0;
            if (currentIndex == systemPattern.Length) currentIndex = 0;
        }

        currentAction.DoAction(dt);
        actionTimer += dt;
    }

    void AttemptShoot()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= stats.ShootDelay)
        {
            shootTimer = 0;

            Fire();
        }
    }
    void Fire()
    {
        firetransform.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);

        var ammo = Instantiate(AmmoPrefab, pos, rot);

        ammo.Setup(bulletPattern);

        currentAmmoCount--;

        TotalShootCount++;
    }

    void AttemptReload()
    {
        reloadTimer += Time.deltaTime;

        if (reloadTimer >= stats.ReloadDelay)
        {
            reloadTimer -= stats.ReloadDelay;

            Reload();
        }
    }
    void Reload()
    {
        currentMagazineCount--;
        currentAmmoCount = stats.MagazineCapacity;
    }

    class Baker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

            // Add additional Components
            var spawnPos = authoring.transform.position;
            if (authoring.SpawnTransform != null)
            {
                spawnPos = authoring.SpawnTransform.position;
            }
            var data = new SpawnerData()
            {
                EntitySpawnPrefab = GetEntity(authoring.SpawnPrefab, TransformUsageFlags.Dynamic),
                SpawnPosition = spawnPos,
                SpawnRate = authoring.SpawnRate,
            };
            AddComponent(baseEntity, data);
        }
    }
}

public struct ShootComponentData : IComponentData
{
    public Entity EntityBulletPrefab;
    public float3 SpawnPosition;
    public quaternion SpawnRotation;
    public float SpawnRate;

    public float NextSpawnTime;


    public int MagazineCount; // How many times can this weapon reload
    public int MagazineCapacity; // How many Ammo per reload 
    public float ReloadDelay; // How long the reload takes
    public float ShootDelay; // How long the shooting takes

    public static ShootComponentData Convert(ShootStats stats)
    {
        return new ShootComponentData()
        {
            MagazineCount = stats.MagazineCount,
            MagazineCapacity = stats.MagazineCapacity,
            ReloadDelay = stats.ReloadDelay,
            ShootDelay = stats.ShootDelay,
        };
    }
}