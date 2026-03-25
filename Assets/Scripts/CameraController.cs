using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
    public Transform primaryTarget;
    public Transform secondaryTarget;

    [Header("Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 200f;
    public float followSpeed = 10f;

    [Header("Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionMask;

    float yaw;
    float pitch;

    bool firstPersonMode = false;

    void LateUpdate()
    {
        if (firstPersonMode)
        {
            FirstPersonUpdate();
        }
        else
        {
            ThirdPersonUpdate();
        }
    }

    void ThirdPersonUpdate()
    {
        //mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
         //clamp vertical look
         pitch = Mathf.Clamp(pitch, -40f, 80f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        //desired camera position
        Vector3 desiredPosition = primaryTarget.position + rotation * thirdPersonOffset;

        //camera collision check
        Vector3 direction = desiredPosition - primaryTarget.position;
        if (Physics.SphereCast(primaryTarget.position, collisionRadius, direction.normalized, out RaycastHit hit, direction.magnitude, collisionMask))
        {
            //pull camera closer if blocked
            desiredPosition = hit.point;
        }

        //smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        //always look at player
        transform.LookAt(primaryTarget.position + Vector3.up * 1.5f);
    }

    void FirstPersonUpdate()
    {
        //camera at head
        Vector3 targetPos = primaryTarget.position + Vector3.up * 1.6f;
        //smooth transition into head
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        //match rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, primaryTarget.rotation, followSpeed *Time.deltaTime);
    }

    public void SwitchToThirdPerson()
    {
        firstPersonMode = false;

        //reset angles for smoother transition
        yaw = primaryTarget.eulerAngles.y;
        pitch = 10f;
    }

    public void SwitchToFirstPerson()
    {
        firstPersonMode = true;
    }

}
