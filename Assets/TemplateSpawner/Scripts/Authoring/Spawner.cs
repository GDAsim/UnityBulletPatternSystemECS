namespace TemplateSpawner
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public class Spawner : MonoBehaviour
    {
        // Add MonoBehaviour Info here to transfer to Entity World
        public GameObject SpawnPrefab;
        public float SpawnRate;

        [Header("Optional")]
        [Tooltip("Custom Spawn Position & Rotation")]
        public Transform SpawnTransform;

        class Baker : Baker<Spawner>
        {
            public override void Bake(Spawner authoring)
            {
                // Transform the GameObject into Entity with transform data
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

    public struct SpawnerData : IComponentData
    {
        public Entity EntitySpawnPrefab;
        public float3 SpawnPosition;
        public float SpawnRate;

        public float NextSpawnTime;
    }
}