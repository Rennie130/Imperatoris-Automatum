using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    ThirdPerson,
    SecondPerson
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public GameMode CurrentMode = GameMode.ThirdPerson;

    public PrimaryController primaryController;
    public SecondaryController secondaryController;
    public CameraController cameraController;

    public void Awake()
    {
        Instance = this;
    }

  void Start()
    {
        //start in third person
        SetMode(GameMode.ThirdPerson);
    }

    void Update()
    {
        //press TAB to toggle modes
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (CurrentMode == GameMode.ThirdPerson)
                SetMode(GameMode.SecondPerson);
            else
                SetMode(GameMode.ThirdPerson);
        }
    }

    void SetMode(GameMode mode)
    {
        CurrentMode = mode;

        if (mode == GameMode.ThirdPerson)
        {
            //enable player movement
            primaryController.EnableMovement(true);
            //disable secondary control
            secondaryController.EnableControl(false);
            //switch camera
            cameraController.SwitchToThirdPerson();
        }
        else
        {
            //freeze player
            primaryController.EnableMovement(false);
            //enable tank controls
            secondaryController.EnableControl(true);
            //switch camera
            cameraController.SwitchToFirstPerson();
        }

    }
}
