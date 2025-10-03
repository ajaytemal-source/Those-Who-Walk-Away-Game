using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ChoiceManagerScript : MonoBehaviour
{

    public ExitGameScript exitGameScript; 
    
    public Button choice1; 
    public Button choice2; 
    public Button choice3;

    public Button twoChoices1;
    public Button twoChoices2;

    public GameObject choiceBG;
    public CanvasGroup choiceBGCanvasGroup; 
    public CanvasGroup raycasterBlocker; 

    public CanvasGroup choice1CG;
    public CanvasGroup choice2CG;
    public CanvasGroup choice3CG;

    public CanvasGroup twoChoices1CG;
    public CanvasGroup twoChoices2CG;

    public ChoiceSelection choice1Script; 
    public ChoiceSelection choice2Script; 
    public ChoiceSelection choice3Script; 

    public ChoiceSelection twoChoices1Script;
    public ChoiceSelection twoChoices2Script;

    private PointerEventData pointerData;
    private List<RaycastResult> results = new List<RaycastResult>();
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;


    //True when player is in choosing action 
    public bool isChoosing;
    public bool choiceBGClicked; 

    //If player interacts with screen without choosing, player 'taps out' of action. 
    public bool tappedOut; 

    void Start(){
        choice1Script = choice1.GetComponent<ChoiceSelection>();
        choice2Script = choice2.GetComponent<ChoiceSelection>();
        choice3Script = choice3.GetComponent<ChoiceSelection>();
        twoChoices1Script = twoChoices1.GetComponent<ChoiceSelection>();
        twoChoices2Script = twoChoices2.GetComponent<ChoiceSelection>();

        pointerData = new PointerEventData(eventSystem);
        //raycaster = GetComponentInParent<GraphicRaycaster>();
    }

    void Update(){
        pointerData.position = Input.mousePosition;
        results.Clear();
        raycaster.Raycast(pointerData, results);

        choiceBGClicked = results.Count > 0 && (results[0].gameObject == choiceBG);

        if (isChoosing){
            if (Input.GetMouseButtonDown(0) && choiceBGClicked){
                isChoosing = false; 
                tappedOut = true; 
                StartCoroutine(setFalse()); 
        }
        }
    }

    IEnumerator setFalse(){
        yield return null; //needs to wait exactly one frame to work 
        tappedOut = false; 
    }



    public void changeAllChoices(List<string> choicesText){
        if(choicesText.Count == 3){
            changeChoiceText(choicesText[0], ref choice1);
            changeChoiceText(choicesText[1], ref choice2);
            changeChoiceText(choicesText[2], ref choice3);
        }
        else if (choicesText.Count == 2){
            changeChoiceText(choicesText[0], ref twoChoices1);
            changeChoiceText(choicesText[1], ref twoChoices2);
        }
        else{
            changeChoiceText(choicesText[0], ref choice2);
        }
    }

    void changeChoiceText(string choiceText, ref Button choice){
        choice.GetComponentInChildren<TextMeshProUGUI>().text = choiceText; 
    }

    IEnumerator fadeChoiceOut(CanvasGroup choiceCanvasGroup){
        while (choiceCanvasGroup.alpha > 0){
            yield return new WaitForSeconds(0.001f);
            choiceCanvasGroup.alpha -= Time.deltaTime*1.5f; 
        }
    }

    IEnumerator fadeChoiceIn(CanvasGroup choiceCanvasGroup){
        while (choiceCanvasGroup.alpha < 1){
            yield return new WaitForSeconds(0.001f);
            choiceCanvasGroup.alpha += Time.deltaTime*1.5f; 
        }
    }

    IEnumerator fadeChoicesOut(int choicesCount){
        if (choicesCount == 3){
            yield return new WaitUntil(() => choice1CG.alpha == 1);
            StartCoroutine(fadeChoiceBGOut());
            StartCoroutine(fadeChoiceOut(choice1CG)); 
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(fadeChoiceOut(choice2CG));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(fadeChoiceOut(choice3CG)); 
        }
        else if (choicesCount == 2){
            yield return new WaitUntil(() => twoChoices1CG.alpha == 1);
            StartCoroutine(fadeChoiceBGOut());
            StartCoroutine(fadeChoiceOut(twoChoices1CG)); 
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(fadeChoiceOut(twoChoices2CG));
        }
        else{
            yield return new WaitUntil(() => choice2CG.alpha == 1);
            StartCoroutine(fadeChoiceBGOut());
            yield return StartCoroutine(fadeChoiceOut(choice2CG)); 
        }
        exitGameScript.pause_enabled = true; 
    }

    IEnumerator fadeChoicesIn(int choicesCount){
        exitGameScript.pause_enabled = false; 
        if (choicesCount == 3){
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => choice1CG.alpha == 0);
            StartCoroutine(fadeChoiceBGIn());
            StartCoroutine(fadeChoiceIn(choice1CG)); 
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(fadeChoiceIn(choice2CG));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(fadeChoiceIn(choice3CG));
        }
        else if (choicesCount == 2){
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => twoChoices1CG.alpha == 0);
            StartCoroutine(fadeChoiceBGIn());
            StartCoroutine(fadeChoiceIn(twoChoices1CG)); 
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(fadeChoiceIn(twoChoices2CG));
        }
        else{
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => choice2CG.alpha == 0);
            StartCoroutine(fadeChoiceBGIn());
            yield return StartCoroutine(fadeChoiceIn(choice2CG)); 
        }
    }



    public IEnumerator turnOffChoices(int choicesCount){
        isChoosing = false;
        if (choicesCount == 3){
            choice1.interactable = false;
            choice2.interactable = false;
            choice3.interactable = false;
            choice1CG.blocksRaycasts = false; 
            choice2CG.blocksRaycasts = false; 
            choice3CG.blocksRaycasts = false; 
        }
        else if (choicesCount == 2){
            twoChoices1.interactable = false;
            twoChoices2.interactable = false;
            twoChoices2CG.blocksRaycasts = false; 
            twoChoices2CG.blocksRaycasts = false; 
        }
        else{
            choice2.interactable = false;
            choice2CG.blocksRaycasts = false;
        }
        yield return StartCoroutine(fadeChoicesOut(choicesCount));
    }


    public IEnumerator turnOnChoices(int choicesCount){
        yield return StartCoroutine(fadeChoicesIn(choicesCount));
        if (choicesCount == 3){
            choice1.interactable = true;
            choice2.interactable = true;
            choice3.interactable = true;
            choice1CG.blocksRaycasts = true; 
            choice2CG.blocksRaycasts = true; 
            choice3CG.blocksRaycasts = true; 
        }
        else if (choicesCount == 2){
            twoChoices1.interactable = true;
            twoChoices2.interactable = true;
            twoChoices1CG.blocksRaycasts = true; 
            twoChoices2CG.blocksRaycasts = true; 
        }
        else{
            choice2.interactable = true;
            choice2CG.blocksRaycasts = true;
        }
        isChoosing = true;
    }


    IEnumerator fadeChoiceBGIn(){
        raycasterBlocker.blocksRaycasts = true; 
        choiceBGCanvasGroup.blocksRaycasts = true; 
        while (choiceBGCanvasGroup.alpha < 1){
            yield return new WaitForSeconds(0.001f);
            choiceBGCanvasGroup.alpha += Time.deltaTime; 
        }
    }

    IEnumerator fadeChoiceBGOut(){
        raycasterBlocker.blocksRaycasts = false; 
        choiceBGCanvasGroup.blocksRaycasts = false;
        while (choiceBGCanvasGroup.alpha > 0){
            yield return new WaitForSeconds(0.001f);
            choiceBGCanvasGroup.alpha -= Time.deltaTime; 
        } 
    }
}
