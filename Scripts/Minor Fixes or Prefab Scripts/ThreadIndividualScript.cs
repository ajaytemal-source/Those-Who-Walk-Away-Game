using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class ThreadIndividualScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image message_backDrop; 
    public TextMeshProUGUI sender; 
    public TextMeshProUGUI time; 
    public TextMeshProUGUI content; 
    public Image pfp; 
    public TextMeshProUGUI sender_initial;
    public RectTransform content_rect; 
    public RectTransform backdrop_rect;

    float last_content_height = -1f; 
    bool backdrop_has_been_resized = false; 
    public float added_height; 

    int frameCounter = 0;
    public int updateEveryNFrames = 5;

    // Start is called before the first frame update
    void Start(){
        adjustSize();
        SetAlpha(0f);
    }

    // Update is called once per frame
    void LateUpdate(){
        if (++frameCounter >= updateEveryNFrames){
            adjustSize();
            frameCounter = 0;
        }
    }

    void adjustSize(){
        float curr_content_height = content_rect.sizeDelta.y; 
        if (!Mathf.Approximately(curr_content_height, last_content_height) || backdrop_has_been_resized){
            backdrop_has_been_resized = false; 
            last_content_height = curr_content_height; 
            backdrop_rect.sizeDelta = new Vector2(backdrop_rect.sizeDelta.x, added_height+content_rect.sizeDelta.y);
        }
    }


    public void OnPointerEnter(PointerEventData eventData){
        SetAlpha(0.02f);
    }

    public void OnPointerExit(PointerEventData eventData){
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha){
        Color c = message_backDrop.color;
        c.a = alpha;
        message_backDrop.color = c;
    }
}
