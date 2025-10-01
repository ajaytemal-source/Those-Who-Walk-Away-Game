using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.IO; 
using System.Linq;
using System.ComponentModel;
using UnityEngine.Rendering;
using System.Collections.Specialized;
using System.Threading;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.FullSerializer;
using System;

public class EmailScrollView : MonoBehaviour{

    //Reference to In-Scene Scripts: 
    public FileNavigatorManager fileNavigatorManager;
    public EmailDialougeManager emailDialougeManager; 
    public ChoiceManagerScript choiceManagerScript; 
    public AutoScroll autoScroll;
    public RectTransform notifContainer;

    //References to EDM Game Objects: 
    public Button exitButton;
    public Button spam_button; 
    public Canvas thisCanvas; 
    public GameObject Email_CentralObject;
    public Button deactivate; 
    
    public Button choice1; 
    public Button choice2;
    public Button choice3;

    public Button twoChoices1;
    public Button twoChoices2;


    //References for ESV Game Objects: 
    public Transform contentWindow;
    public CanvasGroup scroll_canvasgroup;
    public Button replyButton;
    public TextMeshProUGUI subject;
    public ScrollRect scrollRect; //For auto-scrolling
    
    //Sound Effect Audio Sources: 
    public AudioSource keyboard_sfx; 
    public AudioSource send_notif_sfx;

    //Prefabs: 
    public GameObject emailPrefab; 
    public GameObject replyPrefab;
    public GameObject notificationPrefab; 
 
    //Reference to its Inbox Email Object
    public GameObject inbox_email; 

    //Prefab Value-Holders:
    string name_line;
    string timeAndDate; 
    public string actual_content; 

    public string choice1_content;
    public string choice2_content;
    public string choice3_content;

    bool choice1_attachment;
    bool choice2_attachment;
    bool choice3_attachment;

    public bool reply_expect_attachment = false; 
    public int chosenEmail;  

    public string curr_reply; 
    bool ready_to_select = false; 
    bool replyButtonPressed = false; 
    
    public GameObject reply = null; 

    public GameObject downloadLinkGameObject; 
    public bool assignToDLGameObject = false; 

    //Holds Player Name. (Consider Moving to Central Tab)
    public string player_name; 

    //Unique Conversation ID: 
    public int convo_id; 


    // Start is called before the first frame update
    IEnumerator Start()
    {
        replyButton.GetComponent<CanvasGroup>().alpha = 0; 
        replyButton.GetComponent<CanvasGroup>().interactable = false; 
        Button activate = inbox_email.GetComponent<Button>(); //Deals with activating whole view
        makeTrasparent(); //Deals with activating whole view
        activate.onClick.AddListener(makeViewable);//Deals with activating whole view
        deactivate.onClick.AddListener(makeTrasparent); //Deals with activating whole view
        exitButton.onClick.AddListener(makeTrasparent);
        spam_button.onClick.AddListener(makeTrasparent);
        spam_button.onClick.AddListener(makeButtonPrioritizeTab);
        deactivate.onClick.AddListener(makeButtonPrioritizeTab);
        emailPrefab.GetComponent<EmailStringView>().download_file.SetActive(false);
        replyPrefab.GetComponent<ReplyActionScript>().fileNavigatorManager = fileNavigatorManager;
        replyPrefab.GetComponent<ReplyActionScript>().emailDialougeManager = emailDialougeManager;
        notificationPrefab.GetComponent<EmailNotificationScript>().convo_id = convo_id; 
        yield return new WaitForSeconds(0.1f); 
        reply = null; 
        GameEvents.EmailNotifPressed.Subscribe(openViewToNotif);
    }
    
    void makeViewable(){//Deals with activating whole view
        scroll_canvasgroup.gameObject.SetActive(true);
        scroll_canvasgroup.alpha = 1; 
        scroll_canvasgroup.interactable = true; 
        scroll_canvasgroup.blocksRaycasts = true;
    }

    //Consider putting in utility file?
    void makeTrasparent(){//Deals with activating whole view
        scroll_canvasgroup.gameObject.SetActive(false);
        scroll_canvasgroup.alpha = 0; 
        scroll_canvasgroup.interactable = false;
        scroll_canvasgroup.blocksRaycasts = false;
    }

    public bool waitOnReply(List<string> choices){
        if(replyButton.GetComponent<ChoiceSelection>().pressed){
            hideReplyButton();
            makeButtonPrioritizeTab(); 
            ready_to_select = true;
            replyButton.GetComponent<ChoiceSelection>().pressed = false; 
            choiceManagerScript.changeAllChoices(choices);
            StartCoroutine(choiceManagerScript.turnOnChoices(choices.Count));
            reply = Instantiate(replyPrefab, contentWindow); 
            reply.GetComponent<ReplyActionScript>().respondent.text = "Replying to " + curr_reply; 
            autoScroll.scrollVertically(scrollRect);
            return true;
        }
        return false;
    }

