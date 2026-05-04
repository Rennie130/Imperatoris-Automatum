using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    public Transform mech;

    public static GameManager Instance {get; private set; }

    private void Awake() 
    {
        if(Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
            DontDestroyOnLoad(gameObject);

    }

    // Start is called before the first frame update
    void Start()
    {
        MainMenu();

    }

    public void NewGame()
    {
        LoadDistrictOne();

        //Ensure game isn't paused
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;
    }

    public void LoadDistrictOne()
    {
        SceneManager.LoadSceneAsync(1);
        uiManager.ToggleLevelUI();
    }

       //Not using yet but set up for when we have more scenes. 
    // public void LoadNextLevelScene()
    // {
    //     //call scene which is next scene after current scene in the build index
    //     SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    // }

    public void GameOver()
    {
        uiManager.ToggleGameOverUI();

        //pause game when game over
        Time.timeScale = 0f;
        UIManager.gameIsPaused = true;
    }

    public void LevelCompleted()
    {
        uiManager.ToggleLevelCompleteUI();

        //pause game when level completed.
        Time.timeScale = 0f;
        UIManager.gameIsPaused = true;
        //Once there are more levels, can then call a LoadNextLevel() method (isn't created yet).
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        UnityEngine.Debug.Log("Restart");
    }

    public void MainMenu()
    {
        SceneManager.LoadSceneAsync("SenateScene");
        uiManager.ToggleMainMenuUI();
        UnityEngine.Debug.Log("Senate Scene");
    }

    public void Quit()
    {
        Application.Quit();

        UnityEngine.Debug.Log("Quit Game");
    }
}
