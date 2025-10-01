using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using FN_File_Nodes; 

public class IrysToolListScript : MonoBehaviour, IPointerDownHandler
{

    public Button fileButton; 
    public TextMeshProUGUI tool_name; 
    public TextMeshProUGUI tool_date;
    public TextMeshProUGUI tool_size;

    public GameObject copyPastePrefab;
    public GameObject instantiatedCopyPasteView; 
    public Canvas thisCanvas;
    public ClipboardScript clipboardScript; 

    public FN_IrysTool irysToolObject;
    public int file_id; 

    public bool isPlayerTool; 
    public TeamsDialogueManager teamsDialogueManager; 
     

    ///WE NEED TO MAKE A NEW FN OBJECT JUST FOR THIS IN THE DIALOGUE OBJECTS FILES


    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<FileDragDropScript>().thisCanvas = thisCanvas;
        copyPastePrefab.GetComponent<CopyPasteScript>().clipboardScript = clipboardScript;
        if (isPlayerTool){
            fileButton.onClick.AddListener(teamsDialogueManager.openApp);
            fileButton.onClick.AddListener(firstOpen);
        }
        else{
            fileButton.onClick.AddListener(teamsDialogueManager.openAppToError);
        }
    }

    public void InstantiateCPPrefabAtCursor(){
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = 0;
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = false;
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = true; 
        instantiatedCopyPasteView = Instantiate(copyPastePrefab, thisCanvas.transform);
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = true; 
        instantiatedCopyPasteView.GetComponent<CopyPasteScript>().fileObjectToCopy = irysToolObject;
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

    void firstOpen(){
        fileButton.onClick.RemoveListener(firstOpen);
    }





}
