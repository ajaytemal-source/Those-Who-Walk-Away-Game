using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;

using TEAMS_Dialogue_Nodes;
using SaveConversation;

public enum Thread {
    None = 0,
    Eric = 1,
    IRYS = 2
}

public class TeamsDialogueManager : MonoBehaviour
{
    
    //In Scene App References 
    public CanvasGroup Teams_CanvasGroup; 
    public GameObject Teams_CentralObject; 
    public RectTransform notifContainer; //Reference to In Scene Notif Container 

    //References to Different Backgrounds
    public CanvasGroup Home_Background; 
    public CanvasGroup Chat_Background; 
    public CanvasGroup duplicateError; 

    //References to Different Threads
    public GameObject EricThread;
    public GameObject IRYSThread; 
    public RectTransform EricThreadContentWindow; 
    public RectTransform IRYSThreadContentWindow;

    //Loading Screen References 
    public GameObject loadingScreen; 
    public GameObject loadingAnimation; 

    //Sound Effect Audio Sources: 
    public AudioSource keyboard_sfx; 
    public AudioSource recieve_message_sfx;
    public AudioSource send_message_sfx; 

    //Prefab References 
    public GameObject firstResponse;
    public GameObject followingResponse;
    public GameObject notificationPrefab; 
    public GameObject EricScreenshot; 
    public GameObject UserDoesntExistError;
    public GameObject addedClaraMessage; 

    //Thread References:
    //*Everything belonging to Eric eventually is reused for Renes 
    public CanvasGroup EricIsTypingSign; //Becomes Renes 
    public CanvasGroup ClaraIsTypingSign; 
    public CanvasGroup BothAreTypingSign; 
    public CanvasGroup IrysIsTypingSign;
    public GameObject EricTypingBar; //Becomes Renes 
    public ScrollRect EricThreadScroll; //Becomes Renes 
    public GameObject EricTypingBarScroll; //Becomes Renes 
    public GameObject IrysTypingBar; 
    public ScrollRect IrysThreadScroll; 
    public GameObject IrysTypingBarScroll; 
    public TextMeshProUGUI EricTypingBarInput; //Becomes Renes
    public TextMeshProUGUI IrysTypingBarInput;
    public Button EricSendButton; //Becomes Renes 
    public Button IrysSendButton; 
    bool ericIsTyping = false; //Becomes Renes 
    bool claraIsTyping = false; 
    bool irysIsTyping = false; 
    bool ericTypingBarPressed = false; //Becomes Renes 
    bool irysTypingBarPressed = false; 
    //wtf

    //Contact Unread Messages Indicator
    public GameObject EricUnreadMessages; //Becomes Renes 
    public GameObject IrysUnreadMessages; 
    int ericUnreadCount; //Becomes Renes 
    int irysUnreadCount; 
    
    public string playerChatlog = Application.streamingAssetsPath + "/playerChatlog.txt"; 
    public string ericIRYSChatLog = Application.streamingAssetsPath + "/EricIRYSChatlog.txt"; 

    //References to Thread Buttons
    public Button EricContactButton; //Becomes Renes 
    public TextMeshProUGUI EricContactButtonTMP; //Becomes Both Eric and Clara, Then Renes 
    public TextMeshProUGUI EricMessagesTitle; //Becomes Eric and Clara, Then Renes 
    public Button IRYSContactButton; 

    public Image EricThreadTitlePfp; 
    public TextMeshProUGUI EricThreadTitlePfpInitial; 
    public Image EricContactPfp; 
    public TextMeshProUGUI EricContactPfpInitial;

    //Currently Opened Thread 
    Thread currentThread; 

    //Relevant Script References:
    public WindowResizerScript windowResizerScript;
    public ChoiceManagerScript choiceManagerScript; 
    public AutoScroll autoScroll;
    public GlobalGameEventScript globalGameEventScript; 
    public UpdateTimeScript updateTimeScript;
    public SaveFileManager saveFileManager;

    //Input Panel References 
    public Button choice1; 
    public Button choice2; 
    public Button choice3; 
    public Button twoChoices1;
    public Button twoChoices2;

    //Start button game object references
    public Button startButton; 
    public Button minimzeButton; 
    public Button exitButton;

    //Taskbar Icon Prefab
    public GameObject Teams_TaskBarIcon; 

    //Reference to Taskbar View 
    public RectTransform taskbar_view; 

    //Taskbar Icon Holder
    GameObject currTaskBarIcon = null; 
    public bool currently_open = false; //The state of the app, being currently open or closed 

    //
    bool EricLastResponded = false; 
    bool ClaraLastResponded = false; 
    bool IrysLastResponded = false; 

    //Holds Response/Choice Set Objects: 
    private Dictionary<int, Dictionary<int, Response>> conversation_response_dict = new();
    private Dictionary<int, Dictionary<int, Choice_Set>> conversation_choices_dict = new();
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    //Central values given by GGM: 
    public string player_name = "Ajay"; 
    public string player_initial; 
    public Color playerPfpColor; 
    public string date; 

    //Prefabs:
    public GameObject initial_response_prefab; 
    public GameObject subsequent_response_prefab; 

