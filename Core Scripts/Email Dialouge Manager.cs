using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.IO; 
using System.Linq;
using System;

using EMAIL_Dialogue_Nodes;
using SaveConversation;

public class EmailDialougeManager : MonoBehaviour
{
    //Relevant Script References 
    public FileNavigatorManager fileNavigatorManager; 
    public WindowResizerScript windowResizerScript;
    public ChoiceManagerScript choiceManagerScript; 
    public GlobalGameEventScript globalGameEventScript; 
    public UpdateTimeScript updateTimeScript;
    public DragUI dragDropColliderScript; 
    public SaveFileManager saveFileManager; 
    
    //Main Application In Scene References 
    public CanvasGroup Email_CanvasGroup; 
    public Canvas thisCanvas; 
    public GameObject Email_CentralObject; 
    public Button startButton; 
    public Button minimzeButton; 
    public Button exitButton;  
    public Transform inboxEmailScroll; 
    public RectTransform emailScrollViewContainer;
    public Button backButton; 
    public Button spamButton; 
    public GameObject spamView;
    public GameObject empty_sign;

    //Loading Screen References 
    public GameObject loadingScreen; 
    public GameObject loadingAnimation; 

    public RectTransform taskbar_view; //Reference to Taskbar View 
    public RectTransform notifContainer; //Reference to In Scene Notif Container 

    //Choice Input Panel References 
    public Button choice1; 
    public Button choice2; 
    public Button choice3; 

    public Button twoChoices1;
    public Button twoChoices2;

    //Sound Effect Audio Sources: 
    public AudioSource recieve_notif_sfx; 
     
    //Prefabs
    public GameObject inbox_mail; 
    public GameObject email_individ_view; 
    public GameObject email_scroll_view; 
    public GameObject notificationPrefab; 
    public GameObject Email_TaskBarIcon; //Taskbar Icon Prefab 
    public GameObject emptyEmailSign; 

    //Taskbar Icon Holder
    GameObject currTaskBarIcon = null; //attrocious naming 

    bool currently_open = false; 
    bool is_empty; 

    //Holds Response/Choice Set Objects 
    private Dictionary<int, First_Email> first_email_dict = new();
    private Dictionary<int, Dictionary<int, Response>> conversation_response_dict = new();
    private Dictionary<int, Dictionary<int, Choice_Set>> conversation_choices_dict = new();

    public int reply_attached_file_id; //When the player attaches a file and sends it in a reply, this holds the id of the file 
    
    //Central values given by GGM 
    public string player_name;
    public string date; 


    // Start is called before the first frame update
    void Start()
    {
        close();
        hideSpamSign();
        is_empty = true;
        startButton.onClick.AddListener(openApp);
        backButton.onClick.AddListener(hideSpamSign); 
        spamButton.onClick.AddListener(showSpamSign); 
        minimzeButton.onClick.AddListener(minimize);
        exitButton.onClick.AddListener(close);
        email_scroll_view.GetComponent<EmailScrollView>().Email_CentralObject = Email_CentralObject; 
        email_scroll_view.GetComponent<EmailScrollView>().deactivate = backButton;
        email_scroll_view.GetComponentInChildren<ScrollRectVisibility>().dragDropColliderScript = dragDropColliderScript; 
        email_scroll_view.GetComponent<EmailScrollView>().exitButton = exitButton;
        email_scroll_view.GetComponent<EmailScrollView>().spam_button = spamButton; 
        email_scroll_view.GetComponent<EmailScrollView>().choice1 = choice1;
        email_scroll_view.GetComponent<EmailScrollView>().choice2 = choice2;
        email_scroll_view.GetComponent<EmailScrollView>().choice3 = choice3;
        email_scroll_view.GetComponent<EmailScrollView>().twoChoices1 = twoChoices1;
        email_scroll_view.GetComponent<EmailScrollView>().twoChoices2 = twoChoices2;
        email_scroll_view.GetComponent<EmailScrollView>().fileNavigatorManager = fileNavigatorManager;
        email_scroll_view.GetComponent<EmailScrollView>().emailDialougeManager = gameObject.GetComponent<EmailDialougeManager>();  
        email_scroll_view.GetComponent<EmailScrollView>().choiceManagerScript = choiceManagerScript;
        email_scroll_view.GetComponent<EmailScrollView>().notifContainer = notifContainer;
        email_individ_view.GetComponent<EmailStringView>().thisCanvas = thisCanvas; 
        email_individ_view.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().notifContainer = notifContainer;
        GameEvents.EmailNotifPressed.Subscribe(openToNotif);
    }
 
