using Unity.Entities;
using UnityEngine;

public class Move : MonoBehaviour
{
    class Baker : Baker<Move>
    {
        public override void Bake(Move authoring)
        {
            var baseEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(baseEntity, new MoveData());
        }
    }
}

public struct MoveData : IComponentData
{

}
