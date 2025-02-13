using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[DisableAutoCreation]
public partial class ShootSpawnerSystem : SystemBase
{
    protected override void OnCreate() { }
    protected override void OnDestroy() { }
    protected override void OnUpdate()
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        //Entities
        //    .WithName("Spawn")
        //    .WithAll<LocalToWorld>() // Entities that have this Component
        //    .ForEach((
        //        Entity e,
        //        ref LocalTransform transform, in ShootSpawnerData data) =>
        //    {
        //        Entity ammoEntity = ecb.Instantiate(data.Prefab);
        //        ecb.SetComponent(ammoEntity, LocalTransform.FromPosition(data.Position));
        //        ecb.RemoveComponent<ShootSpawnerData>(e);
        //    })
        //    .WithoutBurst()
        //    .Run();
    }
}