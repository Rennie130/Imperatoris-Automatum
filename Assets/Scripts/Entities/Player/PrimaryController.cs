using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PrimaryController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Jump Assist")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    float coyoteTimer;
    float jumpBufferTimer;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Landing")]
    public float landingRecoveryTime = 0.3f;
    private float landingTimer;
    public bool hasLeftGround;
    private bool isLanding;

    [Header("Movement Smoothing")]
    public float acceleration = 30f;
    public float deceleration = 45f;
    private Vector3 currentMoveVelocity;

    private Rigidbody rb;
    private Transform cameraTransform;
    private HealthBase playerHealthBase;

    private bool isGrounded;
    private bool canMove = true;

    [SerializeField] private CharacterAnimationController animationController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        rb.freezeRotation = true; //prevent tipping

        playerHealthBase = GetComponent<HealthBase>();
        //Assign script reference to Game Manager
        GameManager.Instance.playerHealth = playerHealthBase;

        if (animationController == null)
        {
            animationController = GetComponent<CharacterAnimationController>();
        }
    }

    void Update()
    {
        // Disable all player movement while controlling mech
        if (!canMove) return;

        CheckGrounded();
        HandleJump();
    }

    void FixedUpdate()
    {
        //locked in operator/mech mode
        if (!canMove)
        {
            SendControlSignal(); //still controlling secondary
            rb.velocity = Vector3.up * rb.velocity.y;
            return;
        }

        // Temporarily stop horizontal movement during landing recovery
        if (isLanding)
        {
            landingTimer -= Time.fixedDeltaTime;

            Debug.Log("Landing timer: " + landingTimer);

            if (landingTimer <= 0f)
            {
                landingTimer = 0f;
                isLanding = false;
            }
        }

        Move();
        ApplyGravity();
    }

    //stable grounded check
    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (animationController != null)
        {
            animationController.SetGrounded(isGrounded);
        }

        if (isGrounded)
        {
                coyoteTimer = coyoteTime;
                jumpCount = 0; //instant reset
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    //standard movement
    void Move()
    { 
        if (isLanding)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * v + right * h;
        move = Vector3.ClampMagnitude(move, 1f);

    //    Vector3 velocity = move * moveSpeed;
    //    velocity.y = rb.velocity.y;
//
    //    rb.velocity = velocity;

        Vector3 targetVelocity = move * moveSpeed;

        bool changingDirection = Vector3.Dot(currentMoveVelocity, targetVelocity) > 0;

        float speedChange;

        if (move.magnitude < 0.1f)
        {
            speedChange = deceleration;
        }
        else if (changingDirection)
        {
            speedChange = acceleration * 2f;
        }
        else
        {
            speedChange = acceleration;
        }

        currentMoveVelocity = Vector3.MoveTowards(currentMoveVelocity, targetVelocity, speedChange * Time.fixedDeltaTime);

        rb.velocity = new Vector3(currentMoveVelocity.x, rb.velocity.y, currentMoveVelocity.z);

        if (move.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

    }

    void HandleJump()
    {
        //buffer jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        
        //perform jump if allowed
        if (jumpBufferTimer > 0 && (coyoteTimer > 0 || jumpCount < maxJumps))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            hasLeftGround = true;

            if (animationController != null)
            {
                animationController.PlayJump();
            }
            else
            {
                animationController.PlayDoubleJump();
            }
            
            jumpCount++;
            jumpBufferTimer = 0;

            isLanding = false;
            landingTimer = 0f;
            hasLeftGround = true;
        }
    }

    void ApplyGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void SendControlSignal()
    {
        GameModeManager.Instance.secondaryController.ReceiveInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public void EnableMovement(bool value)
    {
        canMove = value;

        if (!value)
        {
            rb.velocity = Vector3.zero; //stop instantly
        }
    }   

    public void OnPlayerDeath()
    {
        GameManager.Instance.GameOver();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only detect landing after the character has jumped
        if (!hasLeftGround || isLanding)
        {
            return;
        }

        // Check whether the collision is with a surface underneath
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isLanding = true;
                landingTimer = landingRecoveryTime;
                hasLeftGround = false;
                break;
            }
        }
    }
}