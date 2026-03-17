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
        SetMode(GameMode.ThirdPerson);
    }

    void Update()
    {
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
            primaryController.EnableMovement(true);
            secondaryController.EnableControl(false);
            cameraController.SwitchToThirdPerson();
        }
        else
        {
            primaryController.EnableMovement(false);
            secondaryController.EnableControl(true);
            cameraController.SwitchToFirstPerson();
        }
    }
}
