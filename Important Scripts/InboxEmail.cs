using System; 
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InboxEmail : MonoBehaviour
{
    public AudioSource src; 

    public Button button; 

    Color32 if_not_pressed = new Color32(255,255,255,255);

    Color32 if_pressed = new Color32(225,225,225,255);
    public GameObject sender; 
    public TextMeshProUGUI time; 
    public TextMeshProUGUI emailPeak; 
    public TextMeshProUGUI subject; 
    public GameObject reply_count; 
    public RectTransform content_window_rect; 
    float last_window_width = -1f; 
    bool window_has_been_resized = false; 
    public GameObject emailPrefab; 

    public GameObject emailScrollView;  

    RectTransform email_rect;
    RectTransform time_rect;
    RectTransform subject_rect;
    RectTransform self_rect;

    public int replyCountVal = 1; 
    
    // Start is called before the first frame update
    void Start()
    {
        content_window_rect = transform.parent.gameObject.GetComponent<RectTransform>();
        StartCoroutine(lateAdjustSize());  
        button = GetComponent<Button>();
        button.onClick.AddListener(isPressed);
        email_rect = emailPeak.GetComponent<RectTransform>();
        time_rect = time.GetComponent<RectTransform>();
        subject_rect = subject.GetComponent<RectTransform>();
        self_rect = GetComponent<RectTransform>();

    }

    void Update(){
        if(window_has_been_resized){
            adjustSize();
        } 
    }

    IEnumerator lateAdjustSize(){
        yield return null; 
        adjustSize(); 
    }

    void adjustSize(){

        float curr_window_width = content_window_rect.rect.width; 

        if (!Mathf.Approximately(curr_window_width, last_window_width) || !window_has_been_resized || email_rect.rect.xMax >= time_rect.rect.xMin) {
            last_window_width = curr_window_width; 
            email_rect.sizeDelta = new Vector2((self_rect.rect.width - 150) - subject_rect.sizeDelta.x, email_rect.sizeDelta.y);
            window_has_been_resized = true; 
        }    
    }

    public void incrementReplyCount(){
        ++replyCountVal;
        if (replyCountVal > 1){
            reply_count.GetComponent<CanvasGroup>().alpha = 1;
        }
        reply_count.GetComponent<TextMeshProUGUI>().text = Convert.ToString(replyCountVal);
    }

    public void MoveUpIndex(){
        gameObject.transform.SetSiblingIndex(emailScrollView.transform.childCount - 1);  
    }

    public void isPressed(){
        ColorBlock new_color = button.colors;
        new_color.normalColor = if_pressed;
        button.colors = new_color;
    }

    public void unread_Status(){
        ColorBlock new_color = button.colors;
        new_color.normalColor = if_not_pressed;
        button.colors = new_color;
    }
}


