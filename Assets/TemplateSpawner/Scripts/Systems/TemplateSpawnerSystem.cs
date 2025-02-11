namespace TemplateSpawner
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Transforms;

    [BurstCompile]
    [DisableAutoCreation]
    public partial struct TemplateSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Create ECB Method 1 - Create From System - https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/systems-entity-command-buffer-automatic-playback.html
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Create ECB Method 2 - Create Manual Adhoc - https://docs.unity3d.com/Packages/com.unity.entities@1.3/manual/systems-entity-command-buffer-use.html
            // Manually do ecs.Playback() and ecb.Dispose()
            //var ecb = new EntityCommandBuffer(Allocator.TempJob);

            DoSystemAPIForeach(ref state, ref ecb);
            //DoJobs(ref state, ref ecb);

            // For ECB Method 2
            //state.Dependency.Complete();
            //ecb.Playback(state.EntityManager);
            //ecb.Dispose();
        }

        void DoSystemAPIForeach(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            var ElapsedTime = SystemAPI.Time.ElapsedTime;

            foreach (var spawnerDataRW in SystemAPI.Query<RefRW<SpawnerData>>())
            {
                var data = spawnerDataRW.ValueRO;

                if (data.NextSpawnTime < ElapsedTime)
                {
                    // Spawns a new entity and positions it at the spawner.
                    Entity newBulletEntity = ecb.Instantiate(data.EntitySpawnPrefab);

                    // Set Position
                    var pos = data.SpawnPosition;
                    ecb.SetComponent(newBulletEntity, LocalTransform.FromPosition(pos));

                    // Resets the next spawn time.
                    data.NextSpawnTime = (float)ElapsedTime + data.SpawnRate;
                }

                spawnerDataRW.ValueRW = data;
            }
        }

        void DoJobs(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            new TemplateJob
            {
                ElapsedTime = SystemAPI.Time.ElapsedTime,
                Ecb = ecb.AsParallelWriter()
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct TemplateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;

        // IJobEntity generates a component data query based on the parameters of its `Execute` method.
        // This example queries for all Spawner components and uses `ref` to specify that the operation
        // requires read and write access. Unity processes `Execute` for each entity that matches the
        // component data query.
        void Execute([ChunkIndexInQuery] int chunkIndex, ref SpawnerData data)
        {
            if (data.NextSpawnTime < ElapsedTime)
            {
                // Spawns a new entity and positions it at the spawner.
                Entity newBulletEntity = Ecb.Instantiate(chunkIndex, data.EntitySpawnPrefab);

                var pos = data.SpawnPosition;
                Ecb.SetComponent(chunkIndex, newBulletEntity, LocalTransform.FromPosition(pos));

                // Resets the next spawn time.
                data.NextSpawnTime = (float)ElapsedTime + data.SpawnRate;
            }
        }
    }
}
