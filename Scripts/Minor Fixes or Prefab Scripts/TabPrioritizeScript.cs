using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabPrioritizeScript : MonoBehaviour, IPointerDownHandler
{
    public RectTransform mainRectTransform; 
    public RectTransform rectTransform;
    
    public void OnPointerDown(PointerEventData eventData){
        rectTransform.gameObject.SetActive(false); 
        mainRectTransform.SetSiblingIndex(4);
    }
}
