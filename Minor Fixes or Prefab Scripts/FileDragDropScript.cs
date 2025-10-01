using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FileDragDropScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public Canvas thisCanvas;
    bool isHolding = false; 
    public GameObject dragDropIconPrefab;
    public GameObject instantiatedDragDropIcon; 
    public int file_id;

    //Mouse position references
    private PointerEventData currentPointerData;
    private Vector2 currentMousePos;
    private Vector2 previousMousePos; 


    // Update is called once per frame
    void Update()
    {
        if (isHolding){
            currentMousePos = Input.mousePosition;
            if (CursorsAtDifferentPositions(currentMousePos, previousMousePos) && instantiatedDragDropIcon == null){
                InstantiateDDPrefabAtCursor();
                isHolding = false;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData){
        if (eventData.button != PointerEventData.InputButton.Right){
            isHolding = true; 
            previousMousePos = Input.mousePosition;
            currentPointerData = eventData;
        }
    }

    public void OnPointerUp(PointerEventData eventData){
        isHolding = false; 
        currentPointerData = eventData;
    }

    bool CursorsAtDifferentPositions(Vector2 a, Vector2 b, float threshold = 5f)
    {
        return Vector2.Distance(a, b) > threshold;
    }

    public void InstantiateDDPrefabAtCursor(){
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = 0; // Adjust Z-axis if needed
        instantiatedDragDropIcon = Instantiate(dragDropIconPrefab, thisCanvas.transform);
        currentPointerData.position = Input.mousePosition; //added to see if fixes
        currentPointerData.pointerDrag = instantiatedDragDropIcon;
        currentPointerData.pointerEnter = instantiatedDragDropIcon; //added to see if fixes 

        instantiatedDragDropIcon.GetComponent<DragDropScript>().file_id = file_id; 
        instantiatedDragDropIcon.GetComponent<DragDropScript>().thisCanvas = thisCanvas; 

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisCanvas.transform as RectTransform,
            Input.mousePosition,
            thisCanvas.worldCamera,
            out Vector2 localPoint
        );

        instantiatedDragDropIcon.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }
}
