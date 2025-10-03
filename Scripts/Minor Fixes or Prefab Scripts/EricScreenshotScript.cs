using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class EricScreenshotScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image message_backDrop; 
    public TextMeshProUGUI time; 


    // Start is called before the first frame update
    void Start()
    {
        //time.GetComponent<TextMeshProUGUI>().text = DateTime.Now.Subtract(TimeSpan.FromMinutes(4)).ToString("h:mm tt");
        SetAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(0.02f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color c = message_backDrop.color;
        c.a = alpha;
        message_backDrop.color = c;
    }
}
