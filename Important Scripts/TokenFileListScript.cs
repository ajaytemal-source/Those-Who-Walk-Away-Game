using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using FN_File_Nodes; 

public class TokenFileListScript : MonoBehaviour, IPointerDownHandler
{
    public GameObject errorNotificationPrefab; 
    public RectTransform notifContainer;

    public Button fileButton; 
    public GameObject token_name; 
    public GameObject token_date;
    public GameObject token_size;

    public int file_id;

    //Reference to the Token Object 
    public FN_Token tokenObject; 

    public GameObject copyPastePrefab;
    public GameObject instantiatedCopyPasteView; 
    public Canvas thisCanvas;

    public ClipboardScript clipboardScript; 

    bool send_error_message; 
    float time_since_message = 6f;  

    void Start(){
        gameObject.GetComponent<FileDragDropScript>().file_id = file_id; 
        gameObject.GetComponent<FileDragDropScript>().thisCanvas = thisCanvas;
        copyPastePrefab.GetComponent<CopyPasteScript>().clipboardScript = clipboardScript;
        fileButton.onClick.AddListener(errorNotification); 
    }

    void Update(){
        if (send_error_message){
            if(time_since_message >= 6f){
                errorNotificationPrefab.GetComponent<DownloadNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = "Cannot open '" + token_name.GetComponent<TextMeshProUGUI>().text + "'.";  
                errorNotificationPrefab.GetComponent<DownloadNotificationScript>().notifContainer = notifContainer; 
                Instantiate(errorNotificationPrefab, notifContainer);
                time_since_message = 0f; 
            }
            send_error_message = false; 
        } 
        if (time_since_message < 6f){
            time_since_message += Time.deltaTime; 
        }
    }

    public void InstantiateCPPrefabAtCursor(){
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = 0;
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = false;
        copyPastePrefab.GetComponent<CopyPasteScript>().copy_button.interactable = true; 
        instantiatedCopyPasteView = Instantiate(copyPastePrefab, thisCanvas.transform);
        copyPastePrefab.GetComponent<CopyPasteScript>().paste_button.interactable = true; 
        instantiatedCopyPasteView.GetComponent<CopyPasteScript>().fileObjectToCopy = tokenObject;
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

    void errorNotification(){
        send_error_message = true; 
    }

}
