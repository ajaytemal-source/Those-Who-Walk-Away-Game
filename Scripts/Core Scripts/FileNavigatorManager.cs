using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.IO; 
using System.Linq;
using UnityEditor;

using FN_File_Nodes;


public class FileNavigatorManager : MonoBehaviour
{
    //Main Application References 
    public Transform wallpaperTransform; 
    public CanvasGroup FD_CanvasGroup; 
    public GameObject FD_CentralObject; 
    public Button startButton; 
    public Button minimzeButton;
    public Button exitButton; 
    public Canvas thisCanvas; 

    public AudioSource keyboard_sfx;

    //Script for Managing Application Resizing 
    public WindowResizerScript windowResizerScript;
    public TeamsDialogueManager teamsDialogueManager; 
    public UpdateTimeScript updateTimeScript;

    //Reference to Clipboard Script
    public ClipboardScript clipboardScript; 

    public RectTransform notifContainer; //Reference to In Scene Notif Container 


    //Parts from the Main FN Application 
    public RectTransform contentWindow; 
    public RectTransform panelWindow; //Should be renamed to "Folder Scroll View Container" 
    public RectTransform textFileScrollContainer; 
    public RectTransform fileDirectoryDisplay; 
    public Button backButton; 

    public GameObject passwordPrompt;
    public Button passwordPromptExit; 
    public CanvasGroup passwordPromptBlocker; 
    public CanvasGroup passwordError; 

    public GameObject adminLockPrompt; 

    //Prefabs for each folder 
    public GameObject folderListView; 
    public GameObject folderContentView; 
    public GameObject folderDirectoryView; 
    public GameObject emptyFolderSign; 
    public GameObject textListView; 
    public GameObject tokenListView; 
    public GameObject irysToolListView;
    public GameObject exeListView; 

    //New Folder Instance Variables
    string file_name; 
    string date; 
    string subFolderTxtFile; 

    //For Folder Expecting Files for Consequences 
    public bool gotFile = false; 
    string expectedFileName; 

    //Download Finish Notification Prefab 
    public GameObject finishedDownloadNotifPrefab; 

    //Taskbar Icon Prefab
    public GameObject FD_TaskBarIcon; 

    //Reference to Taskbar View 
    public RectTransform taskbar_view; 

    //Taskbar Icon Holder
    GameObject currTaskBarIcon = null; 

    bool currently_open = false; 

    //TEMPORARY For Phase 3/4
    public GameObject imageInView; 
    public GameObject imageView; 
    public GameObject screenshotImageView;

    //if true, adding files to directory will be added to saved files, otherwise not 
    public bool addFilesToSave = true; 

    //Dictionaries containing all Folders/Files, referenced by their ids 
    public Dictionary<int, FN_Folder> folder_dict = new();
    public Dictionary<int, FN_File> file_dict = new(); 
    public Dictionary<int, HashSet<int>> unsaved_files = new(); 

    public GameObject adminFolderListView; 

    //Used to check when the player opens the first IRYS tool 
    public bool firstIrysToolOpened = false; 

    //Dictionary Containing all File Trigger Pairs, Are Indexed by the Directory that will trigger once the file is in it. 
    private Dictionary<int, List<FileTriggerPair>> file_trigger_dict = new();

    void Start(){
        close();
        clipboardScript = GetComponent<ClipboardScript>(); 
        startButton.onClick.AddListener(open);
        minimzeButton.onClick.AddListener(minimize);
        exitButton.onClick.AddListener(close);     
        folderListView.GetComponent<FolderListScript>().clipboardScript = clipboardScript; 
        folderListView.GetComponent<FolderListScript>().thisCanvas = thisCanvas; 
        textListView.GetComponent<TextListScript>().clipboardScript = clipboardScript; 
        textListView.GetComponent<TextListScript>().thisCanvas = thisCanvas; 
        tokenListView.GetComponent<TokenFileListScript>().clipboardScript = clipboardScript;
        tokenListView.GetComponent<TokenFileListScript>().thisCanvas = thisCanvas;
        tokenListView.GetComponent<TokenFileListScript>().notifContainer = notifContainer;
        irysToolListView.GetComponent<IrysToolListScript>().thisCanvas = thisCanvas; 
        irysToolListView.GetComponent<IrysToolListScript>().clipboardScript = clipboardScript; 
        folder_dict[0] = new FN_Folder{id = 0, contents = contentWindow}; // Main Directory 
        finishedDownloadNotifPrefab.GetComponent<DownloadNotificationScript>().size.GetComponent<TextMeshProUGUI>().text = "/Downloads"; 
        finishedDownloadNotifPrefab.GetComponent<DownloadNotificationScript>().notifContainer = notifContainer; 
        passwordPromptExit.onClick.AddListener(() => closePrompt(passwordPrompt));
        passwordPrompt.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(HandleInput);
        adminLockPrompt.GetComponentInChildren<Button>().onClick.AddListener(() => closePrompt(adminLockPrompt)); 
        irysToolListView.GetComponent<IrysToolListScript>().teamsDialogueManager = teamsDialogueManager;
        GameEvents.FinishDwnldNotifPressed.Subscribe(openFNFromNotif);
    }

