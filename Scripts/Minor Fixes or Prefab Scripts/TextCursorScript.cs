using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextCursorScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Texture2D textCursor;
    public Vector2 hotSpot = Vector2.zero;
    public bool showCursor = false; 

    public void OnPointerEnter(PointerEventData eventData){
        if(showCursor){
            Cursor.SetCursor(textCursor, new Vector2(32, 32), CursorMode.Auto);
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
