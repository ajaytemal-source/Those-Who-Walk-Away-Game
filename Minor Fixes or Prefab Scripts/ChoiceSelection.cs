using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChoiceSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update

    private Button button; 
    public bool pressed = false; 
    public bool is_hovering_over = false; 
 
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Press);
    }

    void Press(){
        StartCoroutine(isPressed());
    }

    IEnumerator isPressed(){
        pressed = true; 
        yield return new WaitForSeconds(0.2f);
        pressed = false;
    }

    public void OnPointerEnter(PointerEventData eventData){ 
        is_hovering_over = true; 
    }

    public void OnPointerExit(PointerEventData eventData){
        is_hovering_over = false; 
    }

}
