using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class WindowResizerScript : MonoBehaviour
{
    public bool window_is_open; 
    public int app_id; 
    public Vector2 minSize = new Vector2(100, 100);
    public Vector2 maxSize = new Vector2(600, 500);
    public Texture2D vertical_expand_cursor; 
    public Texture2D horizontal_expand_cursor;
    public Texture2D corner_expand_cursor1; 
    public Texture2D corner_expand_cursor2; 

    public Canvas this_canvas; 
    public RectTransform window; 
    float edge_thickness = 3f; 
    float scale_factor; 

    Vector2 LeftSide = new Vector2(-1,0);
    Vector2 RightSide = new Vector2(1,0);
    Vector2 TopSide = new Vector2(0,1);
    Vector2 BottomSide = new Vector2(0,-1);

    Vector2 LeftTop = new Vector2(-1,1);
    Vector2 LeftBottom = new Vector2(-1,-1);
    Vector2 RightTop = new Vector2(1,1);
    Vector2 RightBottom = new Vector2(1,-1);

    private Texture2D currentCursor = null;
    private Vector2 resize_direction;
    
    public bool is_resizing = false;  
    
    private Vector2 currentMousePos;
    private Vector2 previousMousePos; 
    Vector2 pivot;

    private PointerEventData pointerData;
    private List<RaycastResult> results = new List<RaycastResult>();
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    bool is_over_target = false; 


    void Start(){
        currentMousePos = Input.mousePosition;
        previousMousePos = currentMousePos;
        pivot = window.pivot;
        scale_factor = this_canvas.scaleFactor; 

        pointerData = new PointerEventData(eventSystem);
        raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    void Update(){

        pointerData.position = Input.mousePosition;
        results.Clear();
        raycaster.Raycast(pointerData, results);

        is_over_target = results.Count > 0 && (results[0].gameObject == gameObject || results[0].gameObject.transform.IsChildOf(transform));

        previousMousePos = currentMousePos; 
        currentMousePos = Input.mousePosition;
        
        if(window_is_open){
            if (!is_resizing){
                CheckCursor();
            }
            if (Input.GetMouseButtonDown(0) && resize_direction != Vector2.zero){
                is_resizing = true; 
            }
            
            if (Input.GetMouseButtonUp(0)){
                is_resizing = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }            
            if (is_resizing){
                ResizeWindow();
            }
        }

    }

    void ResizeWindow(){

        Vector2 pivot = Vector2.zero;

        if (resize_direction.x != 0)
            pivot.x = (resize_direction.x < 0) ? 1f : 0f; // Left:1, Right:0

        if (resize_direction.y != 0)
            pivot.y = (resize_direction.y < 0) ? 1f : 0f; // Bottom:1, Top:0

        SetPivotWithoutMoving(window, pivot); 

        Vector2 delta = (currentMousePos - previousMousePos) / scale_factor;
        previousMousePos = currentMousePos;

        Vector2 sizeDelta = window.sizeDelta;
        Vector2 anchoredOffset = Vector2.zero;

        if (resize_direction.x != 0){
            float resizeX = resize_direction.x * delta.x;
            sizeDelta.x += resizeX;
        }
        if (resize_direction.y != 0){
            float resizeY = resize_direction.y * delta.y;
            sizeDelta.y += resizeY;    
        }
        sizeDelta = Vector2.Max(sizeDelta, minSize);
        sizeDelta = Vector2.Min(sizeDelta, maxSize);

        window.sizeDelta = sizeDelta;
        window.anchoredPosition += anchoredOffset;
    }

    void CheckCursor(){  

        resize_direction = Vector2.zero;
        Rect window_rect = window.rect; 
    
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            window,
            Input.mousePosition,
            this_canvas.worldCamera,  
            out Vector2 localMouse
        );

        
        if (is_over_target){
            if (Mathf.Abs(localMouse.x - window_rect.xMin) < edge_thickness && (localMouse.y >= window_rect.yMin && localMouse.y <= window_rect.yMax)){
                resize_direction.x = -1;
                //Left
            }
            else if (Mathf.Abs(localMouse.x - window_rect.xMax) < edge_thickness && (localMouse.y >= window_rect.yMin && localMouse.y <= window_rect.yMax)){
                resize_direction.x = 1;
                //Right
            }
            if (Mathf.Abs(localMouse.y - window_rect.yMax) < edge_thickness && (localMouse.x >= window_rect.xMin && localMouse.x <= window_rect.xMax)){
                resize_direction.y = 1;
                //Top
            }
            else if (Mathf.Abs(localMouse.y - window_rect.yMin) < edge_thickness && (localMouse.x >= window_rect.xMin && localMouse.x <= window_rect.xMax)){
                resize_direction.y = -1;
                //Bottom
            }
        }

        if (resize_direction == LeftSide || resize_direction == RightSide){
            SetCursorIfChanged(horizontal_expand_cursor);
        }
        else if (resize_direction == TopSide || resize_direction == BottomSide){
            SetCursorIfChanged(vertical_expand_cursor);
        }
        else if (resize_direction == RightTop || resize_direction == LeftBottom){
            SetCursorIfChanged(corner_expand_cursor2);
        }
        else if (resize_direction == LeftTop || resize_direction == RightBottom){
            SetCursorIfChanged(corner_expand_cursor1);
        }
        else{
            SetCursorIfChanged(null);
        }

    }


    void SetPivotWithoutMoving(RectTransform rect, Vector2 newPivot){
        Vector2 size = rect.rect.size;
        Vector2 deltaPivot = newPivot - rect.pivot;
        Vector2 deltaPosition = new Vector2(deltaPivot.x * size.x, deltaPivot.y * size.y);

        rect.pivot = newPivot;
        rect.anchoredPosition += deltaPosition;
    }

    void SetCursorIfChanged(Texture2D newCursor){
        if (currentCursor == newCursor) return;
        currentCursor = newCursor;
        Cursor.SetCursor(newCursor, new Vector2(32, 32), CursorMode.Auto);
    }

}

