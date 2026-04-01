using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SecondaryController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 100f;
    
    private Rigidbody rb;

    [Header("Acceleration")]
    public float acceleration = 8f;
    public float deceleration = 10f;

    float currentSpeed = 0f;

    float inputH;
    float inputV;

    bool canControl = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
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
        float forwardInput = inputV;
        float turnInput = inputH;
        
        float targetSpeed = forwardInput * moveSpeed * signal;
        float targetTurn = turnInput * rotationSpeed * signal;
    
        //smooth acceleration / deceleration
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (Mathf.Abs(forwardInput) > 0.1f ? acceleration : deceleration) * Time.fixedDeltaTime);

        float turnAccel = (Mathf.Abs(inputH) > 0.1f) ? acceleration : deceleration * 2f;
        //currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, targetTurn, turnAccel * Time.fixedDeltaTime);

        //tank rotation
        float turnAmount = inputH * rotationSpeed * signal * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAmount, 0));

        //forward/backward movement
        Vector3 velocity = transform.forward * currentSpeed;

        //preserve gravity
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;
    }

    public void EnableControl(bool value)
    {
        canControl = value;
    }
}
