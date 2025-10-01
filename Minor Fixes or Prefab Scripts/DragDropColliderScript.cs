using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform rectTransform;
    private Canvas canvas;
    public bool currentlyDragging = false; 

    void Start()
    {
        canvas = GetComponentInParent<Canvas>(); 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rectTransform.SetSiblingIndex(6); 
        currentlyDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        rectTransform.SetSiblingIndex(6);
        currentlyDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        rectTransform.SetSiblingIndex(6);
        currentlyDragging = false;
    }
}