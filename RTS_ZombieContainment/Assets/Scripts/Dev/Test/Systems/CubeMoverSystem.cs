using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct CubeMoverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        

        foreach ((RefRW<LocalTransform> cubeLocalTransform,RefRO<CubeMover> cubeMover) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<CubeMover>>())
        {
            quaternion rotation = quaternion.Euler(0.0f,math.radians(90.0f) * cubeMover.ValueRO.rotationSpeed * SystemAPI.Time.DeltaTime,0.0f);

            cubeLocalTransform.ValueRW.Rotation = math.mul(rotation,cubeLocalTransform.ValueRO.Rotation);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
