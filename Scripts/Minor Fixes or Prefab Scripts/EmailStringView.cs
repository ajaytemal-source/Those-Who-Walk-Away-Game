using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EmailStringView : MonoBehaviour
{
    public Canvas thisCanvas;
    public TextMeshProUGUI sender; 
    public TextMeshProUGUI date; 
    public TextMeshProUGUI content; 
    public CanvasGroup attachment; 
    public GameObject download_file; 
    public GameObject download_file_name; 
    public GameObject download_file_size; 
    public GameObject attachment_name; 
    public TextMeshProUGUI time; 

    //public RectTransform content_rect; 
    //public RectTransform backdrop_rect; 
    float last_content_height = -1f; 
    bool backdrop_has_been_resized = false; 
    

    void Start()
    {
        adjustSize(); 
        download_file.GetComponent<EmailDownloadFileScript>().thisCanvas = thisCanvas; 
    }

    void Update(){
        adjustSize(); 
    }


    public void adjustSize(){
        RectTransform content_rect = content.GetComponent<RectTransform>();
        float curr_content_height = content_rect.sizeDelta.y; 

        if (!Mathf.Approximately(curr_content_height, last_content_height) || backdrop_has_been_resized){
            RectTransform backdrop_rect = gameObject.GetComponent<RectTransform>();
            backdrop_has_been_resized = false; 
            last_content_height = curr_content_height; 
            backdrop_rect.sizeDelta = new Vector2(backdrop_rect.sizeDelta.x, 50+content_rect.sizeDelta.y);
        }
    }

}