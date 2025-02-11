using UnityEngine;
using Unity.Entities;

public class BulletHellAuthoring : MonoBehaviour
{
    class Baker : Baker<BulletHellAuthoring>
    {
        public override void Bake(BulletHellAuthoring authoring)
        {
            //var bulletSpawnerSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<ShootSystem>();

            //var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

            //// ===========================  SimulationSystemGroup       ===========================
            //SimSG.AddSystemToUpdateList(bulletSpawnerSystemHandle);
        }
    }
}