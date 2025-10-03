using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingLoginScript : MonoBehaviour
{
    public CanvasGroup[] circleGroups; // Assign 6 CanvasGroups in the Inspector
    public float fadeDuration = 0.5f;  // Duration for fade-in/out
    public float delayBetween = 0.5f;  // Delay between each circle animation
    public bool loopAnimation = true;  // Toggle looping 
    public CanvasGroup thisCanvasGroup; 

    private void Start()
    {
        thisCanvasGroup.alpha = 0; 
    }

    public void startAnimation(){
        StartCoroutine(AnimateCircles());
    }

    public IEnumerator AnimateCircles()
    {
        thisCanvasGroup.alpha = 0; 
        while (loopAnimation)
        {
            for (int i = 0; i < circleGroups.Length; i++)
            {
                StartCoroutine(FadeInAndOut(circleGroups[i]));
                yield return new WaitForSeconds(delayBetween);
            }
        }
    }

    private IEnumerator FadeInAndOut(CanvasGroup canvasGroup)
    {
        yield return StartCoroutine(Fade(canvasGroup, 0.05f, 1f, fadeDuration)); // Fade In
        yield return StartCoroutine(Fade(canvasGroup, 1f, 0.05f, fadeDuration)); // Fade Out
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = to; // Ensure final value is set
    }
}