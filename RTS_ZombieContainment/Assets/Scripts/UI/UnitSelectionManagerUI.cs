using UnityEngine;

public class UnitSelectionManagerUI : MonoBehaviour
{
    [SerializeField] private RectTransform selectionAreaRectTransform;

    private void Start()
    {
        UnitSelectionManager.Instance.OnSelectionAreaStart += Instance_OnSelectionAreaStart;
        UnitSelectionManager.Instance.OnSelectionAreaEnd += Instance_OnSelectionAreaEnd;

        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
            //print("debe actualziarseee");
        }
    }

    private void Instance_OnSelectionAreaStart(object sender, System.EventArgs e)
    {
       selectionAreaRectTransform.gameObject.SetActive(true);

        UpdateVisual();
    }
    private void Instance_OnSelectionAreaEnd(object sender, System.EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }


    private void UpdateVisual()
    {
        Rect selectionAreaRect = UnitSelectionManager.Instance.GetSelectionAreaRect();

        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y);
        selectionAreaRectTransform.sizeDelta = new Vector2(selectionAreaRect.width,selectionAreaRect.height);
    }
    
}
