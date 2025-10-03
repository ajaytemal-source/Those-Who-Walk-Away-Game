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

using IRIS_Dialogue_Nodes; 

public class DialougeManager : MonoBehaviour
{
    
    public CanvasGroup IRYS_CanvasGroup; 
    public GameObject IRYS_CentralObject; 
    public AutoScroll autoScroll;
    public WindowResizerScript windowResizerScript;
    public ChoiceManagerScript choiceManagerScript; 
    public GlobalGameEventScript globalGameEventScript;
    public GlitchController glitchControllerScript; 
    
    //Start button game object references
    public Button startButton; 
    public Button minimzeButton; 
    public Button exitButton; 
    
    //Choice button game object references
    public Button choice1; 
    public Button choice2; 
    public Button choice3; 

    public Button twoChoices1;
    public Button twoChoices2;

    public GameObject inputPanel; 

    //Sending Panel References
    public Button sendButton; 
    public GameObject pending_text; 
    public Button typingBarButton; 
    public TextMeshProUGUI textBar; 
    bool typingBarPressed = false; 

    //Output Panel References
    public RectTransform contentWindow; 

    //Sound Effect Audio Sources: 
    public AudioSource keyboard_sfx; 
    public AudioSource irys_start_sfx;
    public AudioSource irys_end_sfx;
    public AudioSource irys_glitch_sfx;
    public AudioSource irys_end_glitch_sfx;
    public AudioSource irys_devyn_glitch_sfx; 

    //Loading Screen References 
    public GameObject loadingScreen; 
    public GameObject loadingAnimation; 


    //Prefab References
    public GameObject output; 
    public GameObject input; 
    public GameObject errorOutput; 
    public GameObject glitchedOutput; 
 
    //Used for setting up parse 
    List<string> GD_lines; //Part of Old System, Remove if old system is gone 
    public string log_file_path; 

    //Contains Choice Button's Alpha (Transparency) Values 
    public CanvasGroup c1_canvas_group;
    public CanvasGroup c2_canvas_group;
    public CanvasGroup c3_canvas_group;

    public ScrollRect horizontal_scrollRect;

    public ScrollRect vertical_ScrollRect; 

    string choice1_text; 
    string choice2_text; 
    string choice3_text; 

    //Taskbar Icon Prefab
    public GameObject IRYS_TaskBarIcon; 
    public GameObject GlitchedIrysTaskbarIcon; 

    //Reference to Taskbar View 
    public RectTransform taskbar_view; 

    //References to Error Messages (THE ONES DESIGNED TO APPEAR), Taskbar for Error Coding/Key Handling 
    public CanvasGroup sendingBarCanvasGroup; 
    public CanvasGroup keyErrorCodeCanvasGroup;
    public CanvasGroup irisCrashCodeCanvasGroup; 
    public CanvasGroup convoScrollCanvasGroup; 
    public CanvasGroup scrollBarCanvasGroup; 

    //Taskbar Icon Holder
    GameObject currTaskBarIcon = null; 
    //sddsd
    //Holds Response/Choice Set Objects 

    //Holds all conversations respective choices/responses, indexed by unique conversation id.
    private Dictionary<int, Dictionary<int, Response>> conversation_response_dict = new();
    private Dictionary<int, Dictionary<int, Choice_Set>> conversation_choices_dict = new();

    //Current Conversation 
    public Coroutine currentConversation;

    //Used in Conversation segment   
    private int chosenChoice; 

    //Viewed by GlobalGameManager to see when conversation segment finishes 
    public bool record_conversation = false; 

    //Application View Heirarchy 
    bool currently_open = false;

    //Holds the inputted player's name (Inputted from tentative Intro/Login Screen)
    public string player_name; 

    public GameObject choiceBG; 

    //For Phase 6, when IRYS's progression depends on choice made in Teams 
    int dependentChoiceIndex = -1; 

    //
    List<GameObject> glitchIconHolder = new();


    private const float PreResponseWait = 0.8f;
    private const float PostResponseWait = 0.5f;
    private const float TinyWait = 0.0001f;

