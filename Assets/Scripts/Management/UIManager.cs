using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Scene UI")]
    public UpdateUIStats levelUI;
    public GameObject mainMenuUI; // is the Senate Scene UI for now
    public GameObject pauseMenuUI; 
    public GameObject gameOverUI;
    public GameObject levelCompletedUI;

    public static bool gameIsPaused = false;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        //pauses the game when ESC is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!gameIsPaused)
            {
                Pause();
            }
        }
        
        //locks/unlocks mouse cursor
        if(mainMenuUI.activeInHierarchy || pauseMenuUI.activeInHierarchy || gameOverUI.activeInHierarchy || levelCompletedUI.activeInHierarchy)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ToggleLevelUI()
    {
        pauseMenuUI.SetActive(false);
        mainMenuUI.SetActive(false);
        gameOverUI.SetActive(false);
        levelCompletedUI.SetActive(false);
        levelUI.gameObject.SetActive(true);
    }

    public void ToggleMainMenuUI()
    {
        pauseMenuUI.SetActive(false);
        mainMenuUI.SetActive(true); //for now, it's the Senate Scene and UI acting as "Main Menu"
        gameOverUI.SetActive(false);
        levelCompletedUI.SetActive(false);
        levelUI.gameObject.SetActive(false);
    }

    public void ToggleGameOverUI()
    {
        gameOverUI.SetActive(true);
    }

    public void ToggleLevelCompleteUI()
    {
        levelCompletedUI.SetActive(true); 
        //Once there are more levels, should add information/stats about level and button to proceed to next level.
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }


    //pauses the game
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }
}
