namespace TemplateSpawner
{
    using Unity.Entities;
    using UnityEditor.PackageManager;
    using UnityEngine;

    public class SpawnerCreator : MonoBehaviour
    {
        // Add MonoBehaviour Info here to transfer to Entity World
        public GameObject SpawnPrefab;
        public float SpawnRate;

        [Header("Optional")]
        [Tooltip("Custom Spawn Position & Rotation")]
        public Transform SpawnTransform;

        //Bake manullay
        void Awake()
        {
            //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //// Create Entity using entityManager
            //var thisEntity = entityManager.CreateEntity();
            //entityManager.SetName(thisEntity, name);

            //// Add additional Components
            //var spawnPos = transform.position;
            //if (SpawnTransform != null)
            //{
            //    spawnPos = SpawnTransform.position;
            //}
            //var data = new SpawnerData()
            //{
            //    EntitySpawnPrefab = GetEntity(authoring.SpawnPrefab, TransformUsageFlags.Dynamic),
            //    SpawnPosition = spawnPos,
            //    SpawnRate = SpawnRate,
            //};
            //entityManager.AddComponentData(thisEntity, data);
        }
    }
}