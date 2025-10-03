using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmailNotificationScript : MonoBehaviour
{
    public RectTransform notifContainer;
    public GameObject header; 
    public GameObject subject_peak; 
    public GameObject content_peak; 
    public Button exit_button; 
    float locationLimit; 
    bool reachedLocationLimit = true; 
    bool goOtherWay = false; 
    float elapsedTime = 0f;
    bool isTiming = false;
    private RectTransform rectTransform;
    public int convo_id; 
    bool notifPressed = false; 
    float lastNotifCount; 


    void Start(){
        gameObject.GetComponent<Button>().onClick.AddListener(openEmailToNotif);
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

    void openEmailToNotif(){
        GameEvents.EmailNotifPressed.Raise(convo_id); 
        notifPressed = true; 
        gameObject.GetComponent<Button>().interactable = false;
    }

    void Update()
    {
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


}
