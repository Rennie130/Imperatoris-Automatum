using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SecondaryController : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 100f;
    
    CharacterController controller;
    bool canControl = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!canControl) return;

        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");

        //Rotation
        transform.Rotate(Vector3.up * rotate * rotationSpeed * Time.deltaTime);

        //Forward / Backward Movement
        Vector3 forwardMove = transform.forward * move;
        controller.Move(forwardMove * moveSpeed * Time.deltaTime);
    }

    public void EnableControl(bool value)
    {
        canControl = value;
    }
}
