using UnityEngine;
using Unity.Entities;

public class BulletPatternSystemAuthoring : MonoBehaviour
{
    class Baker : Baker<BulletPatternSystemAuthoring>
    {
        public override void Bake(BulletPatternSystemAuthoring authoring)
        {
            //var shootSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<ShootSystem>();
            var ammoSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoSystem>();
            var ammoinitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoInitSystem>();

            var shootsSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<ShootSpawnerSystem>();


            var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

            // ===========================  SimulationSystemGroup       ===========================
            //SimSG.AddSystemToUpdateList(shootSystemHandle);
            SimSG.AddSystemToUpdateList(ammoSystemHandle);
            SimSG.AddSystemToUpdateList(ammoinitSystemHandle);

            SimSG.AddSystemToUpdateList(shootsSystemHandle);
        }
    }
}