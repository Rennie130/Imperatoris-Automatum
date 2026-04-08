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

    private Rigidbody rb;
    private Transform cameraTransform;

    private bool isGrounded;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;

        rb.freezeRotation = true; //prevent tipping
    }

    void Update()
    {
        CheckGrounded();
        HandleJump();
    }

    void FixedUpdate()
    {
        //locked in operator/mech mode
        if (!canMove)
        {
            SendControlSignal(); //still controlling secondary
            return;
        }

        Move();
        ApplyGravity();
    }

    //stable grounded check
    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

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

        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.velocity.y;

        rb.velocity = velocity;

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
            
            jumpCount++;
            jumpBufferTimer = 0;
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
}