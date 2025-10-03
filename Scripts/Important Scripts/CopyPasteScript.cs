using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO; 
using UnityEngine.UI;
using UnityEngine.EventSystems;

using FN_File_Nodes;

public class CopyPasteScript : MonoBehaviour
{
    public Button copy_button; 
    public Button paste_button; 
    GameObject clicked_file; 
    GameObject clicked_folder_view; 
    bool hovering_over_button; 

    //References to Clipboard 
    public ClipboardScript clipboardScript;

    //Object Reference to What Was Copied 
    public FN_File fileObjectToCopy; 
    public FN_Folder folderObjectToPaste; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)){
            if (copy_button.GetComponent<ChoiceSelection>().is_hovering_over == false && paste_button.GetComponent<ChoiceSelection>().is_hovering_over == false){
                Destroy(gameObject); 
            }
        }
        else if (copy_button.GetComponent<ChoiceSelection>().pressed == true){
            clipboardScript.GetComponent<ClipboardScript>().canPaste(); 
            clipboardScript.GetComponent<ClipboardScript>().copiedFileObject = fileObjectToCopy; 
            Destroy(gameObject); 
        }
        else if (paste_button.GetComponent<ChoiceSelection>().pressed == true){
            clipboardScript.GetComponent<ClipboardScript>().PasteIntoFolder(folderObjectToPaste.id); 
            Destroy(gameObject); 
        }
    }






}
