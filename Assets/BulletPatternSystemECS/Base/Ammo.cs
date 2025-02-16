using Unity.Entities;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    
}

public class AmmoData : IComponentData
{
    public IAction[] Patterns;
    public int CurrentIndex;

    public ActionTypes CurrentActionType;
    public TransformAction CurrentTransformAction;
    public TransformWithEntitiesAction CurrentTransformWithEntitiesAction;

    public float CurrentActionTimer;
}
public struct AmmoInit : IComponentData { }