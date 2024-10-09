using Unity.Entities;
using UnityEngine;

public class CubeMoverAuthoring : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    public class CubeBaker : Baker<CubeMoverAuthoring>
    {
        public override void Bake(CubeMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CubeMover
            {
                rotationSpeed = authoring.rotationSpeed,
            });
        }
    }
}

public struct CubeMover : IComponentData
{
    public float rotationSpeed;
}
