using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{

    private Vector2 selectionStartMousePosition;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartMousePosition = Input.mousePosition;

            Debug.Log("DOWN: " + selectionStartMousePosition);
        }


        if (Input.GetMouseButtonUp(0))
        {
            Vector2 selectionEndMousePosition = Input.mousePosition;

            Debug.Log("UP: " + selectionEndMousePosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = MouseWorldPosition.Instance.GetMousePosition();

            //Debug.Log(mousePosition);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<UnitMover,Selected>().Build(entityManager);


            NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            NativeArray<UnitMover> unitMoverArray = entityQuery.ToComponentDataArray<UnitMover>(Allocator.Temp);

            //Debug.Log(unitMoverArray.Length);

            for(int i=0; i<unitMoverArray.Length;i++)
            {
                UnitMover unitMover = unitMoverArray[i];

                unitMover.targetPosition = mousePosition;


                unitMoverArray[i] = unitMover;
                //entityManager.SetComponentData(entityArray[i], unitMover);
            }
            entityQuery.CopyFromComponentDataArray(unitMoverArray);

           
        }
        


    }
}
