using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class InputReplacerScript : MonoBehaviour
{
    public TMP_InputField inputField;
    public bool changeInput; 
    public string outputTarget = "";

    public AudioSource keyboard_sfx; 

    private void Start()
    {
        keyboard_sfx.Play();
        keyboard_sfx.Pause();
        inputField.onValueChanged.AddListener(HandleInput);
    }

    private void HandleInput(string _)
    {
        StartCoroutine(playKeyboardSound()); 
        if (changeInput){
            int count = inputField.text.Length;
            count = Mathf.Clamp(count, 0, outputTarget.Length);
            inputField.text = outputTarget.Substring(0, count);
            inputField.caretPosition = inputField.text.Length;
        }
        
    }

    IEnumerator playKeyboardSound(){
        keyboard_sfx.UnPause();
        yield return new WaitForSeconds(0.1f);
        keyboard_sfx.Pause();
    }
}