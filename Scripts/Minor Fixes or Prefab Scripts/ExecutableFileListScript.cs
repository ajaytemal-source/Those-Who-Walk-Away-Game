using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExecutableFileListScript : MonoBehaviour
{
    public AudioSource error_sound;
    public Button fileButton; 
    public GameObject key_name; 
    public GameObject key_date;
    public GameObject key_size;
    public Canvas thisCanvas; 

    void Start(){
        fileButton.onClick.AddListener(createBSODScreen); 
    }

    void createBSODScreen(){
        error_sound.Play(); 
        thisCanvas.transform.GetChild(thisCanvas.transform.childCount - 1).GetComponent<CanvasGroup>().alpha = 1; 
        thisCanvas.transform.GetChild(thisCanvas.transform.childCount - 1).GetComponent<CanvasGroup>().interactable = true; 
    }

}
