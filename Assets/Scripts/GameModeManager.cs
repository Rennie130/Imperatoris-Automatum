using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    [Header("References")]
    public PrimaryController primaryController;
    public SecondaryController secondaryController;
    public CameraController cameraController;

    [Header("Signal System")]
    public float maxSignalDistance = 25f;
    public float minSignalDistance = 5f;

    [HideInInspector]
    public float signalStrength = 1f;

    void Awake()
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

        //update signal only in second person
        if (CurrentMode == GameMode.SecondPerson)
        {
            UpdateSignal();
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
            cameraController.SwitchToSecondPerson();
        }

    }

    //calculate signal strength based on distance
    void UpdateSignal()
    {
        float distance = Vector3.Distance(primaryController.transform.position, secondaryController.transform.position);
        signalStrength = Mathf.InverseLerp(maxSignalDistance, minSignalDistance, distance);

        /*
        if (secondaryController != null)
        {
            
        }
        else
        {
            UnityEngine.Debug.Log("Mech destroyed, no secondary controller script accessible.");
        }
        */
    }

    //debug line in scene view
    void OnDrawGizmos()
    {
        if (primaryController == null || secondaryController == null) return;

        Gizmos.color = Color.Lerp(Color.red, Color.green, signalStrength);

        Gizmos.DrawLine(primaryController.transform.position, secondaryController.transform.position);
    }
}
