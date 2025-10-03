using UnityEngine;
using UnityEngine.UI;

public class TBPulsatingScript : MonoBehaviour
{
    public Color originalColor = Color.white;
    public Color startColor = Color.white;          // Base white color
    public Color endColor = new Color(1.3f, 1.3f, 1.3f, 1f); // Slightly brighter (if supported)
    public float pulseSpeed = 2f;                   // Speed of the pulsation

    public Image image;
    public bool isActive = false;                  // Whether the pulsation is active

    void Update(){
        if (isActive && image != null){
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // Oscillates between 0 and 1
            image.color = Color.Lerp(startColor, endColor, t);
        }
    }

    public void ActivatePulsation(){
        isActive = true;
    }

    public void DeactivatePulsation(){
        isActive = false;
        if (image != null)
        {
            image.color = originalColor;
        }
    }
}