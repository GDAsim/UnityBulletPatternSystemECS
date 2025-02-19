namespace SynchronizedGun
{
    using UnityEngine;
    using Unity.Entities;

    public class BulletPatternSystemAuthoring : MonoBehaviour
    {
        class Baker : Baker<BulletPatternSystemAuthoring>
        {
            public override void Bake(BulletPatternSystemAuthoring authoring)
            {
                var gunInitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystemManaged<GunInitSystem>();
                var gunSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystemManaged<GunSystem>();
                var ammoinitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystemManaged<AmmoInitSystem>();
                var ammoSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystemManaged<AmmoSystem>();

                var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

                // ===========================  SimulationSystemGroup       ===========================
                SimSG.AddSystemToUpdateList(gunInitSystemHandle);
                SimSG.AddSystemToUpdateList(gunSystemHandle);
                SimSG.AddSystemToUpdateList(ammoinitSystemHandle);
                SimSG.AddSystemToUpdateList(ammoSystemHandle);
            }
        }
    }
}