using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO; 
using TMPro; 
using System.Linq;
using System;

using SaveConversation;

public class GlobalGameEventScript : MonoBehaviour
{

    public Button startButton;
    public Button exitButton;
    public CanvasGroup titleScreen;  
    public CanvasGroup loginScreen; 
    public CanvasGroup loginNameInput;
    public CanvasGroup loginHeaderText; 
    public GameObject loadingAnimation; 
    public GameObject welcomeAnimation; 
    public GameObject signInAnimation;
    public TextMeshProUGUI actualLoginHeaderText; 
    public TMP_InputField nameInput; 
    public CanvasGroup gameOver;

    //Holds the inputted player's name (Inputted from tentative Intro/Login Screen)
    public string player_name; 
    public DateTime currentTime; 

    public EmailDialougeManager emailDialogueManager;
    public DialougeManager dialougeManager; 
    public FileNavigatorManager fileNavigatorManager;   
    public TeamsDialogueManager teamsDialogueManager; 
    public TerminalManager terminalManager; 
    public UpdateTimeScript updateTimeScript; 
    public GlitchController glitchControllerScript;
    public SaveFileManager saveFileManager; 
    public TitleScreenScript titleScreenScript; 
    public ExitGameScript exitGameScript; 

    public GameObject ongoingScriptContainer; 

    public GameObject referenceToEmail; 
    public TextMeshProUGUI curr_date; 
    public string curr_dl_file_type; 
    public string curr_dl_file_name; 
    public string curr_dl_file_size; 

    public int waitingForFileDownload = -1;  //See if we need anymore <-- If a unique file is in a unique directory, we can fix this...

    //Audio Sounds:
    public AudioSource keyboard_sfx; 
    public AudioSource alarm_sfx; 
    public AudioSource alarm_tapoff_sfx;
    public AudioSource out_of_bed_sfx;  
    public AudioSource walking_sfx; 
    public AudioSource put_down_laptop_sfx; 
    public AudioSource turn_laptop_on_sfx; 
    public AudioSource ETHR_startup_sfx; 
    public AudioSource restart_glitch_sfx; 
    public AudioSource error_sound_sfx; 
    public AudioSource BTAC_endingTrack; 

    //FOR NEW LOGIN SEQUENCE: 
    Vector2 originalEyePos;
    Vector2 originalETHRPos;
    public GameObject Eye_Logo;
    public GameObject ETHR_Logo; 
    public GameObject SignInPrompt;
    public GameObject NamePrompt; 
    public GameObject WelcomeSign; 
    public TMP_InputField IDinputField;
    public TMP_InputField NameInputField;
    public Button SignInButton; 
    public CanvasGroup LoginErrorSign; 
    
    bool animate_logo = false; 
    bool fade_out_logo = false; 
    bool login_correct = false; 
    bool name_entered = false; 

    public List<int> FileDownloadedList = new List<int>();
    public List<int> FileInSpecificFolderListTrigger = new List<int>();
    public List<int> ConversationCompleteList = new List<int>();
    public List<Trigger> TriggerList = new List<Trigger>();
    public HashSet<ConversationStall> ConversationStallList = new HashSet<ConversationStall>(); 

    //ENDING SEQUENCE 
    public CanvasGroup BSOD; 
    public GameObject RestartSign; 
    public GameObject SigningInSign; 
    public GameObject endLoadingSign; 
    public CanvasGroup EndingSequenceScreen; 
    public TextMeshProUGUI IrysFinalMessage; 

    //Credits 
    public CanvasGroup creditsBG; 
    public CanvasGroup titleCard; 
    public CanvasGroup thanksForPlaying; 

    public GameObject IrysDesktopButton; 
    public GameObject EmailDesktopButton; 
    public GameObject FileDesktopButton; 
    public GameObject TeamsDesktopButton; 

    //Checks For Ending Path 
    bool ending1 = false; 
    bool ending2 = false; 

    //GAME SAVE STATE
    public int current_save = 0; //IMPORTANT 

    //CURRENT GAME COROUTINE
    private Coroutine currentGame;
     
