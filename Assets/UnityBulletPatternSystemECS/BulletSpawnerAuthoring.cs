namespace Examples.BulletHell
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class BulletSpawnerAuthoring : MonoBehaviour
    {
        // Add MonoBehaviour Info here to transfer to Entity World
        public GameObject BulletPrefab;
        public float SpawnRate;

        [Header("Optional")]
        [Tooltip("Custom Spawn Position & Rotation")]
        public Transform SpawnTransform;

        class Baker : Baker<BulletSpawnerAuthoring>
        {
            public override void Bake(BulletSpawnerAuthoring authoring)
            {
                // Transform the GameObject into Entity with transform data
                var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

                // Add additional Components
                authoring.transform.GetPositionAndRotation(out var spawnPos, out var spawnRot);
                if (authoring.SpawnTransform != null)
                {
                    spawnPos = authoring.SpawnTransform.position;
                    spawnRot = authoring.SpawnTransform.rotation;
                }
                var data = new BulletSpawnerData()
                {
                    EntityBulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic),
                    SpawnPosition = spawnPos,
                    SpawnRotation = spawnRot,
                    SpawnRate = authoring.SpawnRate,
                };

                AddComponent(baseEntity, data);
            }
        }
    }

    public struct BulletSpawnerData : IComponentData
    {
        public Entity EntityBulletPrefab;
        public float3 SpawnPosition;
        public quaternion SpawnRotation;
        public float SpawnRate;

        public float NextSpawnTime;
    }
}