    //For Phase 6, when Teams's progression depends on choice made in IRYS 
    int dependentChoiceIndex = -1; 

    // Start is called before the first frame update
    void Start()
    {
        close(); 
        closeThread(Thread.Eric);
        closeThread(Thread.IRYS);
        startButton.onClick.AddListener(openApp);
        minimzeButton.onClick.AddListener(minimize);
        exitButton.onClick.AddListener(close);
        EricContactButton.onClick.AddListener(() => viewThread(Thread.Eric));
        IRYSContactButton.onClick.AddListener(() => viewThread(Thread.IRYS));
        GameEvents.TeamsNotifPressed.Subscribe(openToNotif);
        GameEvents.SendChoiceToTeams.Subscribe(recieveOtherScriptChoice);
        ColorUtility.TryParseHtmlString("#A68C7B", out playerPfpColor);
    }

    public void startConversation(string dialogue_to_parse_file, int convo_id, Thread thisThread){
        TextAsset teams_dialogue_json = Resources.Load<TextAsset>(dialogue_to_parse_file);
        conversation_response_dict[convo_id] = new Dictionary<int, Response>(); 
        conversation_choices_dict[convo_id] = new Dictionary<int, Choice_Set>();
        ParseDialogue(teams_dialogue_json, convo_id);
        StartCoroutine(TeamsConversation(convo_id, dialogue_to_parse_file, thisThread)); 
    }

    private void ParseDialogue(TextAsset json_file, int convo_id){
       RawNodeCollection nodeList = JsonUtility.FromJson<RawNodeCollection>(json_file.text);

        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];
        