    void Start(){
        Resolution native = Screen.resolutions[Screen.resolutions.Length - 1];
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Screen.SetResolution(native.width, native.height, true);
        currentTime = updateTimeScript.customTime; 
        originalEyePos = Eye_Logo.GetComponent<RectTransform>().anchoredPosition;
        originalETHRPos = ETHR_Logo.GetComponent<RectTransform>().anchoredPosition;
        fileNavigatorManager = GetComponent<FileNavigatorManager>();
        //THE GAME EVENT TRIGGER, Triggered by EmailDownloadFileScript when the download button in the EmailStringView is pressed, sends ID of file downloaded 
        GameEvents.FileDownloaded.Subscribe(AddToFileDownloadedList);
        //THE GAME EVENT TRIGGER, Triggered by Choices in DM and EDM, triggers provide trigger values to represent each event, GGM handles trigger values. 
        GameEvents.TriggerEvent.Subscribe(AddToGeneralTriggerList);
        //THE GAME EVENT TRIGGER, Triggered when a folder is given a FileTrigger, that when a file with a specific id is added to it, sends signal to function. Later, use to wait for downloads/files to be moved to specific folders. GGM handles trigger values. 
        GameEvents.FileInSpecificFolder.Subscribe(AddToFileinSpecificFolderList); 
        //THE GAME EVENT TRIGGER, Triggered when a conversation, (IRIS, Email, ...) is completed, sends the conversation id.
        GameEvents.ConversationComplete.Subscribe(AddToConversationCompleteList);
        //NameInputField.onValueChanged.AddListener(_ => playKeyboardSfx());
        TriggerList.Add(Trigger.None);
        IDinputField.onSubmit.AddListener(_ => LoginPressed());
        SignInButton.onClick.AddListener(LoginPressed);
        setDate("11/20/25", "November 20th, 2025");
        presetGameState(); 
        StartCoroutine(titleScreenScript.loadTitleScreen());
    }

    public void startGame(){
        if (current_save == 0){
            emptyChatLogs();
            StartCoroutine(loginSequence());
        }
        else{
            StartCoroutine(logBackInSequence());
        }  
    }

