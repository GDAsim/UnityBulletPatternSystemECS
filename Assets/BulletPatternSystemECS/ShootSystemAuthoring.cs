using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SimpleShootSystemAuthoring : MonoBehaviour
{
    [SerializeField] ShootStats BaseShootSystemStats;
    [SerializeField] GameObject AmmoPrefab;

    [Header("Optional")]
    [SerializeField] Transform SpawnTransform;
    ShootStats systemStats;

    class Baker : Baker<SimpleShootSystemAuthoring>
    {
        public override void Bake(SimpleShootSystemAuthoring authoring)
        {
            // Transform the GameObject into Entity with transform data
            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

            // Add additional Components
            //authoring.transform.GetPositionAndRotation(out var spawnPos, out var spawnRot);
            //if (authoring.SpawnTransform != null)
            //{
            //    spawnPos = authoring.SpawnTransform.position;
            //    spawnRot = authoring.SpawnTransform.rotation;
            //}
            //var data = new BulletSpawnerData()
            //{
            //    EntityBulletPrefab = GetEntity(authoring.AmmoPrefab, TransformUsageFlags.Dynamic),
            //    SpawnPosition = spawnPos,
            //    SpawnRotation = spawnRot,
            //    SpawnRate = authoring.SpawnRate,
            //};

            //AddComponent(baseEntity, data);
        }
    }
}


public struct ShootComponentData3 : IComponentData
{
    public Entity EntityBulletPrefab;
    public float3 SpawnPosition;
    public quaternion SpawnRotation;
    public float SpawnRate;

    public float NextSpawnTime;
}

