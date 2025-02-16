namespace HomingGun
{
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [DisableAutoCreation]
    public partial struct MoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { }
        public void OnDestroy(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var Time = (float)SystemAPI.Time.ElapsedTime;
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<MoveData>())
            {
                var transform = localTransform.ValueRO;
                var updown = Mathf.Sin(Time) * 2;
                transform.Position = new float3(0, updown, 0);

                localTransform.ValueRW = transform;
            }
        }
    }
}