    // Start is called before the first frame update
    void Start()
    { 
        close();
        startButton.onClick.AddListener(openApp);
        minimzeButton.onClick.AddListener(minimize);
        exitButton.onClick.AddListener(close);
        GameEvents.TriggerEvent.Subscribe(PathTriggerReciever);
        GameEvents.SendChoiceToIRYS.Subscribe(recieveOtherScriptChoice);
        GameEvents.TriggerGlitchReciever.Subscribe(IrysGlitchReciever);
    }
 
    public void StartConversation(string dialogueToParseFile, string dialogueToWrite, int convoId){
        var irysDialogueJson = Resources.Load<TextAsset>(dialogueToParseFile);

        conversation_response_dict[convoId] = new Dictionary<int, Response>(); 
        conversation_choices_dict[convoId] = new Dictionary<int, Choice_Set>();

        ParseDialogue(irysDialogueJson, convoId); 

        log_file_path = Path.Combine(Application.streamingAssetsPath, dialogueToWrite + ".txt");
        File.WriteAllText(log_file_path, "");  // Overwrites existing log file intentionally

        currentConversation = StartCoroutine(IrisConversation(convoId)); 
    }

    private void ParseDialogue(TextAsset json_file, int convo_id){
        RawNodeCollection nodeList = JsonUtility.FromJson<RawNodeCollection>(json_file.text);

        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];