    public void addFiles(string files_to_parse_file){
        TextAsset FD_files_json = Resources.Load<TextAsset>(files_to_parse_file);
        ParseFiles(FD_files_json);
    }

    public void addFolderToDirectory(FN_Folder folder){
        if(folder.directory != -1){ 
            if(folder.file_trigger_pairs != null){
                foreach (FileTriggerPair file_trigger_pair in folder.file_trigger_pairs){ 
                    if (!file_trigger_dict.ContainsKey(folder.id)){
                        file_trigger_dict[folder.id] = new List<FileTriggerPair>();
                    }
                    file_trigger_dict[folder.id].Add(file_trigger_pair);
                }
            }
            folderListView.GetComponent<FolderListScript>().file_name.GetComponent<TextMeshProUGUI>().text = folder.name;
            folderListView.GetComponent<FolderListScript>().date.GetComponent<TextMeshProUGUI>().text = folder.date;
            GameObject curr_folder = Instantiate(folderListView, folder_dict[folder.directory].contents);
            curr_folder.GetComponent<FolderListScript>().folderObject = folder; 
            GameObject folderGameObject = curr_folder.GetComponent<FolderListScript>().makeFolder(curr_folder, backButton, exitButton, panelWindow, fileDirectoryDisplay, clipboardScript, thisCanvas); 
            folderGameObject.GetComponent<FolderScrollView>().folder_id = folder.id; 
            folder.contents = folderGameObject.GetComponent<RectTransform>().Find("Viewport/Content") as RectTransform;  
            if (folder.password != null){
                if(folder.isAdmin){
                    curr_folder.GetComponent<FolderListScript>().lockFolder(adminLockPrompt, folder.password, passwordPromptBlocker, passwordError, true);
                    adminFolderListView = curr_folder; 
                }
                else{
                    curr_folder.GetComponent<FolderListScript>().lockFolder(passwordPrompt, folder.password, passwordPromptBlocker, passwordError, false);
                }
            }
            else{
                folderGameObject.GetComponent<FolderScrollView>().activateFolderAccess();
            }
        }
        folder_dict[folder.id] = folder;
    }

