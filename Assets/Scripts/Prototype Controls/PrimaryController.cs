using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PrimaryController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private CharacterController controller;
    private Transform cameraTransform;

    bool canMove = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.SecondPerson)
        {
            LookAtSecondary();
            return;
        }

        if (!canMove) return;

        MoveThirdPerson();
    }

    void MoveThirdPerson()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        Vector3 move = cameraTransform.forward * v + cameraTransform.right * h;
        move.y = 0f;
    
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveSpeed * Time.deltaTime * move);
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