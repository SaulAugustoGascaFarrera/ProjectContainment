using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{

    public static MouseWorldPosition Instance;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("There is more than one Instance of MouseWorldPosition Class");
            return;
        }

        Instance = this;
    }

    public Vector3 GetMousePosition()
    {
        Ray mouseCameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if(plane.Raycast(mouseCameraRay,out float distance))
        {
            return mouseCameraRay.GetPoint(distance);
        }
        else
        {
            return Vector3.zero;
        }

        
    }

}
