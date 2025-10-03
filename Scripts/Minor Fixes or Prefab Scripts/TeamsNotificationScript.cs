using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamsNotificationScript : MonoBehaviour
{

    public RectTransform notifContainer;
    public TextMeshProUGUI contact; 
    public TextMeshProUGUI content_peak; 
    public Thread thisThread; 
    public Button exit_button; 
    
    bool notifPressed = false; 
    private RectTransform rectTransform;
    float locationLimit; 
    bool reachedLocationLimit = true; 
    bool goOtherWay = false; 
    float elapsedTime = 0f;
    bool isTiming = false;
    float lastNotifCount; 

    void Start(){
        gameObject.GetComponent<Button>().onClick.AddListener(openThreadToNotif);
        rectTransform = GetComponent<RectTransform>();
        lastNotifCount = notifContainer.childCount - 1f; 
        rectTransform.anchoredPosition = new Vector3(rectTransform.anchoredPosition.x,(-45*lastNotifCount) + rectTransform.anchoredPosition.y);
        StartCoroutine(notificationAnimation()); 
    }

    IEnumerator notificationAnimation(){
        reachedLocationLimit = false; 
        yield return new WaitUntil(() => rectTransform.anchoredPosition.x <= 0);
        reachedLocationLimit = true; 
        isTiming = true; 
        yield return new WaitUntil(() => exit_button.GetComponent<ChoiceSelection>().pressed || elapsedTime > 5f || notifPressed); 
        goOtherWay = true;
        yield return new WaitForSeconds(0.2f); 
        Destroy(gameObject);
    }

    void Update(){
        float currentNotifCount = notifContainer.childCount - 1f;
        if (isTiming){
            elapsedTime += Time.deltaTime; 
        }
        if (!reachedLocationLimit){ 
            transform.localPosition += new Vector3(-10f, 0, 0);
        }
        else if (goOtherWay){
            transform.localPosition += new Vector3(10f, 0, 0);
        }
    }

    void openThreadToNotif(){
        GameEvents.TeamsNotifPressed.Raise(thisThread);
        notifPressed = true; 
        gameObject.GetComponent<Button>().interactable = false;
    }
    
}
