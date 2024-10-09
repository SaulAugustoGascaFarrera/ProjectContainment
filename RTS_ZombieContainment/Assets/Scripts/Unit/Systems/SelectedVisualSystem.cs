using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct SelectedVisualSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>())
        {
           RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
           localTransform.ValueRW.Scale = 0.0f;

        }

        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>())
        {
            RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
            localTransform.ValueRW.Scale = selected.ValueRO.showScale;

        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