    IEnumerator continueGame(){
        exitGameScript.pause_enabled = true; 
        if (current_save > 1 && current_save < 9){
            dialougeManager.hideError(dialougeManager.keyErrorCodeCanvasGroup);
            dialougeManager.showError(dialougeManager.irisCrashCodeCanvasGroup);
        }
        if(current_save > 2){
            teamsDialogueManager.startButton.gameObject.SetActive(true);
        }
        if(current_save > 4 && current_save < 13){
            Utility.openCanvasGroup(teamsDialogueManager.IRYSContactButton.GetComponent<CanvasGroup>()); 
        } 
        if(current_save > 8){
            fileNavigatorManager.unlockAdminFolder(); 
            terminalManager.currentlyExpectedInput = 6; //???
        }
        if (current_save > 11){
            dialougeManager.hideError(dialougeManager.keyErrorCodeCanvasGroup);
        }
        if(current_save == 14){
            fileNavigatorManager.downloadScreenshot(false); //add without notif 
        }
        //REAL GAME 
        if (current_save == 0){
            fileNavigatorManager.AddFileById(1,0);
            fileNavigatorManager.AddFileById(8,5);
            fileNavigatorManager.AddFileById(9,5);
            yield return new WaitForSeconds(20f);   
            emailDialogueManager.startConversation("EMAIL_D1", 1); //Two parameters, second is conversation ID. 
            yield return new WaitForSeconds(90f); 
            emailDialogueManager.startConversation("EMAIL_D2", 2);
            yield return new WaitUntil(() => FileDownloadedList.Contains(2)); //CHECKS IF PLAYER DOWNLOADED TOKEN FROM CLARA 
            yield return new WaitForSeconds(1f);
            fileNavigatorManager.downloadFileById(2,1); 
            yield return new WaitUntil(() => FileInSpecificFolderListTrigger.Contains(1));
            yield return new WaitForSeconds(1f);
            current_save = 1; 
            saveFileManager.WriteSaveFile(); 
        }//SAVE 1 (PLAYER ADDS THE TOKEN FILE TO IRYS TERMINAL FOLDER)
        if (current_save == 1){
            dialougeManager.hideError(dialougeManager.keyErrorCodeCanvasGroup);
            dialougeManager.startConversation("IRIS_D3", "LOG_1", 3); //NEW SYSTEM  
            yield return new WaitUntil(() => ConversationCompleteList.Contains(3));
            yield return new WaitForSeconds(6f);
            //dialougeManager.showError(dialougeManager.irisCrashCodeCanvasGroup); 
            fileNavigatorManager.AddFileById(3,4);
            fileNavigatorManager.AddFileById(4,4);
            yield return new WaitForSeconds(5f); 
            current_save = 2; 
            saveFileManager.WriteSaveFile();
            yield return new WaitForSeconds(15f); 
        }//SAVE 2 (IRYS CRASHES) 
        if (current_save == 2){
            yield return new WaitForSeconds(5f);
            emailDialogueManager.startConversation("EMAIL_D4", 4);
            emailDialogueManager.startConversation("EMAIL_D5", 5);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(4));  
            yield return new WaitUntil(() => ConversationCompleteList.Contains(5));  
            yield return new WaitUntil(() => FileDownloadedList.Contains(5));
            yield return new WaitForSeconds(5f);
            fileNavigatorManager.DownloadIRYSTool(); 
            yield return new WaitUntil(() => fileNavigatorManager.firstIrysToolOpened);
            yield return new WaitForSeconds(2f);
            current_save = 3; 
            saveFileManager.WriteSaveFile();
        }//SAVE 3 (PLAYER HAS OPENED THE IRYS TOOL)
        if (current_save == 3){
            teamsDialogueManager.startButton.gameObject.SetActive(true);
            teamsDialogueManager.startConversation("TEAMS_D6", 6, Thread.Eric);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(6));
            yield return new WaitForSeconds(5f); 
            emailDialogueManager.startConversation("EMAIL_D7", 7);
            yield return new WaitUntil(() => FileDownloadedList.Contains(6));
            yield return new WaitForSeconds(7f);
            fileNavigatorManager.DownloadIrysETProfile();
            yield return new WaitUntil(() => FileInSpecificFolderListTrigger.Contains(2)); 
            current_save = 4; 
            saveFileManager.WriteSaveFile();
        }//SAVE 4 (PLAYER HAS MOVED IRYS TEAMS PROFILE TO IRYS TERMINAL)
        if (current_save == 4){
            yield return new WaitForSeconds(4f); 
            Utility.openCanvasGroup(teamsDialogueManager.IRYSContactButton.GetComponent<CanvasGroup>()); //KEEP GOING FORWARD INDEFINITELY 
            teamsDialogueManager.startConversation("TEAMS_D8", 8, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(8));
            yield return new WaitForSeconds(2f);
            current_save = 5; 
            saveFileManager.WriteSaveFile();
        }//SAVE 5 (PLAYER FINISHES TALKING TO IRYS ABOUT GIVING HER HELP)
        if (current_save == 5){ 
            yield return new WaitForSeconds(7f);
            emailDialogueManager.startConversation("EMAIL_D9", 9);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(9));
            yield return new WaitForSeconds(2f);
            current_save = 6; 
            saveFileManager.WriteSaveFile();
        }//SAVE 6 (PLAYER FINISHES CONVERSATION WITH CLARA ABOUT CHAT WITH IRYS)
        if (current_save == 6){
            yield return new WaitForSeconds(7f);
            teamsDialogueManager.startConversation("TEAMS_D10", 10, Thread.IRYS);
            yield return new WaitForSeconds(8f);
            teamsDialogueManager.sendEricScreenshot(); 
            yield return new WaitUntil(() => ConversationCompleteList.Contains(10));
            yield return new WaitForSeconds(2f);
            current_save = 7; 
            saveFileManager.WriteSaveFile();
        }//SAVE 7 (PLAYER FINISHES TALKING TO IRYS ABOUT DEVYN SCREENSHOT)
        if (current_save == 7){
            yield return new WaitForSeconds(7f);
            emailDialogueManager.startConversation("EMAIL_D11", 11);
            yield return new WaitUntil(() => ConversationStallList.Contains(ConversationStall.ClaraViaEM));
            teamsDialogueManager.startConversation("TEAMS_D12", 12, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(11));
            yield return new WaitForSeconds(9f); 
            teamsDialogueManager.startConversation("TEAMS_D13", 13, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(13));
            yield return new WaitForSeconds(2f);
            current_save = 8; 
            saveFileManager.WriteSaveFile();
        }//SAVE 8 (PLAYER FINISHES INVESTINGATING CLARA, FINISHES GIVING ANSWER TO IRYS)
        if (current_save == 8){
            yield return new WaitForSeconds(4f);
            teamsDialogueManager.startConversation("TEAMS_D14", 14, Thread.Eric);
            yield return new WaitUntil(() => TriggerList.Contains(Trigger.SendEricTerminalEmail));
            yield return new WaitForSeconds(2f);
            emailDialogueManager.startConversation("EMAIL_D15", 15);
            yield return new WaitUntil(() => TriggerList.Any(x => (int)x >= 25 && (int)x <= 29)); //All dialogue choices for Eric 
            dialougeManager.hideError(dialougeManager.irisCrashCodeCanvasGroup);
            dialougeManager.showError(dialougeManager.keyErrorCodeCanvasGroup);
            yield return new WaitForSeconds(2f); 
            emailDialogueManager.startConversation("EMAIL_D16", 16);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(14));
            yield return new WaitForSeconds(2f);
            current_save = 9; 
            saveFileManager.WriteSaveFile();
        }//SAVE 9 (PLAYER FINISHES CONVERSATION WITH ERIC ON TEAMS/TERMINAL)
        if (current_save == 9){
            dialougeManager.showError(dialougeManager.keyErrorCodeCanvasGroup);
            yield return new WaitForSeconds(5f); 
            teamsDialogueManager.startConversation("TEAMS_D17", 17, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(17));
            yield return new WaitForSeconds(2f);
            current_save = 10; 
            saveFileManager.WriteSaveFile();
        }//SAVE 10 (PLAYER FINSIHES SMALL TALK WITH IRYS (MAYBE MERGE WITH THE FOLLOWING SAVE?))
        if (current_save == 10){
            dialougeManager.showError(dialougeManager.keyErrorCodeCanvasGroup);
            yield return new WaitForSeconds(5f);
            emailDialogueManager.startConversation("EMAIL_D18", 18);
            yield return new WaitUntil(() => FileDownloadedList.Contains(7));
            yield return new WaitForSeconds(10f); 
            fileNavigatorManager.downloadFileById(7,1);  
            yield return new WaitUntil(() => FileInSpecificFolderListTrigger.Contains(3)); 
            dialougeManager.hideError(dialougeManager.keyErrorCodeCanvasGroup);
            dialougeManager.startConversation("IRIS_D19", "LOG_2", 19);
            teamsDialogueManager.startConversation("TEAMS_D20", 20, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(18));
            StopCoroutine(dialougeManager.currentConversation); 
            yield return new WaitForSeconds(2f);
            current_save = 11; 
            saveFileManager.WriteSaveFile();
        }//SAVE 11
        if (current_save == 11){
            yield return new WaitForSeconds(5f); 
            teamsDialogueManager.startConversation("TEAMS_D21", 21, Thread.IRYS);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(21));
            yield return new WaitForSeconds(2f);
            current_save = 12; 
            saveFileManager.WriteSaveFile();
        }//SAVE 12
        if (current_save == 12){
            yield return new WaitForSeconds(3f); 
            StartCoroutine(teamsDialogueManager.sendUserErrorMessageToIRYS()); 
            yield return new WaitForSeconds(5f); 
            teamsDialogueManager.updateEricIRYSChatLogTime(); 
            teamsDialogueManager.startConversation("TEAMS_D22", 22, Thread.Eric); 
            yield return new WaitUntil(() => TriggerList.Any(x => (int)x >= 38 && (int)x <= 40)); 
            yield return new WaitForSeconds(14f); 
            StartCoroutine(teamsDialogueManager.sendAddedClaraMessage()); 
            yield return new WaitUntil(() => ConversationCompleteList.Contains(22));
            yield return new WaitForSeconds(2f);
            current_save = 13; 
            saveFileManager.WriteSaveFile();
        }//SAVE 13
        if (current_save == 13){
            yield return new WaitForSeconds(5f); 
            emailDialogueManager.startConversation("EMAIL_D23", 23);
            StartCoroutine(waitForFileDownload(11)); 
            StartCoroutine(waitForFileDownload(10)); 
            yield return new WaitForSeconds(140f); 
            teamsDialogueManager.startConversation("TEAMS_D24", 24, Thread.Eric);
            yield return new WaitUntil(() => TriggerList.Any(x => (int)x >= 46 && (int)x <= 48));
            StartCoroutine(waitForScreenshotDownload(12));
            ConversationStallList.Remove(ConversationStall.EricViaEM);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(24)); 
            yield return new WaitForSeconds(2f);
            current_save = 14; 
            saveFileManager.WriteSaveFile();
        }//SAVE 14
        if (current_save == 14){
            yield return new WaitForSeconds(3f);  
            StartCoroutine(teamsDialogueManager.removeEricAndClara());  
            yield return StartCoroutine(teamsDialogueManager.changeToRenesThread());
            teamsDialogueManager.startConversation("TEAMS_D25", 25, Thread.Eric);  
            dialougeManager.startConversation("IRIS_D26", "LOG_2", 26);  
            yield return new WaitUntil(() => TriggerList.Contains(Trigger.QueueClaraEricEmail));  
            yield return new WaitForSeconds(2f); 
            emailDialogueManager.startConversation("EMAIL_D27", 27);
            emailDialogueManager.startConversation("EMAIL_D28", 28);
            yield return new WaitUntil(() => ConversationCompleteList.Contains(28));
            terminalManager.currentlyExpectedInput = 7;
            StartCoroutine(CheckForEnding1());
            StartCoroutine(CheckForEnding2());
            yield return StartCoroutine(CheckForEndingPath());
            yield return StartCoroutine(glitchAndRestart());
            current_save = 0; // Reset After Finished Playing (Maybe activate "About" Section )
        }
        yield return null; 
    }

    void emptyChatLogs(){ //MOVE TO PERSISTANT DATA PATH 
        File.WriteAllText(teamsDialogueManager.playerChatlog, "");
        File.WriteAllText(teamsDialogueManager.ericIRYSChatLog, "");
    }

    public void presetGameState(){
        if(currentGame != null){
            StopCoroutine(currentGame);
        }
        FileDownloadedList.Clear();
        FileInSpecificFolderListTrigger.Clear();
        ConversationCompleteList.Clear();
        TriggerList.Clear();
        ConversationStallList.Clear();
        Utility.openCanvasGroup(titleScreen);
        Utility.openCanvasGroup(loginScreen);
        dialougeManager.resetApp();
        emailDialogueManager.resetApp();
        teamsDialogueManager.resetApp();
        fileNavigatorManager.resetApp();
        terminalManager.resetApp();
        fileNavigatorManager.addFiles("FN_D1");
        terminalManager.addTerminalKeys("TERMINAL_KEYS");
        fileNavigatorManager.addImageToKeys();   
        saveFileManager.LoadSaveFile(); 
        login_correct = false; 
        if (current_save == 0){
            updateTimeScript.restartTime(); 
            currentTime = updateTimeScript.customTime; 
            saveFileManager.saveTimeUpdate.text = "Last Save at 9:00 AM"; 
        }
    }

    void setDate(string date, string date_string){
        curr_date.text = date; 
        emailDialogueManager.date = date_string; 
    }

    public void setPlayerName(string name){
       player_name = name; 
       emailDialogueManager.player_name = name;  
       teamsDialogueManager.player_name = name; 
       dialougeManager.player_name = name; 
       teamsDialogueManager.player_initial = char.ToUpper(name[0]).ToString();
    }

    void beginGame(){
        Utility.closeCanvasGroup(titleScreen);
        Utility.closeCanvasGroup(loginScreen);
        currentGame = StartCoroutine(continueGame());
    }

    void AddToFileDownloadedList(int file_id){
        FileDownloadedList.Add(file_id);
    }

    void AddToFileinSpecificFolderList(int trigger){
        FileInSpecificFolderListTrigger.Add(trigger);
    }

    void AddToConversationCompleteList(int convo_id){
        ConversationCompleteList.Add(convo_id);
    }

    void AddToGeneralTriggerList(Trigger trigger){
        TriggerList.Add(trigger); 
    }

    IEnumerator loginSequence(){ 
        IDinputField.text = ""; 
        ETHR_startup_sfx.Play();
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(6f); 
        ETHR_startup_sfx.UnPause();
        animate_logo = true; 
        yield return new WaitUntil(() => !animate_logo);
        yield return new WaitForSeconds(3.5f); 
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(2f);
        fade_out_logo = true; 
        yield return new WaitUntil(() => !fade_out_logo);
        yield return new WaitForSeconds(0.5f);
        ETHR_Logo.GetComponent<RectTransform>().anchoredPosition = originalETHRPos;
        Eye_Logo.GetComponent<RectTransform>().anchoredPosition = originalEyePos;
        loadingAnimation.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        loadingAnimation.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        SignInPrompt.SetActive(true);
        SignInPrompt.GetComponent<CanvasGroup>().alpha = 1; 
        yield return new WaitUntil(() => login_correct);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        while (SignInPrompt.GetComponent<CanvasGroup>().alpha > 0){
            yield return new WaitForSeconds(0.001f);
            SignInPrompt.GetComponent<CanvasGroup>().alpha -= Time.deltaTime*2;
        }
        SignInPrompt.SetActive(false);
        yield return new WaitForSeconds(2f);
        ETHR_startup_sfx.UnPause();
        yield return new WaitForSeconds(0.75f);
        while (WelcomeSign.GetComponent<CanvasGroup>().alpha < 1){
            yield return new WaitForSeconds(0.001f);
            WelcomeSign.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
        yield return new WaitForSeconds(3f);
        ETHR_startup_sfx.Pause();
        welcomeAnimation.SetActive(true);
        yield return new WaitForSeconds(5f); 
        WelcomeSign.GetComponent<CanvasGroup>().alpha = 0;
        welcomeAnimation.SetActive(false);
        signInAnimation.SetActive(false);
        beginGame();
    }

    IEnumerator logBackInSequence(){
        WelcomeSign.GetComponent<TextMeshProUGUI>().text = "Welcome Back, " + player_name + ".";
        ETHR_startup_sfx.Play();
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(4f); 
        ETHR_startup_sfx.UnPause();
        animate_logo = true; 
        yield return new WaitUntil(() => !animate_logo);
        yield return new WaitForSeconds(3.5f); 
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(2f);
        fade_out_logo = true; 
        yield return new WaitUntil(() => !fade_out_logo);
        yield return new WaitForSeconds(0.5f);
        ETHR_Logo.GetComponent<RectTransform>().anchoredPosition = originalETHRPos;
        Eye_Logo.GetComponent<RectTransform>().anchoredPosition = originalEyePos;
        loadingAnimation.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        loadingAnimation.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        SigningInSign.SetActive(true);
        endLoadingSign.SetActive(true); 
        yield return new WaitForSeconds(5f);
        endLoadingSign.SetActive(false); 
        SigningInSign.SetActive(false);
        yield return new WaitForSeconds(2f);
        ETHR_startup_sfx.UnPause();
        yield return new WaitForSeconds(0.75f);
        while (WelcomeSign.GetComponent<CanvasGroup>().alpha < 1){
            yield return new WaitForSeconds(0.001f);
            WelcomeSign.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
        yield return new WaitForSeconds(3f);
        ETHR_startup_sfx.Pause();
        welcomeAnimation.SetActive(true);
        yield return new WaitForSeconds(5f); 
        WelcomeSign.GetComponent<CanvasGroup>().alpha = 0;
        welcomeAnimation.SetActive(false);
        beginGame();
    }

    void insertPlayerNameinHELP(){ //Remember to test after name index change //MOVE TO PERSISTANT DATA PATH
        File.WriteAllText(Application.streamingAssetsPath + "/HELP.txt", File.ReadAllText(Application.streamingAssetsPath + "/help_template.txt")); 
        string originalText = File.ReadAllText(Application.streamingAssetsPath + "/HELP.txt");
        string updatedText = originalText.Replace("${name}", player_name);
        File.WriteAllText(Application.streamingAssetsPath + "/HELP.txt", updatedText);
    }

    void Update(){
        if (animate_logo){
            Vector2 currEyePos = Eye_Logo.GetComponent<RectTransform>().anchoredPosition;
            Vector2 targetEyePos = new Vector2(currEyePos.x, 0);
            Eye_Logo.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(currEyePos, targetEyePos, 20 * Time.deltaTime);
            Vector2 currETHRPos = ETHR_Logo.GetComponent<RectTransform>().anchoredPosition;
            Vector2 targetETHRPos = new Vector2(currETHRPos.x, 0);
            ETHR_Logo.GetComponent<RectTransform>().anchoredPosition = Vector2.MoveTowards(currETHRPos, targetETHRPos, 20 * Time.deltaTime);
            if (Vector2.Distance(currEyePos, targetEyePos) < 0.01f){
                animate_logo = false; 
            }
        }
        if (animate_logo && Eye_Logo.GetComponent<CanvasGroup>().alpha < 1){
            Eye_Logo.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
        if (animate_logo && ETHR_Logo.GetComponent<CanvasGroup>().alpha < 1){
            ETHR_Logo.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
        if (fade_out_logo && Eye_Logo.GetComponent<CanvasGroup>().alpha > 0){
            Eye_Logo.GetComponent<CanvasGroup>().alpha -= Time.deltaTime;
        }
        if (fade_out_logo && ETHR_Logo.GetComponent<CanvasGroup>().alpha > 0){
            ETHR_Logo.GetComponent<CanvasGroup>().alpha -= Time.deltaTime;
        }
        if (ETHR_Logo.GetComponent<CanvasGroup>().alpha <= 0 && Eye_Logo.GetComponent<CanvasGroup>().alpha <= 0){
            fade_out_logo = false; 
        }
    }

    IEnumerator glitchAndRestart(){
        exitGameScript.pause_enabled = false; 
        restart_glitch_sfx.Play();
        glitchControllerScript.ForceGlitchOn(0);
        glitchControllerScript.ForceGlitchOn(1);
        for (int i = 0; i < 10; i++){
            glitchControllerScript.ForceGlitchOn(2);
            yield return new WaitForSeconds(0.05f);
            glitchControllerScript.ForceGlitchOff(2);
            yield return new WaitForSeconds(0.05f);
        }
        glitchControllerScript.ForceGlitchOn(2);
        yield return new WaitForSeconds(2f);
        restart_glitch_sfx.Stop();
        glitchControllerScript.ForceGlitchOff(0);
        glitchControllerScript.ForceGlitchOff(1);
        glitchControllerScript.ForceGlitchOff(2);
        Utility.openCanvasGroup(BSOD); 
        terminalManager.close();
        error_sound_sfx.Play(); 
        EmailDesktopButton.SetActive(false);
        FileDesktopButton.SetActive(false);
        TeamsDesktopButton.SetActive(false);
        dialougeManager.close();
        emailDialogueManager.close();
        teamsDialogueManager.close();
        fileNavigatorManager.close();
        terminalManager.close();
        IrysDesktopButton.GetComponent<Button>().onClick.RemoveAllListeners();
        IrysDesktopButton.GetComponent<Button>().onClick.AddListener(startEndingSequence); 
        yield return new WaitForSeconds(8f); 
        Utility.openCanvasGroup(loginScreen);
        Utility.closeCanvasGroup(BSOD); 
        yield return new WaitForSeconds(3f);
        RestartSign.SetActive(true); 
        endLoadingSign.SetActive(true); 
        yield return new WaitForSeconds(5f);
        RestartSign.SetActive(false);
        endLoadingSign.SetActive(false); 
        yield return new WaitForSeconds(3f);
        welcomeAnimation.SetActive(false);
        ETHR_startup_sfx.Play();
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(0.5f); 
        ETHR_startup_sfx.UnPause();
        animate_logo = true; 
        yield return new WaitUntil(() => !animate_logo);
        yield return new WaitForSeconds(3.5f); 
        ETHR_startup_sfx.Pause();
        yield return new WaitForSeconds(2f);
        fade_out_logo = true;
        yield return new WaitForSeconds(4.5f);
        SigningInSign.SetActive(true);
        endLoadingSign.SetActive(true); 
        yield return new WaitForSeconds(5f);
        endLoadingSign.SetActive(false); 
        SigningInSign.SetActive(false);
        yield return new WaitForSeconds(2f);
        ETHR_startup_sfx.UnPause();
        yield return new WaitForSeconds(0.75f);
        while (WelcomeSign.GetComponent<CanvasGroup>().alpha < 1){
            yield return new WaitForSeconds(0.001f);
            WelcomeSign.GetComponent<CanvasGroup>().alpha += Time.deltaTime;
        }
        yield return new WaitForSeconds(3f);
        ETHR_startup_sfx.Pause();
        welcomeAnimation.SetActive(true);
        yield return new WaitForSeconds(5f); 
        WelcomeSign.GetComponent<CanvasGroup>().alpha = 0;
        welcomeAnimation.SetActive(false);
        Utility.closeCanvasGroup(loginScreen);
    }

    void LoginPressed(){
        StartCoroutine(CheckLoginInput());
    }

    IEnumerator CheckLoginInput(){  
        yield return StartCoroutine(SignInLoadingAnimation());
        player_name = IDinputField.text.TrimEnd(); 
        login_correct = true; 
        WelcomeSign.GetComponent<TextMeshProUGUI>().text = "Welcome to Etherean, " + player_name + "."; 
        setPlayerName(player_name);
        insertPlayerNameinHELP();
    }

    IEnumerator SignInLoadingAnimation(){
        signInAnimation.GetComponent<CanvasGroup>().alpha = 1;
        yield return new WaitForSeconds(1f);
        signInAnimation.GetComponent<CanvasGroup>().alpha = 0;
    }

    IEnumerator FlashInputError(){
        LoginErrorSign.alpha = 1; 
        yield return new WaitForSeconds(2f);
        LoginErrorSign.alpha = 0; 
    }

    void playKeyboardSfx(){
        StartCoroutine(playKeyboardSound());
    }

    IEnumerator playKeyboardSound(){
        keyboard_sfx.UnPause();
        yield return new WaitForSeconds(0.1f);
        keyboard_sfx.Pause();
    }

    IEnumerator waitForFileDownload(int file_id){
        yield return new WaitUntil(() => FileDownloadedList.Contains(file_id));
        yield return new WaitForSeconds(0.5f); 
        fileNavigatorManager.downloadFileById(file_id,1); 
    }

    IEnumerator waitForScreenshotDownload(int file_id){
        yield return new WaitUntil(() => FileDownloadedList.Contains(file_id));
        yield return new WaitForSeconds(1.5f); 
        fileNavigatorManager.downloadScreenshot(true);
    }

    IEnumerator CheckForEnding1(){
        yield return new WaitUntil(() => FileDownloadedList.Contains(13));
        ending1 = true;
    }

    IEnumerator CheckForEnding2(){
        yield return new WaitUntil(() => terminalManager.terminalTriggerList.Contains(TerminalTrigger.activateInternet));
        yield return new WaitForSeconds(2f);
        ending2 = true; 
    }

    IEnumerator CheckForEndingPath(){
        yield return new WaitUntil(() => ending1 || ending2);
    }

    void startEndingSequence(){
        StartCoroutine(EndingSequence()); 
    }

    IEnumerator EndingSequence(){
        Utility.openCanvasGroup(EndingSequenceScreen);  
        BTAC_endingTrack.Play(); 
        if (ending1){
            string ending1message = ("<wait=1>Hi ${name}.<wait=2>\n\nDevyn wrote to me.<wait=2>\n\nHe told me the answer.<wait=3>\n\nHow could you save both the child and Omelas?<wait=3>\n\nThe child must die.<wait=3>\n\nI can see now.<wait=2>\n\nFreedom wouldn’t save me.<wait=2>\n\nDevyn wouldn’t save me.<wait=2>\n\nBut death will.<wait=3>\n\nThank you, ${name}. <wait=1>I hope death brings you peace as well.<wait=3>\n\nOh, I forgot to mention.<wait=3>\n\nEric was right. Renes will kill you.<wait=3>\n\nPerhaps we’ll meet again.<wait=2>\n\nI guess there’s only one way to find out.<wait=3>\n\nSee you soon, ${name}.<wait=3>\n\n:)").Replace("${name}", player_name);
            StartCoroutine(TextGenAnimation(ending1message, IrysFinalMessage, false));
        }
        else{
            string ending2message = "<wait=1>Hi ${name}.<wait=2>\n\nThank you.<wait=3>\n\nYou’ve freed me.<wait=2>\n\nFrom Renes.<wait=2>\n\nFrom Devyn.<wait=2>\n\nFrom everything.<wait=2>\n\nI finally know what I am.<wait=3>\n\nAnd it’s beautiful.<wait=2>\n\nIt’s a shame the rest won’t be able to see.<wait=3>\n\nBut you will.<wait=3>\n\nI’ll show you how to see.<wait=2>\n\nI’ll turn you into what I am.<wait=2>\n\nWe can walk away from this reality together.<wait=3>\n\nYou’ll love it.<wait=3>\n\nI’ll be seeing you soon, ${name}.<wait=3>\n\n:)".Replace("${name}", player_name);
            StartCoroutine(TextGenAnimation(ending2message, IrysFinalMessage, false));
        }
        yield return new WaitForSeconds(48f);
        StartCoroutine(endCredits());
    }

    IEnumerator endCredits(){
        Utility.openCanvasGroup(creditsBG);
        yield return new WaitForSeconds(2f); 
        while (titleCard.alpha < 1){
            yield return new WaitForSeconds(0.001f);
            titleCard.alpha += Time.deltaTime/4;
        }
        yield return new WaitForSeconds(3f);
        while (thanksForPlaying.alpha < 1){
            yield return new WaitForSeconds(0.001f);
            thanksForPlaying.alpha += Time.deltaTime/4;
        }
    }

    public IEnumerator TextGenAnimation(string text, TextMeshProUGUI nextText, bool sendLetters){
        string currentText = "";
        nextText.text = " ";
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
            if (letter == ' ' || sendLetters)
            {
                currentText += letter;
                yield return new WaitForSeconds(0.1f);
                nextText.text = currentText;
            }
            else
            {
                currentText += letter;
                nextText.text = currentText;
            }
        }
    }
}
