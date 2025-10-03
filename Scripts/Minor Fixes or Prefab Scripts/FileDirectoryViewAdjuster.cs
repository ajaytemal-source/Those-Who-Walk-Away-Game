using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileDirectoryViewAdjuster : MonoBehaviour
{

    public RectTransform childTMP; 
    float padding = 20f; 


    // Update is called once per frame
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        GetComponent<RectTransform>().sizeDelta = new Vector2(childTMP.rect.width + padding, GetComponent<RectTransform>().sizeDelta.y); 
    }
}
