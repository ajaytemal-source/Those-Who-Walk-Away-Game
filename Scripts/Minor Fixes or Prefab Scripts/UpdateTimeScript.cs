using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UpdateTimeScript : MonoBehaviour
{

    public TextMeshProUGUI clockText; 
    public DateTime customTime;
    private float elapsedSeconds;

    public void restartTime(){
        customTime = DateTime.Today.AddHours(9);
    }

    void Update(){
        // Track how much real time passes
        elapsedSeconds += Time.deltaTime;

        // If at least one second passed, advance the clock
        while (elapsedSeconds >= 1f)
        {
            customTime = customTime.AddSeconds(1);
            elapsedSeconds -= 1f;
            clockText.text = customTime.ToString("h:mm tt");
        }
    }
}
