using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SecondaryController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 100f;
    
    private CharacterController controller;

    float inputH;
    float inputV;

    bool canControl = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canControl) return;

        float signal = GameModeManager.Instance.signalStrength;

        if (signal <= 0.05f)
            return; //lost signal

        HandleMovement(signal);
    }

    public void ReceiveInput(float h, float v)
    {
        inputH = h;
        inputV = v;
    }

    void HandleMovement(float signal)
    {
        float rot = rotationSpeed * signal;
        float speed = moveSpeed * signal;

        //tank rotation
        transform.Rotate(Vector3.up * inputH * rot * Time.deltaTime);

        //forward/backward movement
        Vector3 move = transform.forward * inputV;
        controller.Move(move * speed * Time.deltaTime);
    }

    public void EnableControl(bool value)
    {
        canControl = value;
    }
}