    public void startConversation(string dialogue_to_parse_file, int convo_id){ 
        email_individ_view.GetComponent<EmailStringView>().date.GetComponent<TextMeshProUGUI>().text = date; //what??
        TextAsset email_dialogue_json = Resources.Load<TextAsset>(dialogue_to_parse_file);
        conversation_response_dict[convo_id] = new Dictionary<int, Response>(); 
        conversation_choices_dict[convo_id] = new Dictionary<int, Choice_Set>();
        ParseDialogue(email_dialogue_json, convo_id);
        StartCoroutine(EmailConversation(convo_id, dialogue_to_parse_file)); 
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
                    seconds_between = node.seconds_between, 
                    dialogueTriggerPairCollection = node.dialogueTriggerPairCollection,
                    output = node.output,
                    stallValue = node.stallValue, 
                    stallAfterRespond = node.stallAfterRespond,
                    is_next_response = node.is_next_response, 
                    triggerGlitch = node.triggerGlitch,
                    removeStall = node.removeStall, 
                    next = node.next 
                };
                if (node.has_download){
                    response.download = node.download; 
                }
                response_dict[response.id] = response; 
            }
            else if (node.type == "choice_set"){
                Choice_Set choice_set = new Choice_Set{
                    id = node.id, 
                    choices = node.choices 
                };
                choice_set_dict[choice_set.id] = choice_set; 
            }
            else if (node.type == "first_email"){ 
                first_email_dict[convo_id] = new First_Email{
                    id = node.id,
                    character = node.character,
                    subject = node.subject, 
                    output = node.output, 
                    stallValue = node.stallValue, 
                    stallAfterRespond = node.stallAfterRespond,
                    dialogueTriggerPairCollection = node.dialogueTriggerPairCollection,
                    is_next_response = node.is_next_response, 
                    removeStall = node.removeStall, 
                    next = node.next 
                };
                if (node.has_download){
                    first_email_dict[convo_id].download = node.download; 
                }
            }       
        }
    }


    IEnumerator EmailConversation(int convo_id, string file_name){
        if (is_empty){ //Might move from here 
            Destroy(empty_sign);
            is_empty = false;
        }
        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];

        List<Dialogue_Save> made_responses = new List<Dialogue_Save>();
        List<Dialogue_Save> made_choices = new List<Dialogue_Save>();

        Response curr_response = null; 
        Choice_Set curr_choices = null; 
        int curr_id = 0; // <-- Do First Email, then assign its next to curr id
        bool isResponse = false; // <-- Since we do the first email before the do while loop, it is 'implied' that the choice set would follow. (Can be modified to be more ambiguous later.)
        First_Email first_email = first_email_dict[convo_id]; 
        //First Email Implementation
        if(first_email.stallValue != ConversationStall.None && !first_email.stallAfterRespond){
            globalGameEventScript.ConversationStallList.Add(first_email.stallValue);
            yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(first_email.stallValue));
        }
        first_email.output = first_email.output.Replace("${name}", player_name); //NEW SYSTEM
        if (first_email.dialogueTriggerPairCollection != null){
            for (int i = 0; i < first_email.dialogueTriggerPairCollection.Length; i++){
                for (int j = 0; j < first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs.Length; j++){
                    if (globalGameEventScript.TriggerList.Contains(first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].trigger)){
                        int triggerPairIndex = i + 1; 
                        first_email.output = first_email.output.Replace((triggerPairIndex + "{trigger}"), (first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].dialogue.Replace("${name}", player_name)));
                        break;
                    }
                }
            }
        }
        InboxEmail inboxEmailScript = inbox_mail.GetComponent<InboxEmail>();
        inboxEmailScript.subject.GetComponent<TextMeshProUGUI>().text = first_email.subject + ": ";
        inboxEmailScript.sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterName(first_email.character, player_name); 
        inboxEmailScript.emailPeak.GetComponent<TextMeshProUGUI>().text = first_email.output.Replace("\n\n", " ");
        string currentTime = updateTimeScript.customTime.ToString("h:mm tt");
        inboxEmailScript.time.text = currentTime; 
        if (first_email.download != null){
            email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(true);
            email_individ_view.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().download_id = first_email.download.file_id; 
            email_individ_view.GetComponent<EmailStringView>().download_file_name.GetComponent<TextMeshProUGUI>().text = first_email.download.file_name;
            email_individ_view.GetComponent<EmailStringView>().download_file_size.GetComponent<TextMeshProUGUI>().text = first_email.download.file_size;
        }
        email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact(first_email.character, player_name);  
        email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = first_email.output;
        email_individ_view.GetComponent<EmailStringView>().time.text = currentTime;
        made_responses.Add(new Dialogue_Save{id = curr_id, time = currentTime});
        email_scroll_view.GetComponent<EmailScrollView>().subject.text = first_email.subject; 
        email_scroll_view.GetComponent<EmailScrollView>().convo_id = convo_id; 
        recieve_notif_sfx.Play(); 
        GameObject this_inbox_view = Instantiate(inbox_mail, inboxEmailScroll);
        GameObject this_scrollview = Instantiate(email_scroll_view, emailScrollViewContainer, false); 
        this_scrollview.GetComponent<EmailScrollView>().inbox_email = this_inbox_view;
        GameObject instantiated_first_email = Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
        setNotification(Utility.getCharacterName(first_email.character, player_name), first_email.subject, first_email.output.Replace("\n\n", " "), convo_id, false); 
        Instantiate(notificationPrefab, notifContainer); 
        curr_id = first_email.next; 
        EmailScrollView scroll_view_script = this_scrollview.GetComponent<EmailScrollView>();
        scroll_view_script.curr_reply = Utility.getCharacterContact(first_email.character, player_name); 
        if (first_email.download != null){
            scroll_view_script.downloadLinkGameObject = instantiated_first_email.GetComponent<EmailStringView>().download_file;
        }
        email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(false);
        if(first_email.stallValue != ConversationStall.None && first_email.stallAfterRespond){
            globalGameEventScript.ConversationStallList.Add(first_email.stallValue);
            yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(first_email.stallValue));
        }
        if(first_email.removeStall != ConversationStall.None){
            globalGameEventScript.ConversationStallList.Remove(first_email.removeStall);
        } 
        if(first_email.is_next_response){
            isResponse = true; 
        }
        else{
            isResponse = false; 
        }
        while(curr_id != -1){
            //Subsequent Emails Implementation
            if (isResponse){
                curr_response = response_dict[curr_id];
                if(curr_response.stallValue != ConversationStall.None && !curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
                yield return new WaitForSeconds(curr_response.seconds_between); 
                curr_response.output = curr_response.output.Replace("${name}", player_name);
                if (curr_response.dialogueTriggerPairCollection != null){
                    for (int i = 0; i < curr_response.dialogueTriggerPairCollection.Length; i++){
                        for (int j = 0; j < curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs.Length; j++){
                            if (globalGameEventScript.TriggerList.Contains(curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].trigger)){
                                int triggerPairIndex = i + 1; 
                                curr_response.output = curr_response.output.Replace((triggerPairIndex + "{trigger}"), (curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].dialogue.Replace("${name}", player_name)));
                                break;
                            }
                        }
                    }
                }
                if (curr_response.download != null){
                    email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(true);
                    email_individ_view.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().download_id = curr_response.download.file_id; 
                    email_individ_view.GetComponent<EmailStringView>().download_file_name.GetComponent<TextMeshProUGUI>().text = curr_response.download.file_name;
                    email_individ_view.GetComponent<EmailStringView>().download_file_size.GetComponent<TextMeshProUGUI>().text = curr_response.download.file_size;
                    //Downloading is facilitated by GGM
                }
                this_inbox_view.GetComponent<InboxEmail>().incrementReplyCount();
                this_inbox_view.GetComponent<InboxEmail>().MoveUpIndex();
                string current_time = updateTimeScript.customTime.ToString("h:mm tt");
                this_inbox_view.GetComponent<InboxEmail>().time.text = current_time; 
                if (email_scroll_view.GetComponent<EmailScrollView>().scroll_canvasgroup.alpha == 0){
                    this_inbox_view.GetComponent<InboxEmail>().unread_Status();
                }
                this_inbox_view.GetComponent<InboxEmail>().emailPeak.GetComponent<TextMeshProUGUI>().text = curr_response.output.Replace("\n\n", " ");
                email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact(curr_response.character, player_name);  
                email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = curr_response.output;
                email_individ_view.GetComponent<EmailStringView>().time.text = current_time;
                made_responses.Add(new Dialogue_Save{id = curr_id, time = current_time});
                recieve_notif_sfx.Play();
                if(curr_response.triggerGlitch != -1){
                    GameEvents.TriggerGlitchReciever.Raise(curr_response.triggerGlitch);
                }
                GameObject instantiated_email_response = Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
                if (scroll_view_script.scroll_canvasgroup.alpha == 0){
                    setNotification(Utility.getCharacterName(curr_response.character, player_name), first_email.subject, curr_response.output.Replace("\n\n", " "), convo_id, true); 
                    Instantiate(notificationPrefab, notifContainer); 
                }
                if (curr_response.download != null){
                    scroll_view_script.downloadLinkGameObject = instantiated_email_response.GetComponent<EmailStringView>().download_file;
                }
                yield return new WaitForSeconds(0.0119f); //Why do I have to do this... (Doesn't really matter, this works *almost* perfectly)
                scroll_view_script.autoScroll.scrollVertically(scroll_view_script.scrollRect); //If you are going to keep, make function so you don't have to do this bs 
                email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(false);
                curr_id = curr_response.next; 
                if(curr_response.removeStall != ConversationStall.None){
                    globalGameEventScript.ConversationStallList.Remove(curr_response.removeStall);
                } 
                if(curr_response.stallValue != ConversationStall.None && curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
                if(curr_response.is_next_response){
                    isResponse = true; 
                }
                else{
                    isResponse = false; 
                }
            }
            else{
                //Subsequent Choice Sets Implementation
                curr_choices = choice_set_dict[curr_id];
                List<string> choices = new List<string>();
                foreach (Choice choice in curr_choices.choices){
                    choices.Add(choice.output);
                }
                yield return new WaitForSeconds(0.01f); //Why do I have to do this...
                scroll_view_script.showReplyButton();
                yield return new WaitUntil(() => scroll_view_script.waitOnReply(choices));
                yield return new WaitUntil(() => scroll_view_script.waitOnInput(choices));
                string current_time = updateTimeScript.customTime.ToString("h:mm tt");
                email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact("PL", player_name);  
                email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = scroll_view_script.actual_content;
                email_individ_view.GetComponent<EmailStringView>().time.text = current_time;
                yield return StartCoroutine(scroll_view_script.TypeReply(scroll_view_script.reply, scroll_view_script.actual_content, curr_choices.choices[scroll_view_script.chosenEmail].attachment));
                Destroy(scroll_view_script.reply);
                if (curr_choices.choices[scroll_view_script.chosenEmail].attachment != -1){
                    email_individ_view.GetComponent<EmailStringView>().attachment_name.GetComponent<TextMeshProUGUI>().text = fileNavigatorManager.file_dict[reply_attached_file_id].name;
                    email_individ_view.GetComponent<EmailStringView>().attachment.alpha = 1;
                    scroll_view_script.reply_expect_attachment = false;
                }
                if (curr_choices.choices[scroll_view_script.chosenEmail].trigger != Trigger.None){
                    GameEvents.TriggerEvent.Raise(curr_choices.choices[scroll_view_script.chosenEmail].trigger);
                }
                if(curr_choices.choices[scroll_view_script.chosenEmail].stallValue != ConversationStall.None){
                    globalGameEventScript.ConversationStallList.Remove(curr_choices.choices[scroll_view_script.chosenEmail].stallValue); 
                }
                this_inbox_view.GetComponent<InboxEmail>().incrementReplyCount();
                this_inbox_view.GetComponent<InboxEmail>().time.text = current_time; 
                made_choices.Add(new Dialogue_Save{id = curr_id, chosen_choice = scroll_view_script.chosenEmail, time = current_time});
                Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
                email_individ_view.GetComponent<EmailStringView>().attachment.alpha = 0;
                yield return new WaitForSeconds(0.0119f); //Why do I have to do this...(Doesn't really matter, this works *almost* perfectly)
                scroll_view_script.autoScroll.scrollVertically(scroll_view_script.scrollRect);
                curr_id = curr_choices.choices[scroll_view_script.chosenEmail].next;
                isResponse = true; 
            }
        } 
        saveFileManager.unsaved_convos.Add(new ConvoSave{file_name = file_name, convo_id = convo_id, responses = made_responses.ToArray(), choices = made_choices.ToArray()});
        GameEvents.ConversationComplete.Raise(convo_id); 
        yield break; 
    }

    IEnumerator openLoadingAnimation(){
        loadingScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        loadingAnimation.gameObject.SetActive(true);
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
        loadingScreen.gameObject.SetActive(false);
        loadingAnimation.gameObject.SetActive(false);
    }

    public void close(){
        if (currTaskBarIcon != null){
            Destroy(currTaskBarIcon); 
        }
        currently_open = false; 
        minimize();
    }

    void minimize(){
        Utility.windowMinimize(Email_CanvasGroup, windowResizerScript);
    }

    void reopen(){
        Utility.windowReopen(Email_CentralObject, Email_CanvasGroup, windowResizerScript);
    }

    void openApp(){
        StartCoroutine(delayedOpen());
    }

    IEnumerator delayedOpen(){
        yield return new WaitForSeconds(0.7f);
        open();
    }

    void open(){
        Utility.windowOpen(Email_CentralObject, Email_CanvasGroup, windowResizerScript, currently_open, new Vector2(0f, 40f));
        if(!currently_open){
            StartCoroutine(openLoadingAnimation());
            currTaskBarIcon = Instantiate(Email_TaskBarIcon, taskbar_view);
            currTaskBarIcon.GetComponent<Button>().onClick.AddListener(openOrClose);
            currently_open = true;
        }
        reopen();
    }

    void openOrClose(){
        Utility.tbIconOpenOrClose(Email_CentralObject, Email_CanvasGroup, windowResizerScript);
    }

    void showSpamSign(){
        spamView.GetComponent<CanvasGroup>().alpha = 1; 
        spamView.GetComponent<CanvasGroup>().interactable = true; 
        spamView.GetComponent<CanvasGroup>().blocksRaycasts = true; 
    }

    void hideSpamSign(){
        spamView.GetComponent<CanvasGroup>().alpha = 0; 
        spamView.GetComponent<CanvasGroup>().interactable = false;
        spamView.GetComponent<CanvasGroup>().blocksRaycasts = false; 
    }

    void setNotification(string header, string subject_peak, string content_peak, int convo_id, bool is_reply){
        notificationPrefab.GetComponent<EmailNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = header; //removed notif header
        if (is_reply){
            notificationPrefab.GetComponent<EmailNotificationScript>().subject_peak.GetComponent<TextMeshProUGUI>().text = "Subject: Re: " + subject_peak; 
        }
        else{
            notificationPrefab.GetComponent<EmailNotificationScript>().subject_peak.GetComponent<TextMeshProUGUI>().text = "Subject: " + subject_peak; 
        }
        notificationPrefab.GetComponent<EmailNotificationScript>().content_peak.GetComponent<TextMeshProUGUI>().text = content_peak; 
        notificationPrefab.GetComponent<EmailNotificationScript>().notifContainer = notifContainer; 
        notificationPrefab.GetComponent<EmailNotificationScript>().convo_id = convo_id; 
    }

    IEnumerator adjustIndividEmailSize(){
        yield return new WaitForSeconds(0.01f);
        RectTransform backdropSize = email_individ_view.GetComponent<RectTransform>();
        RectTransform contentSize = email_individ_view.GetComponent<EmailStringView>().content.GetComponent<RectTransform>();
        backdropSize.sizeDelta = new Vector2(backdropSize.sizeDelta.x, 50+contentSize.sizeDelta.y);
    }

    void openToNotif(int convo_id){
        if(currently_open){
            reopen();
        }
        else{
            openApp(); 
        }
    }

    //SAVE IMPLEMENTATION:

    public void loadConversation(ConvoSave convo){
        Destroy(empty_sign);
        int convo_id = convo.convo_id;
        TextAsset email_dialogue_json = Resources.Load<TextAsset>(convo.file_name);
        conversation_response_dict[convo_id] = new Dictionary<int, Response>(); 
        conversation_choices_dict[convo_id] = new Dictionary<int, Choice_Set>();
        ParseDialogue(email_dialogue_json, convo_id);

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
        ///

        Response curr_response = null; 
        Choice_Set curr_choices = null;
        int curr_id = 0;
        bool isResponse = false;  
        First_Email first_email = first_email_dict[convo_id]; 
        first_email.output = first_email.output.Replace("${name}", player_name); //NEW SYSTEM
        if (first_email.dialogueTriggerPairCollection != null){
            for (int i = 0; i < first_email.dialogueTriggerPairCollection.Length; i++){
                for (int j = 0; j < first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs.Length; j++){
                    if (globalGameEventScript.TriggerList.Contains(first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].trigger)){
                        int triggerPairIndex = i + 1; 
                        first_email.output = first_email.output.Replace((triggerPairIndex + "{trigger}"), (first_email.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].dialogue.Replace("${name}", player_name)));
                        break;
                    }
                }
            }
        }
        InboxEmail inboxEmailScript = inbox_mail.GetComponent<InboxEmail>();
        inboxEmailScript.subject.GetComponent<TextMeshProUGUI>().text = first_email.subject + ": ";
        inboxEmailScript.sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterName(first_email.character, player_name); 
        inboxEmailScript.emailPeak.GetComponent<TextMeshProUGUI>().text = first_email.output.Replace("\n\n", " ");
        inboxEmailScript.time.text = saved_response_dict[0].time;
        if (first_email.download != null){
            email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(true);
            email_individ_view.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().download_id = first_email.download.file_id; 
            email_individ_view.GetComponent<EmailStringView>().download_file_name.GetComponent<TextMeshProUGUI>().text = first_email.download.file_name;
            email_individ_view.GetComponent<EmailStringView>().download_file_size.GetComponent<TextMeshProUGUI>().text = first_email.download.file_size;
        }
        email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact(first_email.character, player_name);  
        email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = first_email.output;
        email_individ_view.GetComponent<EmailStringView>().time.text = saved_response_dict[0].time;
        email_scroll_view.GetComponent<EmailScrollView>().subject.text = first_email.subject; 
        email_scroll_view.GetComponent<EmailScrollView>().convo_id = convo_id; 
        GameObject this_inbox_view = Instantiate(inbox_mail, inboxEmailScroll);
        this_inbox_view.GetComponent<InboxEmail>().isPressed(); 
        GameObject this_scrollview = Instantiate(email_scroll_view, emailScrollViewContainer, false); 
        this_scrollview.GetComponent<EmailScrollView>().inbox_email = this_inbox_view;
        GameObject instantiated_first_email = Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
        email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(false);
        instantiated_first_email.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().textElement.color = Color.black;
        instantiated_first_email.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().button.interactable = false;
        curr_id = first_email.next; 
        EmailScrollView scroll_view_script = this_scrollview.GetComponent<EmailScrollView>();
        scroll_view_script.curr_reply = Utility.getCharacterContact(first_email.character, player_name); 
        if (first_email.download != null){
            scroll_view_script.downloadLinkGameObject = instantiated_first_email.GetComponent<EmailStringView>().download_file;
        }
        if(first_email.is_next_response){
            isResponse = true; 
        }
        else{
            isResponse = false; 
        }
        while(curr_id != -1){
            //Subsequent Emails Implementation
            if (isResponse){
                curr_response = response_dict[curr_id];
                curr_response.output = curr_response.output.Replace("${name}", player_name);
                if (curr_response.dialogueTriggerPairCollection != null){
                    for (int i = 0; i < curr_response.dialogueTriggerPairCollection.Length; i++){
                        for (int j = 0; j < curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs.Length; j++){
                            if (globalGameEventScript.TriggerList.Contains(curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].trigger)){
                                int triggerPairIndex = i + 1; 
                                curr_response.output = curr_response.output.Replace((triggerPairIndex + "{trigger}"), (curr_response.dialogueTriggerPairCollection[i].dialogueTriggerPairs[j].dialogue.Replace("${name}", player_name)));
                                break;
                            }
                        }
                    }
                }
                if (curr_response.download != null){
                    email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(true);
                    email_individ_view.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().download_id = curr_response.download.file_id; 
                    email_individ_view.GetComponent<EmailStringView>().download_file_name.GetComponent<TextMeshProUGUI>().text = curr_response.download.file_name;
                    email_individ_view.GetComponent<EmailStringView>().download_file_size.GetComponent<TextMeshProUGUI>().text = curr_response.download.file_size;
                    //Downloading is facilitated by GGM
                }
                this_inbox_view.GetComponent<InboxEmail>().incrementReplyCount();
                this_inbox_view.GetComponent<InboxEmail>().time.text = saved_response_dict[curr_id].time;
                this_inbox_view.GetComponent<InboxEmail>().emailPeak.GetComponent<TextMeshProUGUI>().text = curr_response.output.Replace("\n\n", " ");
                email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact(curr_response.character, player_name);  
                email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = curr_response.output;
                email_individ_view.GetComponent<EmailStringView>().time.text = saved_response_dict[curr_id].time; //USING DIALOGUE SAVE TO GET TIME 
                GameObject instantiated_email_response = Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
                instantiated_email_response.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().textElement.color = Color.black;
                instantiated_email_response.GetComponent<EmailStringView>().download_file.GetComponent<EmailDownloadFileScript>().button.interactable = false;
                if (curr_response.download != null){
                    scroll_view_script.downloadLinkGameObject = instantiated_email_response.GetComponent<EmailStringView>().download_file;
                }
                email_individ_view.GetComponent<EmailStringView>().download_file.SetActive(false);
                curr_id = curr_response.next; 
                if(curr_response.is_next_response){
                    isResponse = true; 
                }
                else{
                    isResponse = false; 
                }
            }
            else{
                //Subsequent Choice Sets Implementation
                curr_choices = choice_set_dict[curr_id];
                int chosenChoice = saved_choice_dict[curr_id].chosen_choice;
                email_individ_view.GetComponent<EmailStringView>().sender.GetComponent<TextMeshProUGUI>().text = Utility.getCharacterContact("PL", player_name);  
                email_individ_view.GetComponent<EmailStringView>().content.GetComponent<TextMeshProUGUI>().text = curr_choices.choices[chosenChoice].output;
                email_individ_view.GetComponent<EmailStringView>().time.text = saved_choice_dict[curr_id].time;

                if (curr_choices.choices[chosenChoice].attachment != -1){
                    email_individ_view.GetComponent<EmailStringView>().attachment_name.GetComponent<TextMeshProUGUI>().text = fileNavigatorManager.file_dict[3].name; // recovery log
                    email_individ_view.GetComponent<EmailStringView>().attachment.alpha = 1;
                    scroll_view_script.reply_expect_attachment = false;
                }
                this_inbox_view.GetComponent<InboxEmail>().incrementReplyCount();
                this_inbox_view.GetComponent<InboxEmail>().time.text = saved_choice_dict[curr_id].time; 
                Instantiate(email_individ_view, this_scrollview.GetComponent<EmailScrollView>().contentWindow);
                email_individ_view.GetComponent<EmailStringView>().attachment.alpha = 0;
                curr_id = curr_choices.choices[chosenChoice].next;
                isResponse = true; 
            }
        }
        GameEvents.ConversationComplete.Raise(convo_id);
            fileNavigatorManager.addFilesToSave = false; 
    }

    public void resetApp(){
        StopAllCoroutines(); 
        first_email_dict.Clear();
        conversation_response_dict.Clear();
        conversation_choices_dict.Clear();
        for (int i = inboxEmailScroll.childCount - 1; i >= 0; i--){
            GameObject child = inboxEmailScroll.GetChild(i).gameObject;
            Destroy(child); 
        }
        for (int i = 1; i < emailScrollViewContainer.childCount; i++){
            GameObject.Destroy(emailScrollViewContainer.GetChild(i).gameObject);
        }
        empty_sign = Instantiate(emptyEmailSign, inboxEmailScroll); 
        close(); 
    }
}