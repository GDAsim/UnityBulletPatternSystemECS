using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[DisableAutoCreation]
public partial struct AmmoSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //DoJobs(ref state, ref ecb, ref localTransformLU);
    }

    void DoJobs(ref SystemState state,
        ref EntityCommandBuffer ecb,
        ref ComponentLookup<LocalTransform> localTransformLU)
    {
        localTransformLU.Update(ref state);

        new BulletSpawnerJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,

            Ecb = ecb,

            localTransformLU = localTransformLU,
        }.Schedule();
    }
}