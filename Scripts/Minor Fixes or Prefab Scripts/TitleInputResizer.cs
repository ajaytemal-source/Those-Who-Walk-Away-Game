using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitleInputResizer : MonoBehaviour
{
    public TextMeshProUGUI text;
    public RectTransform input_bubble; 
    public RectTransform text_rect; 
    //public RectTransform content_window_rect; 
    //float last_window_width = -1f; 
    int last_text_length; 
    int curr_text_length; 
    
    void Start(){
        last_text_length = text.text.Length; 
        //content_window_rect = transform.parent.parent.gameObject.GetComponent<RectTransform>();
    }

    IEnumerator DelayedHeightUpdate(){
        yield return null; 
        input_bubble.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text_rect.rect.height + 55f);
    }

    void LateUpdate(){
        
        curr_text_length = text.text.Length; 
        //float curr_window_width = content_window_rect.rect.width; 

        if (last_text_length != curr_text_length) {
            last_text_length = curr_text_length; 
            //last_window_width = curr_window_width;
            float preferred_width = text.GetPreferredValues().x;
            //float finalWidth = Mathf.Min(preferred_width, curr_window_width/2);
            text_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferred_width);
            input_bubble.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferred_width + 80f);
            StartCoroutine(DelayedHeightUpdate());
        }    
    }

}