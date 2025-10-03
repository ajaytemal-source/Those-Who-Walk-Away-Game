using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SendingBarResizerScript : MonoBehaviour
{

    public float maxWidth = 360f;
    public float buffer; 
    public RectTransform window_rect; 
    public RectTransform sending_panel_rect;

    void Update(){
        float targetWidth = Mathf.Min(window_rect.rect.width-buffer, maxWidth);
        sending_panel_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }


}

