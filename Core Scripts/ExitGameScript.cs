using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; 

public class ExitGameScript : MonoBehaviour
{
    public Button exitButton; 
    public Button logOutButton; 
    public Button shutDownButton; 
    public GameObject logOutWindow;
    public CanvasGroup pausedScreen; 
    public GameObject confirmExitScreen; 
    public Button confirmExitButton;
    public Button cancelExitButton; 
    public Button exitPopUpView; 

    public bool pause_enabled = false; 
    private bool isPaused = false;

    public GlobalGameEventScript globalGameManager;  
    public TitleScreenScript titleScreenScript;

    void Start(){
        logOutWindow.SetActive(false); 
        shutDownButton.onClick.AddListener(shutDown); 
        logOutButton.onClick.AddListener(confirmLogOut); 
        exitButton.onClick.AddListener(openLogOutWindow);
        cancelExitButton.onClick.AddListener(() => confirmExitScreen.SetActive(false));
        exitPopUpView.onClick.AddListener(() => confirmExitScreen.SetActive(false));
    } 

    void openLogOutWindow(){
        Pause();
        EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
        logOutWindow.SetActive(true); 
    }

    void closeLogOutWindow(){
        ResumeGame();
        logOutWindow.SetActive(false);
        confirmExitScreen.SetActive(false);
    }

    void LateUpdate()
    { 
        if (pause_enabled){
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused)
                {
                    openLogOutWindow();
                }
                else
                {
                    closeLogOutWindow();
                }
            }
            else if (isPaused && !EventSystem.current.currentSelectedGameObject){
                closeLogOutWindow();
            }
        }
    }

    void Pause()
    {
        Utility.openCanvasGroup(pausedScreen);
        Time.timeScale = 0f; // freezes the game
        isPaused = true;
    }

    void ResumeGame()
    {
        Utility.closeCanvasGroup(pausedScreen);
        Time.timeScale = 1f; // resumes the game 
        isPaused = false;
    }

    void logOut(){
        confirmExitScreen.SetActive(false);
        ResumeGame(); 
        logOutWindow.SetActive(false);
        globalGameManager.presetGameState();
        StartCoroutine(titleScreenScript.loadTitleScreen());
    }

    void shutDown(){
        confirmExitButton.onClick.RemoveAllListeners(); 
        confirmExitScreen.SetActive(true);
        confirmExitButton.onClick.AddListener(() => Application.Quit());
    }

    void confirmLogOut(){
        confirmExitButton.onClick.RemoveAllListeners(); 
        confirmExitScreen.SetActive(true);
        confirmExitButton.onClick.AddListener(logOut);  
    }
}
