using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO; 
using UnityEngine.UI;

using FN_File_Nodes;

public class ClipboardScript : MonoBehaviour
{
    public FileNavigatorManager fileNavigatorManager; 

    //Signals
    public bool can_copy = true; 
    public bool can_paste = false; 
    public bool copy_signal; 

    //Object reference of the file copied 
    public FN_File copiedFileObject; 

    public void canPaste(){
        can_paste = true;
    }

    public void PasteIntoFolder(int folder_id){
        copiedFileObject.directory = folder_id; 
        fileNavigatorManager.addFileToDirectory(copiedFileObject); 
    }

    public void resetClipboard(){
        can_copy = true; 
        can_paste = false; 
        copiedFileObject = null; 
    }

}