    public void addFileToDirectory(FN_File file){
        if (file_trigger_dict.ContainsKey(file.directory)){
            foreach (FileTriggerPair fileTriggerPair in file_trigger_dict[file.directory]){
                if (fileTriggerPair.file == file.id){
                    GameEvents.FileInSpecificFolder.Raise(fileTriggerPair.trigger);
                    break;
                }
            }
        }
        if(file.directory != -1){
            if (file is FN_Token token){
                GameObject tokenInstance = Instantiate(tokenListView, folder_dict[token.directory].contents);
                TokenFileListScript tokenScript = tokenInstance.GetComponent<TokenFileListScript>();
                tokenScript.file_id = file.id;
                tokenScript.token_name.GetComponent<TextMeshProUGUI>().text = token.name;
                tokenScript.token_date.GetComponent<TextMeshProUGUI>().text = "11/20/25 " + updateTimeScript.customTime.ToString("h:mm tt");
                tokenScript.token_size.GetComponent<TextMeshProUGUI>().text = token.size;
                tokenScript.tokenObject = token;
            }
            else if(file is FN_IrysTool tool){
                GameObject toolInstance = Instantiate(irysToolListView, folder_dict[tool.directory].contents);
                IrysToolListScript toolScript = toolInstance.GetComponent<IrysToolListScript>();
                toolScript.file_id = file.id;
                toolScript.tool_name.text = tool.name;
                toolScript.tool_size.text = tool.size;
                toolScript.tool_date.text = "11/20/25 " + updateTimeScript.customTime.ToString("h:mm tt");
                toolScript.irysToolObject = tool;
                if (file.id == 5){ // CHECKS WHICH TOOL IT IS
                    irysToolListView.GetComponent<IrysToolListScript>().isPlayerTool = true;
                    toolInstance.GetComponent<Button>().onClick.AddListener(() => firstIrysToolOpened = true);
                }
            }
            else if (file is FN_Txt txt){
                GameObject newTextFile = Instantiate(textListView, folder_dict[txt.directory].contents);
                TextListScript textScript = newTextFile.GetComponent<TextListScript>();
                textScript.file_id = file.id;
                textScript.tf_name.GetComponent<TextMeshProUGUI>().text = txt.name;
                textScript.tf_date.GetComponent<TextMeshProUGUI>().text = "11/20/25 " + updateTimeScript.customTime.ToString("h:mm tt");
                textScript.tf_size.GetComponent<TextMeshProUGUI>().text = txt.size;
                textScript.txtObject = txt;
                if (txt.textFileScroll == null){
                    newTextFile.GetComponent<TextListScript>().makeTextFile(newTextFile, txt.name, textFileScrollContainer, txt.content, exitButton);
                }
                else{
                    newTextFile.GetComponent<TextListScript>().thisTextFileView = txt.textFileScroll; 
                }
            }
            if (addFilesToSave){
                if (!unsaved_files.ContainsKey(file.id)){
                unsaved_files[file.id] = new HashSet<int>();
                } 
                unsaved_files[file.id].Add(file.directory); 
            }
        }
        file_dict[file.id] = file;
    }

    public void downloadFileById(int id, int directory){ //Used in GGM upon trigger signals
        file_dict[id].directory = directory;
        addFileToDirectory(file_dict[id]);
        finishedDownloadNotifPrefab.GetComponent<DownloadNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = file_dict[id].name; 
        Instantiate(finishedDownloadNotifPrefab, notifContainer); 
    }

    public void AddFolderById(int id, int directory){ //add boolean parameter for download notif? (Includes function above)
        folder_dict[id].directory = directory; 
        addFolderToDirectory(folder_dict[id]);
    }

    public void AddFileById(int id, int directory){ //Used in GGM upon trigger signals
        file_dict[id].directory = directory;
        addFileToDirectory(file_dict[id]);
    }

    public void ParseFiles(TextAsset json_file){
        RawNodeCollection nodeList = JsonUtility.FromJson<RawNodeCollection>(json_file.text);
        foreach (RawNode node in nodeList.nodes){
            if (node.type == "folder"){
                FN_Folder folder = new FN_Folder{
                    id = node.id, 
                    name = node.name,
                    date = node.date,
                    directory = node.directory,
                    password = node.password, 
                    isAdmin = node.isAdmin, 
                    file_trigger_pairs = node.file_trigger_pairs
                };
                addFolderToDirectory(folder);
            }
            else if (node.type == "tkn"){
                FN_Token token = new FN_Token{
                    id = node.id, 
                    name = node.name,
                    date = node.date,
                    size = node.size, 
                    directory = node.directory
                };
                addFileToDirectory(token);
            }
            else if (node.type == "txt"){
                FN_Txt txtFile = new FN_Txt{
                    id = node.id,
                    name = node.name, 
                    date = node.date,
                    size = node.size, 
                    content = node.content, 
                    directory = node.directory
                };
                addFileToDirectory(txtFile);
            }
            else if (node.type == "exe"){
                FN_IrysTool irysTool = new FN_IrysTool{
                    id = node.id, 
                    name = node.name,
                    date = node.date,
                    size = node.size, 
                    directory = node.directory
                };
                addFileToDirectory(irysTool);
            }
        }
    }

    void closePrompt(GameObject prompt){
        Utility.closeCanvasGroup(prompt.GetComponent<CanvasGroup>());
        Utility.closeCanvasGroup(passwordPromptBlocker);
        passwordError.alpha = 0; 
        passwordPrompt.GetComponentInChildren<TMP_InputField>().text = ""; 
    }

