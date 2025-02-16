namespace HomingGun
{
    using UnityEngine;
    using Unity.Entities;

    public class BulletPatternSystemAuthoring : MonoBehaviour
    {
        class Baker : Baker<BulletPatternSystemAuthoring>
        {
            public override void Bake(BulletPatternSystemAuthoring authoring)
            {
                var gunInitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<GunInitSystem>();
                var gunSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<GunSystem>();
                var ammoinitSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoInitSystem>();
                var ammoSystemHandle = World.DefaultGameObjectInjectionWorld.CreateSystem<AmmoSystem>();

                var aa = World.DefaultGameObjectInjectionWorld.CreateSystem<MoveSystem>();

                

                var SimSG = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>();

                // ===========================  SimulationSystemGroup       ===========================
                SimSG.AddSystemToUpdateList(gunInitSystemHandle);
                SimSG.AddSystemToUpdateList(gunSystemHandle);
                SimSG.AddSystemToUpdateList(ammoinitSystemHandle);
                SimSG.AddSystemToUpdateList(ammoSystemHandle);

                SimSG.AddSystemToUpdateList(aa);

            }
        }
    }
}