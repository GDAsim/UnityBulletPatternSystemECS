namespace Examples.BulletHell
{
    using UnityEngine;
    using Unity.Entities;

    /// <summary>
    /// Main Scene Authoring Script
    /// </summary>
    public class BulletHellAuthoring : MonoBehaviour
    {
        class Baker : Baker<BulletHellAuthoring>
        {
            public override void Bake(BulletHellAuthoring authoring)
            {
                // 1. Create the initial systems in the world
                var bulletSpawnerSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<BulletSpawnerSystem>();

                // 2. Find Existing SystemGroup to insert the system into
                var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

                // 3. Add System to Appropriate Group

                // ===========================  SimulationSystemGroup       ===========================
                SimSG.AddSystemToUpdateList(bulletSpawnerSystemHandle);
            }
        }
    }
}