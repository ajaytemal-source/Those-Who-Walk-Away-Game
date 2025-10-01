using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO; 
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragDropScript : MonoBehaviour
{
    public Canvas thisCanvas;
    bool isHolding = true; 
    public CanvasGroup this_canvas_group; 
    public int file_id;  


    // Update is called once per frame
    void Update()
    {   
        if(isHolding){
            FollowCursor();
            if (!Input.GetMouseButton(0))
            {
                isHolding = false; // Stop holding
                Destroy(gameObject); // Destroy this object
            }
        }
    }

    private void FollowCursor(){
        if (thisCanvas == null) return;

        // Update the position relative to the Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisCanvas.transform as RectTransform,
            Input.mousePosition,
            thisCanvas.worldCamera,
            out Vector2 localPoint
        );

        gameObject.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }
    
}
