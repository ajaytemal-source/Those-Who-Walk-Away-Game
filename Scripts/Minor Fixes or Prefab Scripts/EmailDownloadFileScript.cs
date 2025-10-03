using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailDownloadFileScript : MonoBehaviour
{
    public RectTransform notifContainer; 
    public Canvas thisCanvas;  
    public GameObject downloadNotificationPrefab; 
    public Button button; 
    public TextMeshProUGUI header; 
    public TextMeshProUGUI size; 
    public TMP_Text textElement;
    public bool pressed; 
    
    //Used to give the download id when raising the game event, to whatever script needs it
    public int download_id; 
 
    void Start()
    {
        button.onClick.AddListener(Press);
    }

    void Press(){
        isPressed();
    }

    void isPressed(){
        pressed = true; 
        textElement.color = Color.black; 
        button.interactable = false;
        setDownloadNotif(header.text, size.text);
        Instantiate(downloadNotificationPrefab, notifContainer);  
        GameEvents.FileDownloaded.Raise(download_id); 
    }

    void setDownloadNotif(string header, string size){
        downloadNotificationPrefab.GetComponent<DownloadNotificationScript>().header.GetComponent<TextMeshProUGUI>().text = "Downloading " + header + "...";
        downloadNotificationPrefab.GetComponent<DownloadNotificationScript>().size.GetComponent<TextMeshProUGUI>().text = size; 
        downloadNotificationPrefab.GetComponent<DownloadNotificationScript>().notifContainer = notifContainer; 
    }
}