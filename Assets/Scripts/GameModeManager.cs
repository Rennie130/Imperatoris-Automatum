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

    public GameMode currentMode = GameMode.ThirdPerson;

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
            if (currentMode == GameMode.ThirdPerson)
                SetMode(GameMode.SecondPerson);
            else
                SetMode(GameMode.ThirdPerson);
                ResetSecondaryState();
        }

        //update signal only in second person
        if (currentMode == GameMode.SecondPerson)
        {
            UpdateSignal();
        }  

    }

    void SetMode(GameMode mode)
    {
        currentMode = mode;

        if (mode == GameMode.ThirdPerson)
        {
            //enable player movement
            primaryController.EnableMovement(true);
            //disable secondary control
            secondaryController.EnableControl(false);
        }
        else
        {
            //freeze player
            primaryController.EnableMovement(false);
            //enable tank controls
            secondaryController.EnableControl(true);
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

    void ResetSecondaryState()
    {
        if (secondaryController == null) return;

        Rigidbody rb = secondaryController.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public bool IsControllingSecondary()
    {
        return currentMode == GameMode.SecondPerson;
    }
}
