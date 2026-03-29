using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PrimaryController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

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
        //locked in operator/mech mode
        if (!canMove)
        {
            ApplyGravity();
            SendControlSignal(); //still controlling secondary
            return;
        }

        CheckGrounded();
        Move();
        HandleJump();
        ApplyGravity();
    }

    //stable grounded check
    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position + Vector3.down * (controller.height / 2f), 0.2f, LayerMask.GetMask("Ground"));

        if (isGrounded && velocity.y < 0)
        {
                velocity.y = -2f;
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

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (move.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed *Time.deltaTime);
        }

    }

    void HandleJump()
    {
      if (Input.GetButtonDown("Jump") && isGrounded)
      {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
      }
    }

    void ApplyGravity()
    {
        velocity.y += gravity *  Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void SendControlSignal()
    {
        GameModeManager.Instance.secondaryController.ReceiveInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public void EnableMovement(bool value)
    {
        canMove = value;
    }   
}