using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; 

public class ScrollRectVisibility : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;
    private float checkDelay = 0f;       
    private float verticalBuffer = 50f;   
    private int lastChildCount;
    private Coroutine delayedCheckRoutine;
    public DragUI dragDropColliderScript; 
    public WindowResizerScript windowResizerScript;
    
    public Scrollbar scrollbar;
    public VerticalLayoutGroup verticalLayout; 
    public ContentSizeFitter contentSizeFitter;
    private float lockedSize;
    private float lockedValue;
    private bool allowScroll = true;
    private bool wasAllowScroll = true;
    private Vector2 lockedContentPos;
    private bool stopCount = false; 

    private HashSet<Transform> disabledForVisibility = new HashSet<Transform>();

    void Awake(){
        scrollRect = GetComponent<ScrollRect>();
        var verticalLayout = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;
        lastChildCount = content.childCount;
    }

    void Start(){
        scrollRect.onValueChanged.AddListener((pos) => UpdateVisibility());
        UpdateVisibility(); 
    }

    void LateUpdate(){
        if (content.childCount > lastChildCount){
            lastChildCount = content.childCount;
            StartDelayedCheck();
        }

        if (dragDropColliderScript.currentlyDragging){
            UpdateVisibility();
            allowScroll = false;
        }
        else{
            allowScroll = true;
        }
        if (!wasAllowScroll && allowScroll){
            contentSizeFitter.enabled = true;
            verticalLayout.enabled = true;
            scrollRect.content.anchoredPosition = lockedContentPos;
            scrollbar.size = lockedSize;
            scrollbar.value = lockedValue;
            foreach (Transform child in disabledForVisibility){
                child.gameObject.SetActive(true);
            }
            stopCount = false; 
            UpdateVisibility(); 
        }

        if (allowScroll){
            verticalLayout.enabled = true;
            lockedSize = scrollbar.size;
            lockedValue = scrollbar.value;
            lockedContentPos = scrollRect.content.anchoredPosition;
        }
        else{
            stopCount = true; 
            scrollbar.size = lockedSize;
            scrollbar.value = lockedValue;
            contentSizeFitter.enabled = false; 
            verticalLayout.enabled = false;
            scrollRect.content.anchoredPosition = lockedContentPos;
            foreach(Transform child in disabledForVisibility){
                child.gameObject.SetActive(false);
            }  
        }
        wasAllowScroll = allowScroll;
    }


    private void StartDelayedCheck(){
        if (delayedCheckRoutine != null)
            StopCoroutine(delayedCheckRoutine);

        delayedCheckRoutine = StartCoroutine(DelayedCheck());
    }

    private IEnumerator DelayedCheck(){
        yield return new WaitForSeconds(checkDelay);
            UpdateVisibility();
    }

    void UpdateVisibility(){
        foreach (Transform child in content){
            RectTransform rect = child as RectTransform;
            if (rect == null) continue;

            bool visible = IsVisibleWithBuffer(rect);
            var tmp = child.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            MonoBehaviour childScript = null; 

            if(child.gameObject.GetComponent<EmailStringView>() != null){
                childScript = child.gameObject.GetComponent<EmailStringView>();  // If In Email 
            }
            else if (child.gameObject.GetComponent<ThreadIndividualScript>() != null){
                childScript = child.gameObject.GetComponent<ThreadIndividualScript>();  // If In Teams
            }

            if (childScript == null) continue;

            if (visible){
                if(!stopCount){
                    tmp.enabled = true;
                    childScript.enabled = true;
                    disabledForVisibility.Remove(child);
                }
            }
            else{ 
                if(!stopCount){
                    tmp.enabled = false;
                    childScript.enabled = false;
                    disabledForVisibility.Add(child);
                }
            }
        }
    }

    bool IsVisibleWithBuffer(RectTransform child){
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        Rect viewportRect = new Rect(viewportCorners[0], viewportCorners[2] - viewportCorners[0]);
        viewportRect.yMin -= verticalBuffer;
        viewportRect.yMax += verticalBuffer;
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);
        Rect childRect = new Rect(childCorners[0], childCorners[2] - childCorners[0]);
        return viewportRect.Overlaps(childRect);
    }
}