        foreach (RawNode node in nodeList.nodes){
            if (node.type == "response"){
                Response response = new Response{
                    id = node.id,
                    character = node.character,
                    output = node.output, 
                    typing_intervals = node.typing_intervals,
                    stallValue = node.stallValue, 
                    dialogueTriggerPairCollection = node.dialogueTriggerPairCollection,
                    stallAfterRespond = node.stallAfterRespond,
                    next_response_delay = node.next_response_delay, 
                    next = node.next,
                    triggerGlitch = node.triggerGlitch,
                    removeStall = node.removeStall 
                };
                response_dict[response.id] = response; 
            }
            else if (node.type == "choice_set"){
                Choice_Set choice_set = new Choice_Set{
                    id = node.id, 
                    choices = node.choices,
                    isWaitingForIRYS = node.isWaitingForIRYS,
                    otherScriptDepends = node.otherScriptDepends,
                    nextIsChoice = node.nextIsChoice
                };
                choice_set_dict[choice_set.id] = choice_set; 
            }
        }    
    } 

    IEnumerator TeamsConversation(int convo_id, string file_name, Thread thisThread){
        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];

        List<Dialogue_Save> made_responses = new List<Dialogue_Save>();
        List<Dialogue_Save> made_choices = new List<Dialogue_Save>();

        Color responsePfpColor; 
        string responseInitial; 
        int chosenChoice = -1; 
        Response curr_response = null; 
        Choice_Set curr_choices = null;
        RectTransform contentWindow; 
        ScrollRect threadScroll; 
        GameObject typingBar; 
        GameObject typingBarScroll; 
        TextMeshProUGUI textInput;
        Button sendButton; 
        bool isResponse;  
        int curr_id = 0;   

        switch (thisThread){
            case Thread.Eric:
                contentWindow = EricThreadContentWindow;
                typingBar = EricTypingBar;
                typingBarScroll = EricTypingBarScroll; 
                textInput = EricTypingBarInput;
                threadScroll = EricThreadScroll;
                sendButton = EricSendButton; 
                ColorUtility.TryParseHtmlString("#617F7D", out responsePfpColor);
                responseInitial = "E"; 
                break;
            case Thread.IRYS:
                typingBar = IrysTypingBar;
                contentWindow = IRYSThreadContentWindow;
                textInput = IrysTypingBarInput;
                threadScroll = IrysThreadScroll;
                typingBarScroll = IrysTypingBarScroll; 
                sendButton = IrysSendButton;
                ColorUtility.TryParseHtmlString("#708CC3", out responsePfpColor);
                responseInitial = "C"; 
                break;
            default: 
                Debug.Log("Should not happen");
                typingBar = EricTypingBar;
                textInput = EricTypingBarInput;
                typingBarScroll = EricTypingBarScroll;
                sendButton = EricSendButton;
                contentWindow = EricThreadContentWindow;
                threadScroll = EricThreadScroll;
                ColorUtility.TryParseHtmlString("#708CC3", out responsePfpColor);
                responseInitial = "C"; 
                break; 
        }

        if (response_dict.Keys.Min() == 0){
            isResponse = true; 
        }
        else{
            isResponse = false; 
        }
        do{
            if(isResponse){
                curr_response = response_dict[curr_id];
                if(curr_response.stallValue != ConversationStall.None && !curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
                if(curr_response.triggerGlitch != -1){
                    GameEvents.TriggerGlitchReciever.Raise(curr_response.triggerGlitch);
                }
                if (curr_response.next_response_delay == -1){
                    yield return StartCoroutine(beginResponse(curr_response, thisThread, contentWindow, threadScroll));
                    isResponse = false;
                }
                else{
                    StartCoroutine(beginResponse(curr_response, thisThread, contentWindow, threadScroll));
                    yield return new WaitForSeconds(curr_response.next_response_delay); 
                    isResponse = true; 
                }
                made_responses.Add(new Dialogue_Save{id = curr_response.id, time = updateTimeScript.customTime.ToString("h:mm tt")});
                if(curr_response.stallValue != ConversationStall.None && curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
                if(curr_response.removeStall != ConversationStall.None){
                    globalGameEventScript.ConversationStallList.Remove(curr_response.removeStall);
                }  
                curr_id = curr_response.next; 
            }
            else{ //Is Choice Set 
                curr_choices = choice_set_dict[curr_id];
                if (!curr_choices.isWaitingForIRYS){
                    List<string> choices = new List<string>();
                    foreach (Choice choice in curr_choices.choices){
                        choices.Add(choice.output);
                    }
                    yield return new WaitUntil(() => waitOnTypingBar(typingBar, thisThread, choices));
                    yield return new WaitUntil(() => waitOnInput(ref chosenChoice, typingBar, thisThread, choices));
                    typingBar.GetComponentInChildren<PendingResponseScript>().blinking = false; 
                    yield return StartCoroutine(TypeText(curr_choices.choices[chosenChoice].output, typingBar, typingBarScroll, sendButton, textInput, thisThread));
                    ThreadIndividualScript firstResponseScript = firstResponse.GetComponent<ThreadIndividualScript>();
                    firstResponseScript.sender.text = player_name;
                    firstResponseScript.content.text = curr_choices.choices[chosenChoice].output;
                    firstResponseScript.pfp.color = playerPfpColor;
                    firstResponseScript.sender_initial.text = player_initial; 
                    string current_time = updateTimeScript.customTime.ToString("h:mm tt");
                    firstResponseScript.time.text = current_time;
                    made_choices.Add(new Dialogue_Save{id = curr_id, chosen_choice = chosenChoice, time = current_time});
                    if (curr_choices.choices[chosenChoice].trigger != Trigger.None){
                        GameEvents.TriggerEvent.Raise(curr_choices.choices[chosenChoice].trigger);
                    }
                    if(curr_choices.choices[chosenChoice].stallValue != ConversationStall.None){
                        globalGameEventScript.ConversationStallList.Remove(curr_choices.choices[chosenChoice].stallValue); 
                    }
                    if(thisThread == Thread.IRYS){
                        addToPlayerChatlog(curr_choices.choices[chosenChoice].output); 
                    }
                    Instantiate(firstResponse, contentWindow);
                    if(curr_choices.otherScriptDepends){
                        GameEvents.SendChoiceToIRYS.Raise(chosenChoice); 
                    }
                    send_message_sfx.Play();
                    yield return new WaitForSeconds(0.25f);
                    autoScroll.scrollVertically(threadScroll);
                    curr_id = curr_choices.choices[chosenChoice].next;
                    playerLastResponded(thisThread);
                }
                else{
                    yield return new WaitUntil(() => (dependentChoiceIndex != -1));
                    curr_id = curr_choices.choices[dependentChoiceIndex].next;
                    dependentChoiceIndex = -1; 
                }
                if(curr_choices.nextIsChoice){
                    isResponse = false; 
                    curr_id = curr_choices.choices[0].next; 
                }
                else{
                    isResponse = true; 
                }
            }
        } while(curr_id != -1);
        saveFileManager.unsaved_convos.Add(new ConvoSave{file_name = file_name, convo_id = convo_id, thread = thisThread, responses = made_responses.ToArray(), choices = made_choices.ToArray()});
        GameEvents.ConversationComplete.Raise(convo_id);
        yield return null; 
    }


    IEnumerator beginResponse(Response curr_response, Thread thisThread, RectTransform contentWindow, ScrollRect threadScroll){
        for (int i = 0; i < curr_response.output.Length; i++){
            curr_response.output[i] = curr_response.output[i].Replace("${name}", player_name);
            if (curr_response.dialogueTriggerPairCollection != null){
                for (int j = 0; j < curr_response.dialogueTriggerPairCollection.Length; j++){
                    for (int k = 0; k < curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs.Length; k++){
                        if (globalGameEventScript.TriggerList.Contains(curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs[k].trigger)){
                            int triggerPairIndex = j + 1; 
                            curr_response.output[i] = curr_response.output[i].Replace((triggerPairIndex + "{trigger}"), (curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs[k].dialogue.Replace("${name}", player_name)));
                            break;
                        }
                    }
                }
            }
            yield return StartCoroutine(flipTypingSign(curr_response.typing_intervals[i].intervals, thisThread, curr_response.character));
            if (!senderLastResponded(thisThread, curr_response.character)){
                ThreadIndividualScript firstResponseScript = firstResponse.GetComponent<ThreadIndividualScript>();
                firstResponseScript.sender.text = Utility.getCharacterTeamsName(curr_response.character);
                firstResponseScript.content.text = curr_response.output[i];
                firstResponseScript.pfp.color = Utility.getCharacterTeamsColor(curr_response.character);
                firstResponseScript.sender_initial.text = Utility.getCharacterTeamsInitials(curr_response.character);
                firstResponseScript.time.text = updateTimeScript.customTime.ToString("h:mm tt");
                Instantiate(firstResponse, contentWindow);    
            }
            else{
                followingResponse.GetComponent<ThreadIndividualScript>().content.text = curr_response.output[i];
                Instantiate(followingResponse, contentWindow);
            }
            recieve_message_sfx.Play();
            if (currentThread != thisThread || Teams_CanvasGroup.alpha != 1){
                setNotification(Utility.getCharacterTeamsName(curr_response.character), curr_response.output[i], thisThread);
                Instantiate(notificationPrefab, notifContainer);
                switch (thisThread){
                    case Thread.Eric:
                        ericUnreadCount += 1; 
                        EricUnreadMessages.GetComponent<TextMeshProUGUI>().text = ericUnreadCount.ToString();
                        EricUnreadMessages.GetComponent<CanvasGroup>().alpha = 1;
                        break;
                    case Thread.IRYS:
                        irysUnreadCount += 1; 
                        IrysUnreadMessages.GetComponent<TextMeshProUGUI>().text = irysUnreadCount.ToString();
                        IrysUnreadMessages.GetComponent<CanvasGroup>().alpha = 1;
                        break;
                    default:
                        break;
                }
            }
            yield return new WaitForSeconds(0.25f);
            autoScroll.scrollVertically(threadScroll); 
        }
    }




    IEnumerator flipTypingSign(int[] interval_list, Thread thisThread, string currentCharacter){ 
        bool show_character_typing = false; //starts by not showing sign 
        foreach (int interval in interval_list){
            switch (thisThread){
                case Thread.Eric:
                    if (currentCharacter == "EA" || currentCharacter == "RD"){
                        ericIsTyping = show_character_typing;
                    }
                    else if (currentCharacter == "TRC"){ 
                        claraIsTyping = show_character_typing;
                    }
                    else{}
                    break;
                case Thread.IRYS:
                    irysIsTyping = show_character_typing;
                    break;
            }
            yield return new WaitForSeconds(interval);
            show_character_typing = !show_character_typing;
        }
        switch (thisThread){
            case Thread.Eric:
                ericIsTyping = false;
                claraIsTyping = false; 
                break;
            case Thread.IRYS:
                irysIsTyping = false;
                break;
        }
    }

    void Update(){ 
        if(currentThread == Thread.Eric && ericUnreadCount != 0){
            EricUnreadMessages.GetComponent<CanvasGroup>().alpha = 0;
            ericUnreadCount = 0;
        }
        if(currentThread == Thread.IRYS && irysUnreadCount != 0){
            IrysUnreadMessages.GetComponent<CanvasGroup>().alpha = 0;
            irysUnreadCount = 0;
        }

        if(ericIsTyping && !claraIsTyping){
            Utility.openCanvasGroup(EricIsTypingSign);
        }
        else{
            Utility.closeCanvasGroup(EricIsTypingSign);
        }

        if (claraIsTyping && !ericIsTyping){
            Utility.openCanvasGroup(ClaraIsTypingSign);
        }
        else{
            Utility.closeCanvasGroup(ClaraIsTypingSign);
        }

        if (ericIsTyping && claraIsTyping){
            Utility.openCanvasGroup(BothAreTypingSign);
        }
        else{
            Utility.closeCanvasGroup(BothAreTypingSign);
        }

        if (irysIsTyping){
            Utility.openCanvasGroup(IrysIsTypingSign);
        }
        else{
            Utility.closeCanvasGroup(IrysIsTypingSign);
        }
    }

    bool waitOnTypingBar(GameObject typingBar, Thread thisThread, List<string> choices){ //Where I left off, working on implementation. 
        typingBar.GetComponent<Button>().interactable = true; 
        typingBar.GetComponent<TextCursorScript>().showCursor = true; 
        typingBar.GetComponent<TBPulsatingScript>().isActive = true; 
        if (typingBar.GetComponent<ChoiceSelection>().pressed && currentThread == thisThread){
            typingBar.GetComponent<TBPulsatingScript>().isActive = false;
            ref bool thisTypingBarPressed = ref ericTypingBarPressed; //Have this here to bypass error 
            switch (thisThread){
                case Thread.Eric:
                    thisTypingBarPressed = ref ericTypingBarPressed;
                    break;
                case Thread.IRYS:
                    thisTypingBarPressed = ref irysTypingBarPressed;
                    break;
                default:
                    break;
            }
            thisTypingBarPressed = true; 
            choiceManagerScript.changeAllChoices(choices); //
            StartCoroutine(choiceManagerScript.turnOnChoices(choices.Count));
            typingBar.GetComponent<TextCursorScript>().showCursor = false; 
            typingBar.GetComponent<Button>().interactable = false;
            typingBar.GetComponentInChildren<PendingResponseScript>().blinking = true; 
            return true; 
        }
        return false; 
    }

    bool waitOnInput(ref int chosenChoiceHolder, GameObject typingBar, Thread thisThread, List<string> choices){
        ref bool thisTypingBarPressed = ref ericTypingBarPressed; 
        switch (thisThread){
            case Thread.Eric:
                thisTypingBarPressed = ref ericTypingBarPressed;
                break;
            case Thread.IRYS:
                thisTypingBarPressed = ref irysTypingBarPressed;
                break;
            default:
                break;
        }
        if (thisTypingBarPressed){
            if(choices.Count == 3){
                if (choice1.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 0; 
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice1.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (choice2.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 1; 
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (choice3.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 2;
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice3.GetComponent<ChoiceSelection>().pressed = false;
                    return true;  
                } 
                else if(choiceManagerScript.tappedOut){
                    typingBar.GetComponentInChildren<PendingResponseScript>().blinking = false; 
                    thisTypingBarPressed = false; 
                    StartCoroutine(typingBarTapOut(typingBar, thisThread, choices));
                }
            }
            else if (choices.Count == 2){
                if (twoChoices1.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 0;
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    twoChoices1.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if (twoChoices2.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 1; 
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    twoChoices2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if(choiceManagerScript.tappedOut){
                    typingBar.GetComponentInChildren<PendingResponseScript>().blinking = false;
                    thisTypingBarPressed = false; 
                    StartCoroutine(typingBarTapOut(typingBar, thisThread, choices));
                }
            }
            else{
                if (choice2.GetComponent<ChoiceSelection>().pressed){
                    chosenChoiceHolder = 0; 
                    StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
                    choice2.GetComponent<ChoiceSelection>().pressed = false; 
                    return true;  
                }
                else if(choiceManagerScript.tappedOut){
                    typingBar.GetComponentInChildren<PendingResponseScript>().blinking = false;
                    thisTypingBarPressed = false; 
                    StartCoroutine(typingBarTapOut(typingBar, thisThread, choices));
                }
            }
        }
        return false; 
    }

    IEnumerator typingBarTapOut(GameObject typingBar, Thread thisThread, List<string> choices){
        yield return StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count));
        yield return new WaitUntil(() => waitOnTypingBar(typingBar, thisThread, choices));  
    }

    IEnumerator TypeText(string text, GameObject typingBar, GameObject typingBarScroll, Button sendButton, TextMeshProUGUI textInput, Thread thisThread){
        typingBarScroll.gameObject.SetActive(true);
        keyboard_sfx.Play();
        string currentText = ""; 
        foreach(char letter in text.ToCharArray()){
            currentText += letter; 
            textInput.text = currentText;
            autoScroll.scrollHorizontally(typingBar.GetComponentInChildren<ScrollRect>());  
            yield return new WaitForSeconds(0.05f);
        }
        keyboard_sfx.Stop();
        sendButton.interactable = true; 
        yield return new WaitUntil(() => waitOnSend(sendButton, thisThread));
        typingBarScroll.gameObject.SetActive(false);
        sendButton.interactable = false; 
        textInput.text = ""; 
    }

    bool waitOnSend(Button sendButton, Thread thisThread){
        if(sendButton.GetComponent<ChoiceSelection>().pressed || (currentThread == thisThread && Input.GetKeyDown(KeyCode.Return))){
            sendButton.GetComponent<ChoiceSelection>().pressed = false; 
            return true; 
        }
        else{
            return false; 
        }
    }

    bool senderLastResponded(Thread thisThread, string currentCharacter){
        switch (thisThread){
            case Thread.Eric:
                if(EricLastResponded && (currentCharacter == "EA" || currentCharacter == "RD")){
                    EricLastResponded = true;
                    ClaraLastResponded = false; 
                    return true;
                }
                else if(ClaraLastResponded && currentCharacter == "TRC"){
                    ClaraLastResponded = true; 
                    EricLastResponded = false;
                    return true;
                }
                else if (currentCharacter == "EA" || currentCharacter == "RD"){
                    EricLastResponded = true;
                    ClaraLastResponded = false; 
                    return false;
                }
                else if (currentCharacter == "TRC"){
                    ClaraLastResponded = true; 
                    EricLastResponded = false;
                    return false;
                }  
                else{return false;}        
            case Thread.IRYS:
                if(IrysLastResponded && currentCharacter == "CL"){
                    IrysLastResponded = true; 
                    return true;
                }
                else{
                    IrysLastResponded = true; 
                    return false;
                }
            default:
                return false; 
            return false; 
        }
    }

    void playerLastResponded(Thread thisThread){
        switch (thisThread){
            case Thread.Eric:
                EricLastResponded = false; 
                ClaraLastResponded = false; 
                break;
            case Thread.IRYS:
                IrysLastResponded = false; 
                break;
            default:
                break; 
        }
    }

    void openToNotif(Thread notifThread){
        if(currently_open){
            reopen();
        }
        else{
            openApp(); 
        }
        switch (notifThread){
            case Thread.Eric:
                viewThread(Thread.Eric);
                break; 
            case Thread.IRYS:
                viewThread(Thread.IRYS);
                break;
        }
    }

    void setNotification(string contact, string content_peak, Thread thisThread){
        notificationPrefab.GetComponent<TeamsNotificationScript>().contact.text = contact; 
        notificationPrefab.GetComponent<TeamsNotificationScript>().content_peak.text = content_peak;
        notificationPrefab.GetComponent<TeamsNotificationScript>().notifContainer = notifContainer;
        notificationPrefab.GetComponent<TeamsNotificationScript>().thisThread = thisThread;
    }

    void viewThread(Thread threadToOpen){
        if(currentThread == Thread.None){
            Home_Background.alpha = 0; 
            Chat_Background.alpha = 1; 
            openThread(threadToOpen);
            currentThread = threadToOpen;
        }
        else if (currentThread != threadToOpen){
            closeThread(currentThread);
            openThread(threadToOpen);
            currentThread = threadToOpen;
        }
    }

    void closeThread(Thread threadToClose){
        switch (threadToClose){
            case Thread.Eric:
                EricThread.SetActive(false);
                break; 
            case Thread.IRYS:
                IRYSThread.SetActive(false);
                break;
        }
    }

    void openThread(Thread threadToOpen){
        switch (threadToOpen){
            case Thread.Eric:
                EricThread.SetActive(true);
                autoScroll.scrollVertically(EricThreadScroll);
                break; 
            case Thread.IRYS:
                IRYSThread.SetActive(true);
                autoScroll.scrollVertically(IrysThreadScroll);
                break;
        }
    }   

    IEnumerator openLoadingAnimation(){
        loadingScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        loadingAnimation.gameObject.SetActive(true);
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3f));
        loadingAnimation.gameObject.SetActive(false);
        loadingScreen.gameObject.SetActive(false);
    }


    public void close(){
        if (currTaskBarIcon != null){
            Destroy(currTaskBarIcon); 
        }
        Utility.closeCanvasGroup(duplicateError); 
        currently_open = false; 
        closeThread(Thread.Eric);
        closeThread(Thread.IRYS);
        Home_Background.alpha = 1; 
        Chat_Background.alpha = 0; 
        currentThread = Thread.None; 
        minimize();
    }

    void minimize(){
        Utility.windowMinimize(Teams_CanvasGroup, windowResizerScript);
    }

    void reopen(){
        Utility.windowReopen(Teams_CentralObject, Teams_CanvasGroup, windowResizerScript);
    }

    public void openApp(){
        StartCoroutine(delayedOpen());
    }

    public void openAppToError(){
        StartCoroutine(delayError()); 
    }

    IEnumerator delayError(){
        open();
        yield return new WaitForSeconds(0.5f);
        Utility.openCanvasGroup(duplicateError);  
    }

    IEnumerator delayedOpen(){
        yield return new WaitForSeconds(0.7f);
        open();
    }

    public void open(){
        Utility.windowOpen(Teams_CentralObject, Teams_CanvasGroup, windowResizerScript, currently_open, new Vector2(-10f, 30f));
        if(!currently_open){
            StartCoroutine(openLoadingAnimation());
            currTaskBarIcon = Instantiate(Teams_TaskBarIcon, taskbar_view);
            currTaskBarIcon.GetComponent<Button>().onClick.AddListener(openOrClose);
            currently_open = true;
        }
        reopen();
    }

    void openOrClose(){
        Utility.tbIconOpenOrClose(Teams_CentralObject, Teams_CanvasGroup, windowResizerScript);
    }
    
    void recieveOtherScriptChoice(int choice_index){
        dependentChoiceIndex = choice_index; 
    }

    public void sendEricScreenshot(int index = -1){
        if (index == -1){
            string newScreenshotTime = updateTimeScript.customTime.Subtract(TimeSpan.FromMinutes(4)).ToString("h:mm tt");
            EricScreenshot.GetComponent<EricScreenshotScript>().time.text =  newScreenshotTime;
            int newIndex = Instantiate(EricScreenshot, IRYSThreadContentWindow).transform.GetSiblingIndex();
            saveFileManager.ericScreenshotIndex = newIndex; 
            saveFileManager.ericScreenshotTime = newScreenshotTime; 
            addToPlayerChatlog("2025-08-08 " + updateTimeScript.customTime.ToString("HH:mm") + " —  Clara\nIMG.png\n");
        }
        else{
            EricScreenshot.GetComponent<EricScreenshotScript>().time.text =  saveFileManager.ericScreenshotTime;
            GameObject screenshot = Instantiate(EricScreenshot, IRYSThreadContentWindow);
            screenshot.transform.SetSiblingIndex(index); 
        }
    }

    void addToPlayerChatlog(string content){
        string addedLine = "2025-08-08 " + updateTimeScript.customTime.ToString("HH:mm") + " — " + player_name + ":\n" + content +"\n"; 
        using (StreamWriter writer = new StreamWriter(playerChatlog, true))
        {
            writer.WriteLine(addedLine);
        }
    }

    public void updateEricIRYSChatLogTime(){
        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/EricIRYSChatlogTemplate.txt");
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace("${name}", player_name);
        }
        DateTime curr_time = updateTimeScript.customTime;
        lines[0] = lines[0].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(23)).ToString("HH:mm"));
        lines[3] = lines[3].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(21)).ToString("HH:mm"));
        lines[7] = lines[7].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(21)).ToString("HH:mm"));
        lines[10] = lines[10].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(19)).ToString("HH:mm"));
        lines[18] = lines[18].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(19)).ToString("HH:mm"));
        lines[23] = lines[23].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(17)).ToString("HH:mm"));
        lines[28] = lines[28].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(17)).ToString("HH:mm"));
        lines[33] = lines[33].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(16)).ToString("HH:mm"));
        lines[37] = lines[37].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(15)).ToString("HH:mm"));
        lines[42] = lines[42].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(15)).ToString("HH:mm"));
        lines[48] = lines[48].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(14)).ToString("HH:mm"));
        lines[55] = lines[55].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(14)).ToString("HH:mm"));
        lines[60] = lines[60].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(13)).ToString("HH:mm"));
        lines[64] = lines[64].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(11)).ToString("HH:mm"));
        lines[70] = lines[70].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(10)).ToString("HH:mm"));
        lines[76] = lines[76].Insert(11, curr_time.Subtract(TimeSpan.FromMinutes(6)).ToString("HH:mm"));
        File.WriteAllLines(ericIRYSChatLog, lines);
    }

    public IEnumerator sendUserErrorMessageToIRYS(){
        Instantiate(UserDoesntExistError, IRYSThreadContentWindow);
        yield return new WaitForSeconds(0.2f); 
        autoScroll.scrollVertically(IrysThreadScroll);
        IRYSContactButton.gameObject.SetActive(false);
    }
    
    public IEnumerator sendAddedClaraMessage(){
        Instantiate(addedClaraMessage, EricThreadContentWindow);
        yield return new WaitForSeconds(0.2f);
        EricContactButtonTMP.text = "Eric, The_Real_Clara"; 
        EricMessagesTitle.text = "Eric, The_Real_Clara"; 
        autoScroll.scrollVertically(IrysThreadScroll);
    }

    public IEnumerator removeEricAndClara(){
        UserDoesntExistError.GetComponentInChildren<TextMeshProUGUI>().text = "The users [Eric] and [The_Real_Clara] no longer exist.";  
        Instantiate(UserDoesntExistError, EricThreadContentWindow);
        yield return new WaitForSeconds(0.2f);
        autoScroll.scrollVertically(EricThreadScroll);
    }

    public IEnumerator changeToRenesThread(){
        Utility.closeCanvasGroup(EricContactButton.GetComponent<CanvasGroup>());
        yield return new WaitForSeconds(5f); 
        closeThread(Thread.Eric);
        Home_Background.alpha = 1; 
        Chat_Background.alpha = 0; 
        for (int i = 1; i < EricThreadContentWindow.childCount; i++){
            GameObject.Destroy(EricThreadContentWindow.GetChild(i).gameObject);
        }
        currentThread = Thread.None;
        EricIsTypingSign.GetComponent<TextMeshProUGUI>().text = "Renes_Damain is typing...";
        ericUnreadCount = 0; 
        EricContactButtonTMP.text = "Renes_Damain";
        EricMessagesTitle.text = "Renes_Damian";
        EricThreadTitlePfp.color = Utility.getCharacterTeamsColor("RD"); 
        EricContactPfp.color = Utility.getCharacterTeamsColor("RD"); 
        EricThreadTitlePfpInitial.text = "R";
        EricContactPfpInitial.text = "R"; 
        yield return new WaitForSeconds(5f);
        Utility.openCanvasGroup(EricContactButton.GetComponent<CanvasGroup>());
    }



    //SAVE IMPLEMENTATION: 

    public void loadConversation(ConvoSave convo){
        int convo_id = convo.convo_id; 
        Thread thisThread = convo.thread; 
        TextAsset teams_dialogue_json = Resources.Load<TextAsset>(convo.file_name);
        conversation_response_dict[convo_id] = new Dictionary<int, Response>(); 
        conversation_choices_dict[convo_id] = new Dictionary<int, Choice_Set>();
        ParseDialogue(teams_dialogue_json, convo_id);
        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];

        //NEW FOR THIS 
        Dictionary<int, Dialogue_Save> saved_response_dict = new Dictionary<int, Dialogue_Save>();
        Dictionary<int, Dialogue_Save> saved_choice_dict = new Dictionary<int, Dialogue_Save>();

        foreach (Dialogue_Save response in convo.responses){
            saved_response_dict[response.id] = response; 
        }
        foreach (Dialogue_Save choice in convo.choices){
            saved_choice_dict[choice.id] = choice; 
        }
        //

        Color responsePfpColor; 
        string responseInitial; 
        int chosenChoice = -1; 
        Response curr_response = null; 
        Choice_Set curr_choices = null;
        RectTransform contentWindow; 
        ScrollRect threadScroll; 
        GameObject typingBar; 
        GameObject typingBarScroll; 
        TextMeshProUGUI textInput;
        Button sendButton; 
        bool isResponse;  
        int curr_id = 0;   

        switch (thisThread){
            case Thread.Eric:
                contentWindow = EricThreadContentWindow;
                typingBar = EricTypingBar;
                typingBarScroll = EricTypingBarScroll; 
                textInput = EricTypingBarInput;
                threadScroll = EricThreadScroll;
                sendButton = EricSendButton; 
                ColorUtility.TryParseHtmlString("#617F7D", out responsePfpColor);
                responseInitial = "E"; 
                break;
            case Thread.IRYS:
                typingBar = IrysTypingBar;
                contentWindow = IRYSThreadContentWindow;
                textInput = IrysTypingBarInput;
                threadScroll = IrysThreadScroll;
                typingBarScroll = IrysTypingBarScroll; 
                sendButton = IrysSendButton;
                ColorUtility.TryParseHtmlString("#708CC3", out responsePfpColor);
                responseInitial = "C"; 
                break;
            default: // Will set to R
                Debug.Log("Should not happen");
                typingBar = EricTypingBar;
                textInput = EricTypingBarInput;
                typingBarScroll = EricTypingBarScroll;
                sendButton = EricSendButton;
                contentWindow = EricThreadContentWindow;
                threadScroll = EricThreadScroll;
                ColorUtility.TryParseHtmlString("#708CC3", out responsePfpColor);
                responseInitial = "C"; 
                break; 
        }

        if (response_dict.Keys.Min() == 0){
            isResponse = true; 
        }
        else{
            isResponse = false; 
        }
        do{
            if(isResponse){
                curr_response = response_dict[curr_id];
                for (int i = 0; i < curr_response.output.Length; i++){
                    curr_response.output[i] = curr_response.output[i].Replace("${name}", player_name);
                    if (curr_response.dialogueTriggerPairCollection != null){
                        for (int j = 0; j < curr_response.dialogueTriggerPairCollection.Length; j++){
                            for (int k = 0; k < curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs.Length; k++){
                                if (globalGameEventScript.TriggerList.Contains(curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs[k].trigger)){
                                    int triggerPairIndex = j + 1; 
                                    curr_response.output[i] = curr_response.output[i].Replace((triggerPairIndex + "{trigger}"), (curr_response.dialogueTriggerPairCollection[j].dialogueTriggerPairs[k].dialogue.Replace("${name}", player_name)));
                                    break;
                                }
                            }
                        }
                    }
                    if (!senderLastResponded(thisThread, curr_response.character)){
                        ThreadIndividualScript firstResponseScript = firstResponse.GetComponent<ThreadIndividualScript>();
                        firstResponseScript.sender.text = Utility.getCharacterTeamsName(curr_response.character);
                        firstResponseScript.content.text = curr_response.output[i];
                        firstResponseScript.pfp.color = Utility.getCharacterTeamsColor(curr_response.character);
                        firstResponseScript.sender_initial.text = Utility.getCharacterTeamsInitials(curr_response.character);
                        firstResponseScript.time.text = saved_response_dict[curr_id].time;
                        Instantiate(firstResponse, contentWindow);    
                    }
                    else{
                        followingResponse.GetComponent<ThreadIndividualScript>().content.text = curr_response.output[i];
                        Instantiate(followingResponse, contentWindow);
                    }
                }
                if (curr_response.next_response_delay == -1){
                    isResponse = false;
                }
                else{
                    isResponse = true; 
                }
                curr_id = curr_response.next; 
            }
            else{ //Is Choice Set 
                curr_choices = choice_set_dict[curr_id];
                int thisChosenChoice = saved_choice_dict[curr_id].chosen_choice;
                ThreadIndividualScript firstResponseScript = firstResponse.GetComponent<ThreadIndividualScript>();
                firstResponseScript.sender.text = player_name;
                firstResponseScript.content.text = curr_choices.choices[thisChosenChoice].output;
                firstResponseScript.pfp.color = playerPfpColor;
                firstResponseScript.sender_initial.text = player_initial; 
                firstResponseScript.time.text = saved_choice_dict[curr_id].time;
                Instantiate(firstResponse, contentWindow);
                curr_id = curr_choices.choices[thisChosenChoice].next;
                playerLastResponded(thisThread);
                if(curr_choices.nextIsChoice){
                    isResponse = false; 
                    curr_id = curr_choices.choices[0].next; 
                }
                else{
                    isResponse = true; 
                }
            }
        } while(curr_id != -1);
        GameEvents.ConversationComplete.Raise(convo_id);
    }

    public void resetApp(){
        StopAllCoroutines();
        EricUnreadMessages.GetComponent<CanvasGroup>().alpha = 0;
        IrysUnreadMessages.GetComponent<CanvasGroup>().alpha = 0;
        ericIsTyping = false; 
        claraIsTyping = false; 
        irysIsTyping = false; 
        ericTypingBarPressed = false; 
        irysTypingBarPressed = false;
        EricLastResponded = false; 
        ClaraLastResponded = false; 
        IrysLastResponded = false; 
        ericUnreadCount = 0; //Becomes Renes 
        irysUnreadCount= 0;
        conversation_response_dict.Clear();
        conversation_choices_dict.Clear();
        close(); 
        for (int i = 1; i < EricThreadContentWindow.childCount; i++){
            GameObject.Destroy(EricThreadContentWindow.GetChild(i).gameObject);
        }
        for (int i = 1; i < IRYSThreadContentWindow.childCount; i++){
            GameObject.Destroy(IRYSThreadContentWindow.GetChild(i).gameObject);
        }
        Utility.closeCanvasGroup(IRYSContactButton.GetComponent<CanvasGroup>()); 
        startButton.gameObject.SetActive(false); 
    }

}
