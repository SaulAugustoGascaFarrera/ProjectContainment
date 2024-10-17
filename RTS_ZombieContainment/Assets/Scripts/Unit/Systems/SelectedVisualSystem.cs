using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(ResetEventsSystem))]
partial struct SelectedVisualSystem : ISystem
{
   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithPresent<Selected>())
        {
            if (selected.ValueRO.OnDeselected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                visualLocalTransform.ValueRW.Scale = 0;
                //Debug.Log("onDeselected");
            }
            if (selected.ValueRO.OnSelected)
            {
                RefRW<LocalTransform> visualLocalTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
                visualLocalTransform.ValueRW.Scale = selected.ValueRO.showScale;
                //Debug.Log("onSelected");
            }
        }

        //foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>().WithDisabled<Selected>())
        //{
        //    RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
        //    localTransform.ValueRW.Scale = 0.0f;

        //}

        //foreach (RefRO<Selected> selected in SystemAPI.Query<RefRO<Selected>>())
        //{
        //    RefRW<LocalTransform> localTransform = SystemAPI.GetComponentRW<LocalTransform>(selected.ValueRO.visualEntity);
        //    localTransform.ValueRW.Scale = selected.ValueRO.showScale;

        //}
    }

}
