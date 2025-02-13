using AOT;
using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using static ShootSpawner;

public class ShootSpawner : MonoBehaviour
{
    class Baker : Baker<ShootSpawner>
    {
        public override void Bake(ShootSpawner authoring)
        {
            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);

            var data = new ShootSpawnerData();
            data.Action = ShootSpawnerData.MoveForward;

            //data.Function = BurstCompiler.CompileFunctionPointer<TransformDataDelegate>(MoveForward);

            AddComponentObject(baseEntity, data);
        }
    }
}
public class ShootSpawnerData : IComponentData
{
    public Action<TransformData, float, float> Action;

    public static void MoveForward(TransformData startData, float speed, float time)
    {
        var forward = startData.Rotation * Vector3.forward * (speed * time);

        startData.Position = startData.Position + forward;

        //return startData;
    }
}