    //Maybe put in utility file (if the variables are consistant/can be removed from this function --> Nodes)
    public bool waitOnInput(List<string> choices){  
        if(ready_to_select){
            if(choices.Count == 3){
                if (choice1.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 0; 
                    actual_content = choices[0];
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice1.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (choice2.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 1; 
                    actual_content = choices[1];
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (choice3.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 2; 
                    actual_content = choices[2];
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice3.GetComponent<ChoiceSelection>().pressed = false;
                    return true;  
                } 
                else if(choiceManagerScript.tappedOut){
                    ready_to_select = false;
                    StartCoroutine(replyTapOut(choices));
                }
            }
            else if (choices.Count == 2){
                if (twoChoices1.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 0; 
                    actual_content = choices[0];
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    twoChoices1.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (twoChoices2.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 1; 
                    actual_content = choices[1]; 
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    twoChoices2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if(choiceManagerScript.tappedOut){
                    ready_to_select = false;
                    StartCoroutine(replyTapOut(choices));
                }
            }
            else{
                if (choice2.GetComponent<ChoiceSelection>().pressed){
                    chosenEmail = 0; 
                    actual_content = choices[0];
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if(choiceManagerScript.tappedOut){
                    ready_to_select = false;
                    StartCoroutine(replyTapOut(choices));
                }
            }
        }
        return false; 
    }

    IEnumerator replyTapOut(List<string> choices){
        if (reply != null){
            Destroy(reply);
            reply = null; 
        }
        yield return StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
        showReplyButton();
        yield return new WaitUntil(() => waitOnReply(choices)); 
    }

    public delegate bool delegate_reply(); 
    public delegate bool delegate_attach();

    public IEnumerator TypeReply(GameObject reply, string text, int attachment_file_id){ 
        Destroy(reply.GetComponent<ReplyActionScript>().pending_text); 
        keyboard_sfx.Play();
        string currentText = ""; 
        foreach(char letter in text.ToCharArray()){
            currentText += letter; 
            reply.GetComponent<ReplyActionScript>().reply_content.text = currentText; 
            yield return new WaitForSeconds(0.05f); 
        } 
        keyboard_sfx.Stop();
        reply.GetComponent<ReplyActionScript>().expected_file_id = attachment_file_id; 
        if (attachment_file_id != -1){
            reply.GetComponent<ReplyActionScript>().attachementButton.GetComponent<CanvasGroup>().alpha = 1; 
            reply.GetComponent<ReplyActionScript>().attachementButton.GetComponent<CanvasGroup>().interactable = true; 
            yield return new WaitUntil(() => reply.GetComponent<ReplyActionScript>().attachementButton.GetComponent<ChoiceSelection>().pressed); 
            reply.GetComponent<ReplyActionScript>().attachementButton.GetComponent<CanvasGroup>().alpha = 0; 
            reply.GetComponent<ReplyActionScript>().attachementButton.GetComponent<CanvasGroup>().interactable = false; 
            reply.GetComponent<ReplyActionScript>().expecting_attachment = attachment_file_id != -1;
            reply.GetComponent<ReplyActionScript>().dragFileInstructions.alpha = 1; 
            reply.GetComponent<TBPulsatingScript>().ActivatePulsation();
            delegate_attach waitAttach = () => waitOnAttach(reply);
            yield return new WaitUntil(()=> waitAttach()); 
            reply.GetComponent<ReplyActionScript>().attachment_has_been_attached = false;
            reply.GetComponent<ReplyActionScript>().expecting_attachment = false; 
            reply.GetComponent<ReplyActionScript>().dragFileInstructions.alpha = 0; 
            reply.GetComponent<TBPulsatingScript>().DeactivatePulsation();
        } 
        reply.GetComponent<ReplyActionScript>().sendButton.interactable = true;
        delegate_reply waitSend = () => waitOnSend(reply);
        yield return new WaitUntil(()=> waitSend());
        send_notif_sfx.Play(); 
        inbox_email.GetComponent<InboxEmail>().isPressed();
    }

    bool waitOnSend(GameObject reply){ 
        return reply.GetComponent<ReplyActionScript>().sendButton.GetComponent<ChoiceSelection>().pressed || (scroll_canvasgroup.alpha == 1 && Input.GetKeyDown(KeyCode.Return)); 
    }

    bool waitOnAttach(GameObject reply){
        return reply.GetComponent<ReplyActionScript>().attachment_has_been_attached;
    }

    void setNotification(string header, string subject_peak, string content_peak){
        notificationPrefab.GetComponent<EmailNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = header; 
        notificationPrefab.GetComponent<EmailNotificationScript>().subject_peak.GetComponent<TextMeshProUGUI>().text = "Subject: Re: " + subject_peak; 
        notificationPrefab.GetComponent<EmailNotificationScript>().content_peak.GetComponent<TextMeshProUGUI>().text = content_peak; 
        notificationPrefab.GetComponent<EmailNotificationScript>().notifContainer = notifContainer;  
    }

    void makeButtonPrioritizeTab(){
        Email_CentralObject.transform.SetSiblingIndex(6);
    }

    void hideReplyButton(){
        replyButton.GetComponent<CanvasGroup>().interactable = false;
        replyButton.GetComponent<CanvasGroup>().alpha = 0; 
    }

    public void showReplyButton(){  
        replyButton.GetComponent<CanvasGroup>().alpha = 1; 
        replyButton.GetComponent<CanvasGroup>().interactable = true; 
    }

    void openViewToNotif(int notif_convo_id){
        if (notif_convo_id == convo_id){
            Debug.Log("Recieved correct");
            makeViewable(); 
        }
        else{
            Debug.Log("Recieved incorrect");
        }
    }
}