using UnityEngine;
using Unity.Entities;

public class BulletPatternSystemAuthoring : MonoBehaviour
{
    class Baker : Baker<BulletPatternSystemAuthoring>
    {
        public override void Bake(BulletPatternSystemAuthoring authoring)
        {
            var shootSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<ShootSystem>();

            var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

            // ===========================  SimulationSystemGroup       ===========================
            SimSG.AddSystemToUpdateList(shootSystemHandle);
        }
    }
}