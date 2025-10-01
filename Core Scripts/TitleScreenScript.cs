using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleScreenScript : MonoBehaviour
{
    public SaveFileManager saveFileManager; 

    public GameObject irysTitle;  
    public GameObject inputTitle;
    public GlobalGameEventScript globalGameManager; 
    public CanvasGroup titleWallpaper; 

    public Button newGame; 
    public Button continueGame; 
    public Button exitGame;

    public GameObject newGamePopUp; 
    public Button confirmNewGame; 
    public Button cancelNewGame; 
    public GameObject newGamePromptBlocker; 

    public AudioClip keyboard_sfx; 
    public AudioClip irys_sfx; 
    public AudioClip titleScreenMusic;
    public AudioClip laptopButton; 
    public AudioSource audioSource; 

    Vector2 continueButtonPos; 
    Vector2 newGameButtonPos; 
    Vector2 exitGameButtonPos; 

    void Start()
    {
        exitGame.onClick.AddListener(() => Application.Quit()); 
        continueGame.onClick.AddListener(() => StartCoroutine(exitTitleScreen())); 
        newGame.onClick.AddListener(newGameCheck);
        confirmNewGame.onClick.AddListener(resetSave); 
        cancelNewGame.onClick.AddListener(removeNewGamePopup); 
        continueButtonPos =  continueGame.GetComponent<RectTransform>().anchoredPosition;
        newGameButtonPos =  newGame.GetComponent<RectTransform>().anchoredPosition;
        exitGameButtonPos =  exitGame.GetComponent<RectTransform>().anchoredPosition;
    }

    public IEnumerator loadTitleScreen(){
        if (globalGameManager.current_save == 0){
            continueGame.gameObject.SetActive(false); 
            newGame.GetComponent<RectTransform>().anchoredPosition = continueButtonPos; 
            exitGame.GetComponent<RectTransform>().anchoredPosition = newGameButtonPos; 
        }
        else{
            continueGame.gameObject.SetActive(true); 
            newGame.GetComponent<RectTransform>().anchoredPosition = newGameButtonPos; 
            exitGame.GetComponent<RectTransform>().anchoredPosition = exitGameButtonPos;  
        }
        gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(irys_sfx); 
        irysTitle.gameObject.SetActive(true); 
        yield return new WaitForSeconds(0.3f); 
        irysTitle.GetComponentInChildren<TextMeshProUGUI>().text = "Those"; 
        yield return new WaitForSeconds(0.3f); 
        irysTitle.GetComponentInChildren<TextMeshProUGUI>().text = "Those Who"; 
        audioSource.Stop(); 
        yield return new WaitForSeconds(1f); 
        inputTitle.gameObject.SetActive(true);
        audioSource.PlayOneShot(keyboard_sfx); 
        yield return StartCoroutine(globalGameManager.TextGenAnimation("Walk Away", inputTitle.GetComponentInChildren<TextMeshProUGUI>(), true));
        audioSource.Stop();
        yield return new WaitForSeconds(1f); 
        audioSource.clip = titleScreenMusic;
        audioSource.loop = true; 
        audioSource.Play();
        yield return new WaitForSeconds(0.5f); 
        StartCoroutine(fadeIn(continueGame.GetComponent<CanvasGroup>()));
        StartCoroutine(fadeIn(titleWallpaper)); 
        yield return new WaitForSeconds(0.3f); 
        StartCoroutine(fadeIn(newGame.GetComponent<CanvasGroup>()));
        yield return new WaitForSeconds(0.3f); 
        StartCoroutine(fadeIn(exitGame.GetComponent<CanvasGroup>()));  
        enableAllButtons();     
    }

    public IEnumerator fadeIn(CanvasGroup canvasGroup){ //MOVE TO UTILITY 
        while (canvasGroup.alpha < 1){
            yield return new WaitForSeconds(0.001f);
            canvasGroup.alpha += Time.deltaTime*2;
        }
        canvasGroup.interactable = true; 
        canvasGroup.blocksRaycasts = true; 
    }

    public IEnumerator fadeOut(CanvasGroup canvasGroup){ //MOVE TO UTILITY
        while (canvasGroup.alpha > 0){
            yield return new WaitForSeconds(0.001f);
            canvasGroup.alpha -= Time.deltaTime*2;
        }
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / 2f);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop(); 
    }

    void disableAllButtons(){
        newGame.interactable = false; 
        continueGame.interactable = false; 
        exitGame.interactable = false; 
    }

    void enableAllButtons(){
        newGame.interactable = true; 
        continueGame.interactable = true; 
        exitGame.interactable = true;; 
    }

    IEnumerator exitTitleScreen(){
        disableAllButtons(); 
        StartCoroutine(fadeOut(titleWallpaper));
        StartCoroutine(fadeOut(gameObject.GetComponent<CanvasGroup>()));
        StartCoroutine(fadeOut(continueGame.GetComponent<CanvasGroup>()));
        StartCoroutine(fadeOut(newGame.GetComponent<CanvasGroup>()));
        StartCoroutine(fadeOut(exitGame.GetComponent<CanvasGroup>()));
        yield return StartCoroutine(FadeOutAudio()); 
        audioSource.loop = false; 
        yield return new WaitForSeconds(4f); 
        globalGameManager.startGame();
        ResetTitleScreen(); 
        audioSource.PlayOneShot(laptopButton); 
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }

    void ResetTitleScreen(){
        audioSource.volume = 1f;
        irysTitle.gameObject.SetActive(false);
        irysTitle.GetComponentInChildren<TextMeshProUGUI>().text = ""; 
        inputTitle.gameObject.SetActive(false);
        Utility.closeCanvasGroup(continueGame.GetComponent<CanvasGroup>());
        Utility.closeCanvasGroup(newGame.GetComponent<CanvasGroup>());
        Utility.closeCanvasGroup(exitGame.GetComponent<CanvasGroup>());
    }

    void newGameCheck(){
        if (globalGameManager.current_save == 0){
            resetSave(); 
        }
        else{
            newGamePromptBlocker.SetActive(true); 
            newGamePopUp.SetActive(true); 
        }
    }

    void removeNewGamePopup(){
        newGamePromptBlocker.SetActive(false); 
        newGamePopUp.SetActive(false); 
    }

    void resetSave(){
        removeNewGamePopup(); 
        saveFileManager.deleteSaveFile(); 
        globalGameManager.presetGameState();  
        StartCoroutine(exitTitleScreen());  
    }

}
