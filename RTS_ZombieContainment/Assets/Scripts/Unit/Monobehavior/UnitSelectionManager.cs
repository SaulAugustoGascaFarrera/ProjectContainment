using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance;

    public event EventHandler OnSelectionAreaStart;

    public event EventHandler OnSelectionAreaEnd;

    private Vector2 selectionStartMousePosition;

    
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is more than one UnitSelectionManager Intance class");
            return;
        }

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;

            //Debug.Log("DOWN: " + selectionStartMousePosition);

            OnSelectionAreaStart?.Invoke(this,EventArgs.Empty);
        }


        if (Input.GetMouseButtonUp(0))
        {
     
            OnSelectionAreaEnd?.Invoke(this,EventArgs.Empty);

               

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected>().Build(entityManager);


            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Selected> selectedArray = entityQuery.ToComponentDataArray<Selected>(Allocator.Temp);

            for (int i = 0;i< entityArray.Length;i++)
            {
                entityManager.SetComponentEnabled<Selected>(entityArray[i], false);

                Selected selected = selectedArray[i];

                selected.OnDeselected = true;

                entityManager.SetComponentData(entityArray[i], selected);
            }

            

            Rect selectionAreaRect = GetSelectionAreaRect();
            float selectionAreaSize = selectionAreaRect.width + selectionAreaRect.height;
            float multipleSelectionSizeMin = 40.0f;
            bool isMultipleSelection = selectionAreaSize > multipleSelectionSizeMin;

            //Debug.Log(isMultipleSelection + " " + selectionAreaSize);


            if(isMultipleSelection)
            {
                entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Unit>().WithPresent<Selected>().Build(entityManager);

                entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                NativeArray<LocalTransform> localTransformArray = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

                for (int i = 0; i < localTransformArray.Length; i++)
                {
                    LocalTransform unitLocalTransform = localTransformArray[i];

                    Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unitLocalTransform.Position);

                    if (selectionAreaRect.Contains(unitScreenPosition))
                    {
                        //unit is inside selction area
                        entityManager.SetComponentEnabled<Selected>(entityArray[i], true);

                        Selected selected = entityManager.GetComponentData<Selected>(entityArray[i]);
                        selected.OnSelected = true;
                        entityManager.SetComponentData(entityArray[i], selected);
                    }

                    //entityManager.SetComponentEnabled<Selected>(entityArray[i], true);
                }
            }
            else
            {
                //single select
                entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

                PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();

                CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;


                UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                int unitsLayer = 6;

                RaycastInput raycastInput = new RaycastInput
                {
                    Start = ray.GetPoint(0),
                    End = ray.GetPoint(9999f),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << unitsLayer,
                        GroupIndex = 0
                    }
                    
                };

                if(collisionWorld.CastRay(raycastInput,out Unity.Physics.RaycastHit raycastHit))
                {
                    if(entityManager.HasComponent<Unit>(raycastHit.Entity))
                    {
                        //Hit an Unit
                        entityManager.SetComponentEnabled<Selected>(raycastHit.Entity, true);

                        Selected selected = entityManager.GetComponentData<Selected>(raycastHit.Entity);
                        selected.OnSelected = true;
                        entityManager.SetComponentData(raycastHit.Entity,selected);
                    }
                }

            }

            

        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetMousePosition();

            //Debug.Log(mousePosition);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover,Selected>().Build(entityManager);


            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            //Debug.Log(entityArray.Length);

            NativeArray<float3> mousePositionArray = GenerateMovePositionArray(mousePosition, entityArray.Length);

            //NativeArray<float3> mousePositionArray = GenerateTrianglePositionArray(mousePosition,entityArray.Length);

            for (int i=0; i<unitMoverArray.Length;i++)
            {
                UnitMover unitMover = unitMoverArray[i];

                unitMover.targetPosition = mousePositionArray[i];


                unitMoverArray[i] = unitMover;
                //entityManager.SetComponentData(entityArray[i], unitMover);
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);

           
        }
        


    }


    public Rect GetSelectionAreaRect()
    {
        Vector2 selectionEndMousePosition = Input.mousePosition;

        Vector2 lowerLeftCorner = new Vector2(
            Mathf.Min(selectionStartMousePosition.x,selectionEndMousePosition.x),
            Mathf.Min(selectionStartMousePosition.y, selectionEndMousePosition.y)
        );

        Vector2 upperRightCorner = new Vector2(
            Mathf.Max(selectionStartMousePosition.x, selectionEndMousePosition.x),
            Mathf.Max(selectionStartMousePosition.y, selectionEndMousePosition.y)
        );

        return new Rect(lowerLeftCorner.x,lowerLeftCorner.y,upperRightCorner.x - lowerLeftCorner.x,upperRightCorner.y - lowerLeftCorner.y);
    }


    private NativeArray<float3> GenerateMovePositionArray(float3 targetPosition,int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount,Allocator.Temp);

        if(positionCount == 0)
        {
            return positionArray;
        }

        //Debug.Log("PA " + positionArray.Length);

        positionArray[0] = targetPosition;

        if (positionCount == 1)
        {
            return positionArray;
        }

        float ringSize = 5f;
        int ring = 0;
        int positionIndex = 1;

        while(positionIndex < positionCount)
        {
            int ringPositionCount = 3 + ring * 2;


            for(int i=0;i<ringPositionCount;i++)
            {
                float angle = i * (math.PI2 / ringPositionCount);
                float3 ringVector = math.rotate(quaternion.RotateY(angle),new float3(ringSize * (ring + 1),0,0));
                float3 ringPosition = targetPosition + ringVector;

                positionArray[positionIndex] = ringPosition;
                positionIndex++; 
                
                if(positionIndex >= positionCount)
                {
                    break;
                }
            }
            ring++;
        }

        return positionArray;

    }

    private NativeArray<float3> GenerateMovePositionArray2(float3 targetPosition, int positionCount)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(positionCount, Allocator.Temp);
        for (int i = 0; i < positionCount; i++)
        {
            float angle = i * (2 * math.PI / positionCount);
            float3 position = new float3(math.cos(angle), 0f, math.sin(angle)) * 5f + targetPosition;
            positionArray[i] = position;
        }

        return positionArray;
    }

    private NativeArray<float3> GenerateTrianglePositionArray(float3 targetPosition, float size)
    {
        NativeArray<float3> positionArray = new NativeArray<float3>(3, Allocator.Temp);

        // Calculamos los vértices de un triángulo equilátero
        // Vértice 1 (en la parte superior)
        positionArray[0] = targetPosition + new float3(0f, 0f, size);

        // Vértice 2 (en la parte inferior izquierda)
        positionArray[1] = targetPosition + new float3(-size * math.sin(math.radians(60)), 0f, -size * math.cos(math.radians(60)));

        // Vértice 3 (en la parte inferior derecha)
        positionArray[2] = targetPosition + new float3(size * math.sin(math.radians(60)), 0f, -size * math.cos(math.radians(60)));

        return positionArray;
    }
}
