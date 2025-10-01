using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using FN_File_Nodes;

public class TextListScript : MonoBehaviour, IPointerDownHandler//, IPointerUpHandler
{
    //Instance Variables 
    public GameObject tf_name; 
    public GameObject tf_date;
    public GameObject tf_size;
    public GameObject thisTextFileView; 

    public int file_id;
    public Canvas thisCanvas;

    //Reference to the Txt Object 
    public FN_Txt txtObject; 

    //Scroll View Prefab 
    public GameObject textFileView; 


    //Copy/Paste Prompt Prefab
    public GameObject copyPastePrefab; 
    public GameObject instantiatedCopyPasteView; 

    //Refrence to ClipBoard 
    public ClipboardScript clipboardScript; 

    //Button for Text File Content View 
    public Button textFileButton; 


    void Start(){
        copyPastePrefab.GetComponent<CopyPasteScript>().clipboardScript = clipboardScript;
    }

    public void makeTextFile(GameObject textInList, string txtFileName, RectTransform FileNavigatorPanel, string textFileName, Button exitButton){
        gameObject.GetComponent<FileDragDropScript>().file_id = file_id; 
        gameObject.GetComponent<FileDragDropScript>().thisCanvas = thisCanvas;
        textFileView.GetComponent<TextFileScrollViewScript>().exitButton = exitButton; 
        textFileView.GetComponent<TextFileScrollViewScript>().txtFileName.text = txtFileName;
        thisTextFileView = Instantiate(textFileView,  FileNavigatorPanel);
        thisTextFileView.GetComponent<TextFileScrollViewScript>().text_file_in_list = textInList; 
        TextAsset textFile = Resources.Load<TextAsset>(textFileName); 
        RectTransform child = thisTextFileView.transform.Find("Viewport/Content/Text") as RectTransform; 
        TextMeshProUGUI textComponent = child.GetComponent<TextMeshProUGUI>();
        textComponent.text = File.ReadAllText(Application.streamingAssetsPath + "/" + textFileName + ".txt"); 
        txtObject.textFileScroll = thisTextFileView; 
        textFileButton.onClick.AddListener(viewTextFile);
    }

    void Update(){
        if (thisTextFileView != null){
            textFileButton.onClick.AddListener(viewTextFile);
        }
    }

    void viewTextFile(){
        thisTextFileView.gameObject.SetActive(true); 
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
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = true; 
        instantiatedCopyPasteView = Instantiate(copyPastePrefab, thisCanvas.transform);
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = true; 
        instantiatedCopyPasteView.GetComponent<CopyPasteScript>().fileObjectToCopy = txtObject;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisCanvas.transform as RectTransform,
            Input.mousePosition,
            thisCanvas.worldCamera,
            out Vector2 localPoint
        );
        instantiatedCopyPasteView.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }


}
