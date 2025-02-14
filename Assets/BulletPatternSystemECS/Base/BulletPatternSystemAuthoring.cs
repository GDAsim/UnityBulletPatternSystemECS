using UnityEngine;
using Unity.Entities;
using SimpleGun;

public class BulletPatternSystemAuthoring : MonoBehaviour
{
    class Baker : Baker<BulletPatternSystemAuthoring>
    {
        public override void Bake(BulletPatternSystemAuthoring authoring)
        {
            var gunInitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<GunInitSystem>();


            var shootSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<GunSystem>();
            var ammoSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoSystem>();
            var ammoinitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoInitSystem>();

            var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

            // ===========================  SimulationSystemGroup       ===========================
            SimSG.AddSystemToUpdateList(gunInitSystemHandle);


            SimSG.AddSystemToUpdateList(shootSystemHandle);
            SimSG.AddSystemToUpdateList(ammoSystemHandle);
            SimSG.AddSystemToUpdateList(ammoinitSystemHandle);
        }
    }
}