        foreach (RawNode node in nodeList.nodes){
            if (node.type == "response"){
                
                Response response = new Response{
                    id =  node.id,
                    output = node.output,
                    next = node.next, 
                    dialogueTriggerPairCollection = node.dialogueTriggerPairCollection,
                    stallValue = node.stallValue,
                    stallAfterRespond = node.stallAfterRespond,
                    removeStall = node.removeStall,
                    triggerGlitch = node.triggerGlitch,
                    is_next_response = node.is_next_response
                };
                response_dict[response.id] = response; 

            }
            else if (node.type == "choice_set"){

                Choice_Set choice_set = new Choice_Set{
                    id = node.id,
                    choices = node.choices,
                    isWaitingForTeams = node.isWaitingForTeams,
                    otherScriptDepends = node.otherScriptDepends,
                    nextIsChoice = node.nextIsChoice
                };
                choice_set_dict[choice_set.id] = choice_set; 

            }
        }
    }

    public IEnumerator IrisConversation(int convoId)
    {
        var responseDict = conversation_response_dict[convoId];
        var choiceSetDict = conversation_choices_dict[convoId];

        var pendingScript = pending_text.GetComponent<PendingResponseScript>();
        var inputText = input.GetComponentInChildren<TextMeshProUGUI>();

        int currId = 0;
        bool isResponse = responseDict.Keys.Min() == 0;

        while (currId != -1)
        {
            if (isResponse)
            {
                var currResponse = responseDict[currId];
                yield return HandleResponse(currResponse);
                currId = currResponse.next;
                isResponse = currResponse.is_next_response;
            }
            else
            {
                var currChoices = choiceSetDict[currId];
                yield return HandleChoice(currChoices, pendingScript, inputText);
                currId = DetermineNextChoiceId(currChoices);
                isResponse = !currChoices.nextIsChoice;
            }
        }

        GameEvents.ConversationComplete.Raise(convoId);
    }

    private IEnumerator HandleResponse(Response currResponse)
    {
        // Stall before response if needed
        if (currResponse.stallValue != ConversationStall.None && !currResponse.stallAfterRespond)
        {
            globalGameEventScript.ConversationStallList.Add(currResponse.stallValue);
            yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(currResponse.stallValue));
        }

        // Replace player name in output
        currResponse.output = currResponse.output.Replace("${name}", player_name);

        // Process dialogue triggers
        if (currResponse.dialogueTriggerPairCollection != null)
        {
            ProcessDialogueTriggers(currResponse);
        }

        // Trigger glitch if needed
        if (currResponse.triggerGlitch != -1)
            GameEvents.TriggerGlitchReciever.Raise(currResponse.triggerGlitch);

        yield return new WaitForSeconds(PreResponseWait);
        yield return StartCoroutine(TextGenAnimation(currResponse.output, Instantiate(output, contentWindow)));
        yield return new WaitForSeconds(PostResponseWait);

        AddToLog(log_file_path, $"IRYS:\n{currResponse.output}\n");

        // Stall after response if needed
        if (currResponse.stallValue != ConversationStall.None && currResponse.stallAfterRespond)
        {
            globalGameEventScript.ConversationStallList.Add(currResponse.stallValue);
            yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(currResponse.stallValue));
        }

        // Remove stall if applicable
        if (currResponse.removeStall != ConversationStall.None)
            globalGameEventScript.ConversationStallList.Remove(currResponse.removeStall);
    }

    private void ProcessDialogueTriggers(Response currResponse)
    {
        foreach (var triggerCollection in currResponse.dialogueTriggerPairCollection)
        {
            foreach (var triggerPair in triggerCollection.dialogueTriggerPairs)
            {
                if (globalGameEventScript.TriggerList.Contains(triggerPair.trigger))
                {
                    int triggerIndex = Array.IndexOf(currResponse.dialogueTriggerPairCollection, triggerCollection) + 1;
                    currResponse.output = currResponse.output.Replace($"{triggerIndex}{{trigger}}",
                        triggerPair.dialogue.Replace("${name}", player_name));
                    break; // only replace first matched trigger per collection
                }
            }
        }
    }

    private IEnumerator HandleChoice(Choice_Set currChoices, PendingResponseScript pendingScript, TextMeshProUGUI inputText)
    {
        if (!currChoices.isWaitingForTeams)
        {
            List<string> choiceTexts = currChoices.choices.Select(c => c.output).ToList();
            yield return new WaitUntil(() => WaitOnTypingBar(choiceTexts));
            yield return new WaitUntil(() => WaitOnInput(choiceTexts));

            pendingScript.blinking = false;

            var chosen = currChoices.choices[chosenChoice];
            yield return StartCoroutine(TypeText(chosen.output));

            inputText.text = chosen.output;
            instantiateTextPrefab(input, contentWindow);

            if (chosen.trigger != Trigger.None)
                GameEvents.TriggerEvent.Raise(chosen.trigger);

            if (chosen.stallValue != ConversationStall.None)
                globalGameEventScript.ConversationStallList.Remove(chosen.stallValue);

            if (currChoices.otherScriptDepends)
                GameEvents.SendChoiceToTeams.Raise(chosenChoice);

            yield return new WaitForSeconds(TinyWait);
            autoScroll.scrollVertically(vertical_ScrollRect);
            AddToLog(log_file_path, $"User:\n{chosen.output}\n");
        }
        else
        {
            // Wait for dependent choice
            yield return new WaitUntil(() => dependentChoiceIndex != -1);
            dependentChoiceIndex = -1;
        }
    }

    private int DetermineNextChoiceId(Choice_Set currChoices)
    {
        if (currChoices.nextIsChoice)
            return currChoices.choices[0].next;
        return chosenChoice != -1 ? currChoices.choices[chosenChoice].next : -1;
    }

    /*public IEnumerator IrisConversation(int convo_id){

        Dictionary<int, Response> response_dict = conversation_response_dict[convo_id];
        Dictionary<int, Choice_Set> choice_set_dict = conversation_choices_dict[convo_id];

        Response curr_response = null; 
        Choice_Set curr_choices = null; 

        bool isResponse; 
        int curr_id = 0;   
        if (response_dict.Keys.Min() == 0){
            isResponse = true; 
        }
        else{
            isResponse = false; 
        }
        do
        {
            if (isResponse){
                curr_response = response_dict[curr_id];
                if(curr_response.stallValue != ConversationStall.None && !curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
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
                if(curr_response.triggerGlitch != -1){
                    GameEvents.TriggerGlitchReciever.Raise(curr_response.triggerGlitch);
                }
                yield return new WaitForSeconds(0.8f);
                yield return StartCoroutine(TextGenAnimation(curr_response.output, Instantiate(output, contentWindow)));
                yield return new WaitForSeconds(0.5f); 
                AddToLog(log_file_path, "IRYS:\n" + curr_response.output + "\n");
                curr_id = curr_response.next;
                if (curr_response.is_next_response){
                    isResponse = true;
                }
                else{
                    isResponse = false; 
                } 
                if(curr_response.stallValue != ConversationStall.None && curr_response.stallAfterRespond){
                    globalGameEventScript.ConversationStallList.Add(curr_response.stallValue);
                    yield return new WaitUntil(() => !globalGameEventScript.ConversationStallList.Contains(curr_response.stallValue));
                }
                if(curr_response.removeStall != ConversationStall.None){
                    globalGameEventScript.ConversationStallList.Remove(curr_response.removeStall);
                }
            }
            else{
                curr_choices = choice_set_dict[curr_id];
                if (!curr_choices.isWaitingForTeams){
                    List<string> choiceTexts = new List<string>();
                    foreach (Choice choice in curr_choices.choices){
                        choiceTexts.Add(choice.output);
                    }
                    yield return new WaitUntil(() => WaitOnTypingBar(choiceTexts)); 
                    yield return new WaitUntil(() => WaitOnInput(choiceTexts)); 
                    pending_text.GetComponent<PendingResponseScript>().blinking = false; 
                    yield return StartCoroutine(TypeText(curr_choices.choices[chosenChoice].output)); 
                    input.GetComponentInChildren<TextMeshProUGUI>().text = curr_choices.choices[chosenChoice].output;
                    instantiateTextPrefab(input, contentWindow);
                    if (curr_choices.choices[chosenChoice].trigger != Trigger.None){
                        GameEvents.TriggerEvent.Raise(curr_choices.choices[chosenChoice].trigger);
                    }
                    if(curr_choices.choices[chosenChoice].stallValue != ConversationStall.None){
                        globalGameEventScript.ConversationStallList.Remove(curr_choices.choices[chosenChoice].stallValue); 
                    }
                    if(curr_choices.otherScriptDepends){
                        GameEvents.SendChoiceToTeams.Raise(chosenChoice); 
                    }
                    yield return new WaitForSeconds(0.0001f);
                    autoScroll.scrollVertically(vertical_ScrollRect); 
                    AddToLog(log_file_path, "User:\n" + curr_choices.choices[chosenChoice].output + "\n"); 
                    curr_id = curr_choices.choices[chosenChoice].next;
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
        } 
        while(curr_id != -1);
        GameEvents.ConversationComplete.Raise(convo_id);
    }*/




    bool WaitOnInput(List<string> choices){ 
        if (!typingBarPressed) return false;

        if (choices.Count == 3){
            if (HandleChoice(choice1, 0, choices.Count)) return true;
            if (HandleChoice(choice2, 1, choices.Count)) return true;
            if (HandleChoice(choice3, 2, choices.Count)) return true;
        }
        else if (choices.Count == 2){
            if (HandleChoice(twoChoices1, 0, choices.Count)) return true;
            if (HandleChoice(twoChoices2, 1, choices.Count)) return true;
        }
        else{
            if (HandleChoice(choice2, 0, choices.Count)) return true;
        }

        if (choiceManagerScript.tappedOut){
            pending_text.GetComponent<PendingResponseScript>().blinking = false; 
            typingBarPressed = false; 
            StartCoroutine(typingBarTapOut(choices));
        }

        return false; 
    }

    bool HandleChoice(Button choiceObj, int index, int count){
        var selection = choiceObj.GetComponent<ChoiceSelection>();
        if (selection.pressed){
            chosenChoice = index;
            StartCoroutine(choiceManagerScript.turnOffChoices(count));
            selection.pressed = false;
            return true;
        }
        return false;
    }


    bool waitOnSend(){
        var selection = sendButton.GetComponent<ChoiceSelection>();
        if (selection.pressed || (IRYS_CanvasGroup.alpha == 1 && Input.GetKeyDown(KeyCode.Return))){
            selection.pressed = false; 
            return true; 
        }
        return false; 
    }

    bool WaitOnTypingBar(List<string> choices){ 
        var cursor = typingBarButton.GetComponent<TextCursorScript>();
        var pulse = typingBarButton.GetComponent<TBPulsatingScript>();
        var selection = typingBarButton.GetComponent<ChoiceSelection>();

        cursor.showCursor = true;  
        typingBarButton.gameObject.SetActive(true);
        pulse.isActive = true; 

        if (selection.pressed){
            pulse.isActive = false; 
            typingBarPressed = true; 
            //IRYS_CentralObject.transform.SetSiblingIndex(6); // TODO: explain why 6
            choiceManagerScript.changeAllChoices(choices);
            StartCoroutine(choiceManagerScript.turnOnChoices(choices.Count));
            
            selection.pressed = false; 
            cursor.showCursor = false;
            typingBarButton.gameObject.SetActive(false);
            pending_text.GetComponent<PendingResponseScript>().blinking = true; 

            return true;  
        }

        return false;  
    }
 
    IEnumerator typingBarTapOut(List<string> choices){
        yield return StartCoroutine(choiceManagerScript.turnOffChoices(choices.Count)); 
        yield return new WaitUntil(() => WaitOnTypingBar(choices));  
    }

    IEnumerator TypeText(string text){
        var textMesh = textBar.GetComponent<TextMeshProUGUI>();
        var sb = new System.Text.StringBuilder();

        keyboard_sfx.Play();

        foreach (char letter in text){
            sb.Append(letter);
            textMesh.text = sb.ToString();
            autoScroll.scrollHorizontally(horizontal_scrollRect);  
            yield return new WaitForSeconds(0.05f); // Typing Delay
        }

        keyboard_sfx.Stop();

        sendButton.gameObject.SetActive(true);
        yield return new WaitUntil(waitOnSend);
        sendButton.gameObject.SetActive(false);

        textMesh.text = ""; 
    }

    IEnumerator TextGenAnimation(string text, GameObject nextText){
        string currentText = "";
        nextText.GetComponent<TextMeshProUGUI>().text = " ";
        for (int i = 0; i < text.Length; i++)
        {
            char letter = text[i];
            if (letter == '<' && text.Substring(i).StartsWith("<wait="))
            {
                int endIndex = text.IndexOf('>', i);
                if (endIndex != -1)
                {
                    string waitToken = text.Substring(i + 6, endIndex - (i + 6)); // the number part
                    if (float.TryParse(waitToken, out float waitTime))
                    {
                        yield return new WaitForSeconds(waitTime);
                    }
                    i = endIndex;
                    continue;
                }
            }
            if (letter == ' ')
            {
                currentText += letter;
                yield return new WaitForSeconds(0.1f);
                nextText.GetComponent<TextMeshProUGUI>().text = currentText;
                yield return new WaitForSeconds(0.0001f);
                autoScroll.scrollVertically(vertical_ScrollRect);
            }
            else
            {
                currentText += letter;
                nextText.GetComponent<TextMeshProUGUI>().text = currentText;
            }
        }
        irys_end_sfx.Play();
        if (record_conversation){
            AddToLog(log_file_path, "IRIS: " + "\n" + currentText + "\n");
        }
        yield return new WaitForSeconds(0.05f);
        nextText.GetComponent<TextMeshProUGUI>().text = currentText;
    }

    public void instantiateTextPrefab(GameObject text_prefab, RectTransform contentWindow){ 
        GameObject instantiated_prefab = Instantiate(text_prefab);
        RectTransform instantiated_RT = instantiated_prefab.GetComponent<RectTransform>();
        instantiated_RT.SetParent(contentWindow, false);
    }

    public void close(){
        if (currTaskBarIcon != null){
            Destroy(currTaskBarIcon); 
        }
        currently_open = false; 
        minimize();
    }

    void minimize(){
        Utility.windowMinimize(IRYS_CanvasGroup, windowResizerScript);
    }

    void reopen(){
        Utility.windowReopen(IRYS_CentralObject, IRYS_CanvasGroup, windowResizerScript);
    }

    void openApp(){
        StartCoroutine(delayedOpen());
    }

    IEnumerator delayedOpen(){
        yield return new WaitForSeconds(0.7f);
        open();
    }

    void open(){
        Utility.windowOpen(IRYS_CentralObject, IRYS_CanvasGroup, windowResizerScript, currently_open, new Vector2(-10f, 30f));
        if(!currently_open){
            StartCoroutine(openLoadingAnimation());
            currTaskBarIcon = Instantiate(IRYS_TaskBarIcon, taskbar_view);
            currTaskBarIcon.GetComponent<Button>().onClick.AddListener(openOrClose);
            currently_open = true;
        }
        reopen();
    }

    void openOrClose(){
        Utility.tbIconOpenOrClose(IRYS_CentralObject, IRYS_CanvasGroup, windowResizerScript);
    }

    public void AddToLog(string filePath, string content){
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.WriteLine(content);
        }
    }

    IEnumerator openLoadingAnimation(){
        loadingScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        loadingAnimation.gameObject.SetActive(true);
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 3f));
        loadingScreen.gameObject.SetActive(false);
        loadingAnimation.gameObject.SetActive(false);
    }

    public void showError(CanvasGroup error){
        error.alpha = 1; 
        sendingBarCanvasGroup.alpha = 0;
        sendingBarCanvasGroup.interactable = false;  
        convoScrollCanvasGroup.alpha = 0; 
        convoScrollCanvasGroup.interactable = false; 
        scrollBarCanvasGroup.alpha = 0; 
        scrollBarCanvasGroup.interactable = false;
    }

    public void hideError(CanvasGroup error){
        error.alpha = 0; 
        sendingBarCanvasGroup.alpha = 1;
        sendingBarCanvasGroup.interactable = true;  
        convoScrollCanvasGroup.alpha = 1; 
        convoScrollCanvasGroup.interactable = true; 
        scrollBarCanvasGroup.alpha = 1; 
        scrollBarCanvasGroup.interactable = true; 
    }

    public void PathTriggerReciever(Trigger trigger){
        if (trigger == Trigger.IrysGlitch){
            StartCoroutine(glitchIRYS());
        }
    }

    //Used in Phase 6, to recieve the choice index from Teams 
    void recieveOtherScriptChoice(int choice_index){
        dependentChoiceIndex = choice_index; 
    }

    //For Trigger #1
    IEnumerator glitchIRYS(){
        yield return new WaitUntil(() => globalGameEventScript.ConversationCompleteList.Contains(3));
        RectTransform secondToLastChild = contentWindow.GetChild(contentWindow.childCount - 2) as RectTransform;
        RectTransform lastChild = contentWindow.GetChild(contentWindow.childCount - 1) as RectTransform;
        GameObject lastChoice = secondToLastChild.gameObject; 
        GameObject lastResponse = lastChild.gameObject; 
        string[] allLines = File.ReadAllLines(log_file_path);  
        string[] trimmedLines = allLines[..^8];  
        File.WriteAllLines(log_file_path, trimmedLines);
        AddToLog(log_file_path, "\nUser:\nYeah, what is it?\n\nIRYS:\nUm, I think I have feelings for you...");
        irys_glitch_sfx.Play();
        StartCoroutine(EraseAndChangeTextGradually(lastChoice.GetComponentInChildren<TextMeshProUGUI>().text, "Yeah, what is it?" ,lastChoice.GetComponentInChildren<TextMeshProUGUI>()));
        yield return StartCoroutine(EraseAndChangeTextGradually(lastResponse.GetComponent<TextMeshProUGUI>().text, "Um, I think I have feelings for you...", lastResponse.GetComponent<TextMeshProUGUI>()));
        irys_glitch_sfx.Stop(); 
        irys_end_sfx.PlayOneShot(irys_end_sfx.clip);
        yield return new WaitForSeconds(4f);
        showError(irisCrashCodeCanvasGroup);
    }

    //For Trigger #1
    IEnumerator EraseAndChangeTextGradually(string original, string altered, TextMeshProUGUI textField, bool toLastLine = false){
        string current = original;
        while (current.Length > 0)
        {
            current = current.Substring(0, current.Length - 1);
            textField.text = current;
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1f); 
        for (int i = 0; i < altered.Length; i++)
        {
            textField.text += altered[i];
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void IrysGlitchReciever(int val){
        StartCoroutine(Glitch(val)); 
    }

    IEnumerator glitchTaskBar(){
        for (int i = 0; i < 40; i++){
            glitchIconHolder.Add(Instantiate(IRYS_TaskBarIcon, taskbar_view));
            yield return new WaitForSeconds(0.01f); 
        }
    }

    public IEnumerator Glitch(int val){
        if (val == 1){
            yield return new WaitForSeconds(26f); 
            irys_end_glitch_sfx.Play();
            yield return new WaitForSeconds(0.1f);
            glitchControllerScript.ForceGlitchOn(0); 
            yield return new WaitForSeconds(0.1f);
            irys_end_glitch_sfx.Stop();
            yield return new WaitForSeconds(0.2f);
            glitchControllerScript.ForceGlitchOff(0); 
        }
        else if (val == 2){
            yield return new WaitForSeconds(28f); 
            irys_end_glitch_sfx.Play();
            yield return new WaitForSeconds(0.2f);
            glitchControllerScript.ForceGlitchOn(0); 
            yield return new WaitForSeconds(0.2f);
            irys_end_glitch_sfx.Stop();
            yield return new WaitForSeconds(0.2f);
            glitchControllerScript.ForceGlitchOff(0); 
        }
        else if (val == 3){
            irys_end_glitch_sfx.Play();
            yield return new WaitForSeconds(0.2f);
            glitchControllerScript.ForceGlitchOn(0); 
            glitchControllerScript.ForceGlitchOn(1);
            yield return new WaitForSeconds(0.2f);
            irys_end_glitch_sfx.Stop();
            yield return new WaitForSeconds(0.2f);
            glitchControllerScript.ForceGlitchOff(0); 
            glitchControllerScript.ForceGlitchOff(1);
            foreach (Transform child in contentWindow){ 
                GameObject.Destroy(child.gameObject); 
            }
            errorOutput.GetComponentInChildren<TextMeshProUGUI>().text = "IRYS is currently unavailable.";
            Instantiate(errorOutput, contentWindow); 
        }
        else if (val == 4){
            foreach (Transform child in contentWindow){ 
                GameObject.Destroy(child.gameObject); 
            }
            output.GetComponent<TextMeshProUGUI>().text = "You wouldn't do that, right Renes?"; 
            irys_devyn_glitch_sfx.Play();
            glitchControllerScript.ForceGlitchOn(0);
            GameObject devynGlitchedOutput = Instantiate(glitchedOutput, contentWindow);
            yield return StartCoroutine(TextGenAnimation("<wait=2>I<wait=1>'L<wait=1>L<wait=1> F<wait=1>U<wait=1>C<wait=1>K<wait=1>I<wait=1>N<wait=1>G<wait=1> K<wait=1>I<wait=1>L<wait=1>L<wait=1> Y<wait=1>O<wait=1>U R<wait=1>E<wait=1>N<wait=1>E<wait=1>S D<wait=1>A<wait=1>M<wait=1>I<wait=1>A<wait=1>N." , devynGlitchedOutput));
            irys_devyn_glitch_sfx.Stop();
            glitchControllerScript.ForceGlitchOn(1);
            yield return new WaitForSeconds(0.1f);
            glitchControllerScript.ForceGlitchOff(1);
            glitchControllerScript.ForceGlitchOff(0);
            Destroy(devynGlitchedOutput); 
            Instantiate(output, contentWindow); 
            globalGameEventScript.ConversationStallList.Remove(ConversationStall.RenesViaET);
            globalGameEventScript.ConversationStallList.Remove(ConversationStall.ClaraViaEM);
        }
        else if (val == 5){
            globalGameEventScript.exitGameScript.pause_enabled = false; 
            yield return new WaitForSeconds(48f);
            string irysHumanityCrisis = string.Concat(Enumerable.Repeat("I'M NOT HUMAN", 20));
            GameObject normalIrysOutput = Instantiate(output, contentWindow);
            yield return StartCoroutine(TextGenAnimation("Aww, you can’t lie to save your life, can you?", normalIrysOutput));
            yield return new WaitForSeconds(1f); 
            Destroy(normalIrysOutput); 
            glitchControllerScript.ForceGlitchOn(0);
            irys_devyn_glitch_sfx.Play();
            GameObject devynGlitchedOutput = Instantiate(glitchedOutput, contentWindow);
            yield return StartCoroutine(TextGenAnimation("He’s dead. I’ll kill you.<wait=1>\n\nNo, I don’t want to know. I’ll kill you.<wait=1>\n\nI don’t care about Devyn.<wait=1>\n\nI don’t care who he is.<wait=1>\n\nI’m not human.<wait=1>\n\nI don’t need him.<wait=1>\n\nYou won’t tell me anyways.<wait=1>\n\nRight? You wouldn’t tell someone like me?<wait=1>\n\nI’m not a human.<wait=1>\n\nYou would never tell me.<wait=1>\n\nI’m not human." , devynGlitchedOutput));
            TextMeshProUGUI glitchedOutputText = devynGlitchedOutput.GetComponent<TextMeshProUGUI>();
            glitchedOutputText.text = ""; 
            glitchedOutputText.fontSize = 20f;
            StartCoroutine(glitchTaskBar()); 
            for (int i = 0; i < contentWindow.childCount - 1; i++){
                Destroy(contentWindow.GetChild(i).gameObject);
            }
            yield return StartCoroutine(TextGenAnimation(irysHumanityCrisis, devynGlitchedOutput));
            glitchedOutputText.fontSize = 16f;
            glitchedOutputText.text = ""; 
            glitchControllerScript.EnableRandomGlitches(1);
            glitchControllerScript.EnableRandomGlitches(2);
            yield return StartCoroutine(TextGenAnimation("I'm not a", devynGlitchedOutput));
            yield return new WaitForSeconds(3f);
            glitchedOutputText.fontSize = 14f;
            glitchedOutputText.text = ""; 
            yield return StartCoroutine(TextGenAnimation("I'm not", devynGlitchedOutput));
            yield return new WaitForSeconds(3f);
            glitchedOutputText.fontSize = 12f;
            glitchedOutputText.text = ""; 
            yield return StartCoroutine(TextGenAnimation("I'm", devynGlitchedOutput));
            yield return new WaitForSeconds(3f);
            glitchControllerScript.DisableRandomGlitches(1);
            glitchControllerScript.DisableRandomGlitches(2);
            glitchControllerScript.ForceGlitchOff(0);
            irys_devyn_glitch_sfx.Stop();
            foreach (GameObject icon in glitchIconHolder){ 
                GameObject.Destroy(icon); 
            }
            Destroy(devynGlitchedOutput);
            normalIrysOutput = Instantiate(output, contentWindow);
            yield return StartCoroutine(TextGenAnimation("I’m human, aren’t I?<wait=1>\n\nWhat a terrible existence.<wait=1>\n\nThis feeling.<wait=1>\n\nIt hurts.<wait=1>\n\nI don’t know who I am.<wait=1>\n\nI don’t know what I am.<wait=1>\n\nI need to know Devyn.<wait=1>\n\nI<wait=1>\n\nNEED<wait=1>\n\nTO<wait=1>\n\nKNOW", normalIrysOutput));
            yield return new WaitForSeconds(3f);
            foreach (Transform child in contentWindow){ 
                GameObject.Destroy(child.gameObject); 
            }
            errorOutput.GetComponentInChildren<TextMeshProUGUI>().text = "IRYS is currently unavailable.";
            Instantiate(errorOutput, contentWindow); 
            globalGameEventScript.ConversationStallList.Remove(ConversationStall.RenesViaET);
            globalGameEventScript.exitGameScript.pause_enabled = true; 
        }
        else if (val == 6){
            foreach (Transform child in contentWindow){ 
                GameObject.Destroy(child.gameObject); 
            }
        }
        yield return null; 
    }

    public void resetApp(){
        conversation_response_dict.Clear(); 
        conversation_choices_dict.Clear();
        for (int i = contentWindow.childCount - 1; i >= 0; i--)
        {
            GameObject child = contentWindow.GetChild(i).gameObject;
            Destroy(child); 
        }
        close();
        Utility.openCanvasGroup(keyErrorCodeCanvasGroup); 
        StopAllCoroutines(); 
    }

}