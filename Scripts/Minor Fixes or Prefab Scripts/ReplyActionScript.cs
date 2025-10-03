using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ReplyActionScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public FileNavigatorManager fileNavigatorManager;
    public EmailDialougeManager emailDialougeManager; 
    public TextMeshProUGUI reply_content; 

    public Button sendButton;

    public GameObject textFilePrefab;  

    public GameObject pending_text; 

    public TextMeshProUGUI respondent;

    public Button attachementButton; 

    public string text_name;     

    public string text_size;

    private bool isHovering = false; 

    public bool expecting_attachment = false; 

    public CanvasGroup dragFileInstructions; 


    public bool attachment_has_been_attached = false; 
    public CanvasGroup attachment_canvasgroup;
    public GameObject attachment_name; 
    public GameObject attachment_size;  

    public int attachment_file_id; 
    public int expected_file_id; 
    
    void Start(){
        Image image = gameObject.GetComponent<Image>();
        image.raycastTarget = true; 
    }

    void Update(){
        if (isHovering){
            if (!Input.GetMouseButton(0) && attachment_file_id == expected_file_id){
                    addAttachmentInfo(fileNavigatorManager.file_dict[attachment_file_id].name, fileNavigatorManager.file_dict[attachment_file_id].size);
                    emailDialougeManager.reply_attached_file_id = attachment_file_id; 
                    attachment_has_been_attached = true;
                    isHovering = false; 
            }
        }
    }
    
    
    public void OnPointerEnter(PointerEventData eventData){
        if (expecting_attachment && eventData.pointerDrag != null){ 
            var draggedObject = eventData.pointerDrag.GetComponent<DragDropScript>();
            if (draggedObject != null){
                attachment_file_id = eventData.pointerDrag.GetComponent<DragDropScript>().file_id;
                isHovering = true; 
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (expecting_attachment && eventData.pointerDrag != null){
            isHovering = false; 
        }
    }

    public void addAttachmentInfo(string file_name, string file_size){
        attachment_size.GetComponent<TextMeshProUGUI>().text = file_size; 
        attachment_name.GetComponent<TextMeshProUGUI>().text = file_name; 
        attachment_canvasgroup.alpha = 1; 
    }

}
