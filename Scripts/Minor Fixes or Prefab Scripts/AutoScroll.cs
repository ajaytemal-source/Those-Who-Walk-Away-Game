using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public void scrollHorizontally(ScrollRect horizontal_content) {
        Canvas.ForceUpdateCanvases();
        horizontal_content.horizontalNormalizedPosition = 1f;
    }

    public void scrollVertically(ScrollRect vertical_content) {
        Canvas.ForceUpdateCanvases();
        vertical_content.verticalNormalizedPosition = 0f;
    }

    public void scrollVerticallyEmail(ScrollRect vertical_content, float curr_position){
        Canvas.ForceUpdateCanvases();
        vertical_content.verticalNormalizedPosition = curr_position; 
    }

}