using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PrimaryController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    public int maxJumps = 2; //total jumps
    private int jumpCount = 0; //tracks jumpage

    public float groundedThreshold = 0.05f; //buffer for grounded detection

    private CharacterController controller;
    private Transform cameraTransform;

    Vector3 velocity;
    bool isGrounded;
    bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        //check if grounded
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        //second person mode + look at secondary only
        if (GameModeManager.Instance.CurrentMode == GameMode.SecondPerson)
        {
            LookAtSecondary();
            ApplyGravity();
            return;
        }

        //variable jump height
        if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            velocity.y *= 0.5f;
        }

        if (!canMove) return;

        MoveThirdPerson();
        HandleJump();
        ApplyGravity();
    }

    void MoveThirdPerson()
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
    
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveSpeed * Time.deltaTime * move);
    }

    void HandleJump()
    {
        //force grounded stability
        if (controller.isGrounded)
        {
            jumpCount = 0;

            if (velocity.y <0)
            {
                velocity.y = -2f; //stick to ground
            }
        }
        //reset jump input when grounded
       // if (controller.isGrounded && velocity.y <= groundedThreshold)
      //  {
        //    jumpCount = 0;
      //      velocity.y = 2f; //keep grounded stable
      //  }

        //jump input
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity *  Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAtSecondary()
    {
        Transform secondary = GameModeManager.Instance.secondaryController.transform;

        Vector3 direction = secondary.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void EnableMovement(bool value)
    {
        canMove = value;
    }   
}