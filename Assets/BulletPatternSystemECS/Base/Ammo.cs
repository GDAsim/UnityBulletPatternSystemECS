using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    
}

[BurstCompile]
public class AmmoData : IComponentData
{
    public IAction[] Patterns;
    public int CurrentIndex;

    public ActionTypes CurrentActionType;
    public TransformAction CurrentTransformAction;
    public TransformWithEntitiesAction CurrentTransformWithEntitiesAction;
    public DelayAction CurrentDelayAction;

    public float CurrentActionTimer;
}

[BurstCompile]
public struct AmmoInit : IComponentData { }

[BurstCompile]
public struct AmmoDataShared : ISharedComponentData
{
    public Entity FiredFrom;
}