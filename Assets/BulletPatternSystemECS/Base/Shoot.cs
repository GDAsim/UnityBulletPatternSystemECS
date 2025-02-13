using Unity.Entities;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] Ammo AmmoPrefab;

    [Header("Optional")]
    [SerializeField] Transform CustomFirePos;

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
            //currentAction.ReadyAction(transform);
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

            //shootData.BulletPattern = BulletPatterns.Straight(2);

            shootData.SetStats(authoring.stats, 0);

            AddComponentObject(baseEntity, shootData);
        }
    }
}

public class ShootData : IComponentData
{
    public Entity AmmoPrefab;
    public Entity SpawnTransform;

    //public IAction[] BulletPattern;

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