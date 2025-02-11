
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[DisableAutoCreation]
public partial struct ShootSystem : ISystem
{
    public void OnCreate(ref SystemState state) { }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        DoJobs(ref state, ref ecb);
    }

    void Do(ref SystemState state)
    {

    }

    void DoJobs(ref SystemState state, ref EntityCommandBuffer ecb)
    {
        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        new BulletSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb.AsParallelWriter()
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct BulletSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;

    // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    // This example queries for all Spawner components and uses `ref` to specify that the operation
    // requires read and write access. Unity processes `Execute` for each entity that matches the
    // component data query.
    private void Execute([ChunkIndexInQuery] int chunkIndex, ref ShootComponentData3 spawner)
    {
        if (spawner.NextSpawnTime < ElapsedTime)
        {
            // Spawns a new entity and positions it at the spawner.
            Entity newBulletEntity = Ecb.Instantiate(chunkIndex, spawner.EntityBulletPrefab);

            float3 dir = math.mul(spawner.SpawnRotation, new float3(0, 0, 1));
            var pos = spawner.SpawnPosition + dir * (float)ElapsedTime;
            Ecb.SetComponent(chunkIndex, newBulletEntity, LocalTransform.FromPosition(pos));

            // Resets the next spawn time.
            spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
        }
    }
}
