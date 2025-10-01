using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; 
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

using FN_File_Nodes; 


public class FolderScrollView : MonoBehaviour, IPointerDownHandler
{
    public int folder_id; 
    public Button deactivate;
    public Button exitButton; 
    public GameObject fileListView; 
    public CanvasGroup this_folder_view; 
    public GameObject fileDirectoryView; 
    public RectTransform fileDirectoryDisplay; 
    Button directoryActivateButton = null; 
    public string folder_name; 
    GameObject directoryButton;
    public Canvas thisCanvas; 
    public bool main_folder; //Checked for folder view that isnt a prefab and already exists in the scene 
    public bool empty_folder = false;  
    public GameObject emptyFolderSignPrefab; 
    RectTransform contentWindow; 

    //Reference to folder object 
    public FN_Folder folderObject;

    //Copy/Paste Prompt Prefab
    public GameObject copyPastePrefab; 
    public GameObject instantiatedCopyPasteView;

    //Reference to Clipboard
    public ClipboardScript clipboardScript; 


    void Start(){
        copyPastePrefab.GetComponent<CopyPasteScript>().clipboardScript = clipboardScript;
        if (main_folder){
            makeViewable();
        }
        else{
            makeTrasparent();
            deactivate.onClick.AddListener(makeTrasparent);
            deactivate.onClick.AddListener(removeAllDirectories);
            exitButton.onClick.AddListener(makeTrasparent);
            exitButton.onClick.AddListener(removeAllDirectories);
        }
        contentWindow = transform.Find("Viewport/Content") as RectTransform;
        GameEvents.FinishDwnldNotifPressed.Subscribe(openFolderFromNotif);

    }

    void Update(){
        if (directoryButton == null && !main_folder){
            makeTrasparent();
        }
        if (empty_folder && contentWindow.transform.childCount > 1){ 
            Destroy(contentWindow.transform.GetChild(0).gameObject);
            empty_folder = false; 
        }
        else if (!empty_folder && contentWindow.transform.childCount == 0){
            Instantiate(emptyFolderSignPrefab, contentWindow); 
            empty_folder = true; 
        }
    }

    public void activateFolderAccess(){
        Button activate = fileListView.GetComponent<Button>();
        activate.onClick.AddListener(makeViewable);
        activate.onClick.AddListener(createDirectoryView); 
    }

    public void openFolder(){
        makeViewable();
        createDirectoryView();
    }

    void makeViewable(){ //this doesn't work?
        gameObject.SetActive(true); 
    }

    void createDirectoryView(){
        fileDirectoryView.GetComponentInChildren<TextMeshProUGUI>().text = folder_name;
        directoryButton = Instantiate(fileDirectoryView, fileDirectoryDisplay);
        directoryActivateButton = directoryButton.GetComponent<Button>();  
        directoryActivateButton.onClick.AddListener(moveToDirectory);
    }

    void moveToDirectory(){
        int curr_index = directoryButton.transform.GetSiblingIndex();
        for (int i = fileDirectoryDisplay.transform.childCount-1; i > curr_index; i--){
            Transform child = fileDirectoryDisplay.transform.GetChild(i); 
            child.SetParent(null); 
            Destroy(child.gameObject); 
        }
    }
    
    void removeAllDirectories(){
        foreach (Transform child in fileDirectoryDisplay.transform)
        {
            child.transform.SetParent(null);
            Destroy(child.gameObject);  
        }
    }

    void makeTrasparent(){
        gameObject.SetActive(false); 
    }

    void openFolderFromNotif(int directory){ 
        if (directory == folder_id){
            Debug.Log("Yes"); 
            openFolder(); 
        }
    }

    public void InstantiateCPPrefabAtCursor(){
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = 0;  
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = false; 
        if (clipboardScript.GetComponent<ClipboardScript>().can_paste){
            copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = true;
        }
        else{
            copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = false;
        }        
        instantiatedCopyPasteView = Instantiate(copyPastePrefab, thisCanvas.transform);
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = true; 
        instantiatedCopyPasteView.GetComponent<CopyPasteScript>().folderObjectToPaste = folderObject; 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisCanvas.transform as RectTransform,
            Input.mousePosition,
            thisCanvas.worldCamera,
            out Vector2 localPoint
        );
        instantiatedCopyPasteView.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    public void OnPointerDown(PointerEventData eventData){
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InstantiateCPPrefabAtCursor();
        }
    }



}
