using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using FN_File_Nodes;


public class FolderListScript : MonoBehaviour, IPointerDownHandler
{
    public GameObject file_name; 
    public GameObject date;  
    public GameObject folderScroll; 
    public GameObject thisFolderScroll; 

    public FN_Folder folderObject; 

    public CanvasGroup lockedFolderIcon; 
    public ClipboardScript clipboardScript;
    public GameObject copyPastePrefab; 
    public GameObject instantiatedCopyPasteView;
    public Canvas thisCanvas;  


    //Locked info 
    public GameObject passwordPrompt; 
    public CanvasGroup passwordPromptBlocker; 
    public CanvasGroup passwordError; 
    public string password; 
    public bool is_locked = false;   



    public GameObject makeFolder(GameObject fileInList, Button backButton, Button exitButton, RectTransform FileNavigatorPanel, RectTransform fileDirectoryDisplay, ClipboardScript clipboardScript, Canvas FD_canvas){
        GameObject this_folder = Instantiate(folderScroll, FileNavigatorPanel, false);
        this_folder.GetComponent<FolderScrollView>().thisCanvas = FD_canvas; //Remove 
        this_folder.GetComponent<FolderScrollView>().clipboardScript = clipboardScript; //Remove 
        this_folder.GetComponent<FolderScrollView>().fileListView = fileInList; 
        this_folder.GetComponent<FolderScrollView>().deactivate = backButton; //Remove 
        this_folder.GetComponent<FolderScrollView>().folder_name = file_name.GetComponent<TextMeshProUGUI>().text; //?
        this_folder.GetComponent<FolderScrollView>().fileDirectoryDisplay = fileDirectoryDisplay; 
        this_folder.GetComponent<FolderScrollView>().folderObject = folderObject;  
        this_folder.GetComponent<FolderScrollView>().exitButton = exitButton; 
        thisFolderScroll = this_folder; 
        return this_folder; 
    }

    public void lockFolder(GameObject pwordPrompt, string pword, CanvasGroup pwordPromptBlocker, CanvasGroup errorMessage, bool isAdmin){
        passwordPrompt = pwordPrompt;
        is_locked = true; 
        passwordPromptBlocker = pwordPromptBlocker;
        lockedFolderIcon.alpha = 1; 
        passwordError = errorMessage; 
        if (!isAdmin){
            password = pword; 
            passwordPrompt.GetComponentInChildren<TMP_InputField>().onSubmit.AddListener(checkPassword);
        }
        gameObject.GetComponent<Button>().onClick.AddListener(showPasswordPrompt);
    }

    public void showPasswordPrompt(){
        passwordError.alpha = 0; 
        Utility.openCanvasGroup(passwordPromptBlocker); 
        Utility.openCanvasGroup(passwordPrompt.GetComponent<CanvasGroup>());
    }

    public void checkPassword(string input){
        if (input == password){
            unlockFolder();
            thisFolderScroll.GetComponent<FolderScrollView>().openFolder();
        }
        else{
            passwordError.alpha = 1; 
        }  
    }   

    public void unlockFolder(){
        gameObject.GetComponent<Button>().onClick.RemoveListener(showPasswordPrompt);
        Utility.closeCanvasGroup(passwordPrompt.GetComponent<CanvasGroup>());
        Utility.closeCanvasGroup(passwordPromptBlocker); 
        thisFolderScroll.GetComponent<FolderScrollView>().activateFolderAccess(); 
    }



    public void OnPointerDown(PointerEventData eventData){
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            InstantiateCPPrefabAtCursor();
        }
    }

    public void InstantiateCPPrefabAtCursor(){
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = 0;
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = false;
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = false;
        instantiatedCopyPasteView = Instantiate(copyPastePrefab, thisCanvas.transform);
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = true;
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = true; 
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisCanvas.transform as RectTransform,
            Input.mousePosition,
            thisCanvas.worldCamera,
            out Vector2 localPoint
        );

        instantiatedCopyPasteView.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

}