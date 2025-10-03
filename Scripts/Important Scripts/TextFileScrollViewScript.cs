using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO; 
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using System.Timers;

public class TextFileScrollViewScript : MonoBehaviour
{
    public Button exitButton; 
    public Button deactivate;
    public TextMeshProUGUI txtFileName; 
    public GameObject text_file_in_list; 
    public CanvasGroup this_text_file; 

    void Start(){
        makeTrasparent();
        deactivate.onClick.AddListener(makeTrasparent);
        exitButton.onClick.AddListener(makeTrasparent);
    }

    public void makeViewable(){
        gameObject.SetActive(true);
    }

    public void makeTrasparent(){
        gameObject.SetActive(false);
    }

}
