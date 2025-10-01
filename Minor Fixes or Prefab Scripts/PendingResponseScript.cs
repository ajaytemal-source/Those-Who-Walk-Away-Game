using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendingResponseScript : MonoBehaviour
{
    public CanvasGroup pending_canvas_group; 
    float time_interval = 1; 
    private float timeSinceLastChange = 0f;
    public bool blinking = false;

    private void Update()
    {
        if(blinking){
            timeSinceLastChange += Time.deltaTime;
            if (timeSinceLastChange >= time_interval)
            {
                // Change the alpha value
                if (pending_canvas_group.alpha == 1){
                    pending_canvas_group.alpha = 0; 
                }
                else{
                    pending_canvas_group.alpha = 1;
                }
                timeSinceLastChange = 0f;
            }
        }
        else{
            pending_canvas_group.alpha = 0; 
        }
    }

}
