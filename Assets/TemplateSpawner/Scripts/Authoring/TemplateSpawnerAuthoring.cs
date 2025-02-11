namespace TemplateSpawner
{
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Scene Authoring Script
    /// </summary>
    public class TemplateSpawnerAuthoring : MonoBehaviour
    {
        class Baker : Baker<TemplateSpawnerAuthoring>
        {
            public override void Bake(TemplateSpawnerAuthoring authoring)
            {
                // 1. Create the initial systems in the world
                var templateSpawnerSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<TemplateSpawnerSystem>();

                // 2. Find Existing SystemGroup to insert the system into
                var InitSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<InitializationSystemGroup>();
                var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();
                var PresentSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PresentationSystemGroup>();

                // 3. Add System to Appropriate Group

                // ========================  InitializationSystemGroup   ==============================

                // ===========================  SimulationSystemGroup       ===========================
                SimSG.AddSystemToUpdateList(templateSpawnerSystemHandle);

                // ===========================  PresentationSystemGroup  ===========================
            }
        }
    }
}