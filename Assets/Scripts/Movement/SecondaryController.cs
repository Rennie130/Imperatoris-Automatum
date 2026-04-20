using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SecondaryController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 100f;
    
    private Rigidbody rb;

    [Header("Inertia")]
    public float directionChangeResistance = 12f; //higher = more resistance
    public float skidFactor = 0.97f; //how much sideways motion persists

    Vector3 currentVelocity; //real movement velocity

    [Header("Acceleration")]
    public float acceleration = 8f;
    public float deceleration = 10f;

    float currentSpeed = 0f;

    float inputH;
    float inputV;

    bool canControl = false;

    MechCombat combat;
    Health healthPool;

    void Awake()
    {
        combat = GetComponent<MechCombat>();
        healthPool = GetComponent<Health>();
        healthPool.OnDeath += OnMechDeath;

        if (combat == null)
        {
            Debug.Log($"[ERROR] MechCombat missing on {name}");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.mass = 1000f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (!GameModeManager.Instance.IsControllingSecondary())
            return;
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

    private void OnMechDeath()
    {
        GameManager.Instance.GameOver();
    }

    void HandleMovement(float signal)
    {
        float forwardInput = inputV;
        float turnInput = inputH;

        //forward vs reverse speed
        float forwardSpeed = moveSpeed;
        float reverseSpeed = moveSpeed * 0.5f; //slower reverse

        float targetSpeed = 0f;

        if (Mathf.Abs(forwardInput) > 0.1f)
        {
            float direction = Mathf.Sign(forwardInput); // +1 or -1

            float speed = (direction > 0)
                ? forwardSpeed
                : reverseSpeed;


            targetSpeed = direction * speed * Mathf.Abs(forwardInput) * signal;
        }
    
        //smooth acceleration / deceleration
        float accelRate = (Mathf.Abs(forwardInput) > 0.1f) ? acceleration : deceleration;


        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.fixedDeltaTime);

        //turning logic
        float speedPercent = Mathf.Abs(currentSpeed) / moveSpeed;

        //turning multipliers
        float turnWhileMovingMultiplier = Mathf.Lerp(1f, 0.4f, speedPercent);
        float turnInPlaceMultiplier = 1.5f;

        float turnSpeed = 0f;

        if (Mathf.Abs(forwardInput) < 0.1f)
        {
            //turn in place
            turnSpeed = turnInput * rotationSpeed * turnInPlaceMultiplier * signal;
        }
        else
        {
            //turn while moving
            turnSpeed = turnInput * rotationSpeed * turnWhileMovingMultiplier * signal;
        }

        if (forwardInput < 0)
        {
            turnWhileMovingMultiplier *= 0.8f; //worse turning in reverse
        }

        //stopping "weight"
        if (Mathf.Abs(forwardInput) < 0.1f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * 1.5f * Time.fixedDeltaTime);
        }

        //apply rotation
        float turnAmount = turnSpeed * Time.fixedDeltaTime;

        //tank rotation
       // float turnAmount = inputH * rotationSpeed * signal * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turnAmount, 0));

        //forward/backward movement
        Vector3 velocity = transform.forward * currentSpeed;
        //preserve gravity
        velocity.y = rb.velocity.y;

        //desired velocity
        Vector3 desiredVelocity = transform.forward * currentSpeed;

        //inertia / skid
        //preserve vertical velocity
        desiredVelocity.y = rb.velocity.y;

        //detect sharp direction change
        float directionDot = Vector3.Dot(currentVelocity.normalized, desiredVelocity.normalized);

        //if reversing direction -> apply resistance
        if (directionChangeResistance < 0.2f)
        {
            desiredVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.fixedDeltaTime / directionChangeResistance);
        }

        //apply skid (keep some sideways movement)
        Vector3 lateral = Vector3.ProjectOnPlane(currentVelocity, transform.forward);

        currentVelocity = desiredVelocity + lateral * skidFactor;

        //apply to rigidbody
        rb.velocity = currentVelocity;
    }

    public void EnableControl(bool value)
    {
        canControl = value;
    }
}
