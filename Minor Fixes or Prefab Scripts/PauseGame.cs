using UnityEngine;

public class PauseGame : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        Time.timeScale = 0f; // freezes the game
        gameObject.GetComponent<CanvasGroup>().alpha = 1;
        isPaused = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f; // resumes the game
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
        isPaused = false;
    }
}