    private void HandleInput(string _){
        StartCoroutine(playKeyboardSound());
    }

    IEnumerator playKeyboardSound(){
        keyboard_sfx.UnPause();
        yield return new WaitForSeconds(0.1f);
        keyboard_sfx.Pause();
    }
    
    public void close(){
        if (currTaskBarIcon != null){
            Destroy(currTaskBarIcon); 
        }
        currently_open = false; 
        closePrompt(passwordPrompt); 
        closePrompt(adminLockPrompt);
        minimize();
    }

    void minimize(){
        Utility.windowMinimize(FD_CanvasGroup, windowResizerScript);
    }

    void reopen(){
        Utility.windowReopen(FD_CentralObject, FD_CanvasGroup, windowResizerScript);
    }

    void open(){
        Utility.windowOpen(FD_CentralObject, FD_CanvasGroup, windowResizerScript, currently_open, new Vector2(10f, 50f));
        if(!currently_open){
            currTaskBarIcon = Instantiate(FD_TaskBarIcon, taskbar_view);
            currTaskBarIcon.GetComponent<Button>().onClick.AddListener(openOrClose);
            currently_open = true;
        }
        reopen();
    }

    void openOrClose(){
        Utility.tbIconOpenOrClose(FD_CentralObject, FD_CanvasGroup, windowResizerScript);
    }

    void openFNFromNotif(int directory){
        if(currently_open){
            reopen();
        }
        else{
            open(); 
        }
    }

    public void DownloadIRYSTool(){
        irysToolListView.GetComponent<IrysToolListScript>().isPlayerTool = true; 
        downloadFileById(5,1);
    }

    public void DownloadIrysETProfile(){ 
        irysToolListView.GetComponent<IrysToolListScript>().isPlayerTool = false; 
        downloadFileById(6,1);
    }

    public void unlockAdminFolder(){
        adminFolderListView.GetComponent<FolderListScript>().unlockFolder(); 
    }

    public void addImageToKeys(){
        GameObject image = Instantiate(imageInView, folder_dict[5].contents);
        imageView.GetComponent<TextFileScrollViewScript>().exitButton = exitButton; 
        GameObject imageScroll = Instantiate(imageView, textFileScrollContainer);
        image.GetComponentInChildren<Button>().onClick.AddListener(() => imageScroll.GetComponent<TextFileScrollViewScript>().makeViewable());
    }

    public void downloadScreenshot(bool sendNotif){
        if(sendNotif){
            finishedDownloadNotifPrefab.GetComponent<DownloadNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = "IMG.png"; 
            Instantiate(finishedDownloadNotifPrefab, notifContainer);
        }
        imageInView.GetComponentInChildren<TextMeshProUGUI>().text = "IMG.png";
        GameObject image = Instantiate(imageInView, folder_dict[1].contents);
        screenshotImageView.GetComponent<TextFileScrollViewScript>().exitButton = exitButton; 
        GameObject imageScroll = Instantiate(screenshotImageView, textFileScrollContainer);
        image.GetComponentInChildren<Button>().onClick.AddListener(() => imageScroll.GetComponent<TextFileScrollViewScript>().makeViewable());
    }

    public void resetApp(){
        clipboardScript.resetClipboard(); 
        folder_dict.Clear();
        file_dict.Clear();
        unsaved_files.Clear();
        for (int i = contentWindow.childCount - 1; i >= 0; i--){
            Destroy(contentWindow.GetChild(i).gameObject);
        }
        folder_dict[0] = new FN_Folder{id = 0, contents = contentWindow};
        for (int i = panelWindow.childCount - 1; i >= 0; i--){
            Destroy(panelWindow.GetChild(i).gameObject);
        }
        for (int i = textFileScrollContainer.childCount - 1; i >= 0; i--){
            Destroy(textFileScrollContainer.GetChild(i).gameObject);
        }
        for (int i = fileDirectoryDisplay.childCount - 1; i >= 0; i--){
            Destroy(fileDirectoryDisplay.GetChild(i).gameObject);
        }
        close(); 
        StopAllCoroutines(); 
    }
}