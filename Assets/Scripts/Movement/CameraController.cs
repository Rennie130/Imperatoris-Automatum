using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform primaryTarget;
    public Transform secondaryTarget;

    [Header("Third Person")]
    //public float positionSnapSpeed = 8f;
    //public float rotationSpeed = 2f;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float followSmooth = 8f;

    [Header("Mouse Control")]
    public float mouseSensitivity = 3f;
    public float minY = -30f;
    public float maxY = 60f;

    float yaw;
    float pitch;

    [Header("Auto Align")]
    public float autoAlignSpeed = 2f;
    public bool autoAlign = true;

    [Header("Collision")]
    public LayerMask cameraCollisionMask;
    public float collisionOffset = 0.3f;

    void Start()
    {
        if (primaryTarget != null)
        {
            yaw = primaryTarget.eulerAngles.y;
        }
        pitch = 10f;
    }

    void LateUpdate()
    {
        switch (GameModeManager.Instance.CurrentMode)
        {
            case GameMode.ThirdPerson:
                ThirdPersonUpdate();
                break;
                
            case GameMode.SecondPerson:
                SecondPersonUpdate();
                break;
        }
    }

    void ThirdPersonUpdate()
    {
        if (primaryTarget == null) return;

        Transform target = primaryTarget;

        //mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minY, maxY);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

       //desired position
       Vector3 offset = rotation * new Vector3(0, cameraHeight, -cameraDistance);
       Vector3 desiredPosition = target.position + offset;

        //collision
        RaycastHit hit;

       if (Physics.Linecast(target.position + Vector3.up * 1.5f, desiredPosition, out hit, cameraCollisionMask))
        {
            desiredPosition = hit.point + hit.normal * collisionOffset;
        }

       //smooth but slightly stiff ps2 feel
       transform.position = Vector3.Lerp(transform.position, desiredPosition, 8f * Time.deltaTime);

       //look at player
       transform.LookAt(target.position + Vector3.up * 1.5f);
       
        //auto-align behind player
       if (autoAlign && Mathf.Abs(Input.GetAxis("Mouse X")) < 0.1f)
        {
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, autoAlignSpeed * Time.deltaTime);
        }
    }

    void SecondPersonUpdate()
    {
        if (primaryTarget == null || secondaryTarget == null) return;

        Transform secondary = secondaryTarget;
        Transform primary = primaryTarget;

        //first-person position (head)
        Vector3 headOffset = Vector3.up * 1.6f;
        Vector3 headPosition = primary.position + headOffset;

        //prevent clipping
        if (Physics.CheckSphere(headPosition, 0.2f))
        {
            headPosition += Vector3.up * 0.5f; //push up slightly
        }

        transform.position = headPosition;

       //always look at secondary
        Vector3 lookTarget = secondary.position + Vector3.up * 2f;
        Vector3 direction = (lookTarget - transform.position).normalized;
       
       Quaternion targetRotation = Quaternion.LookRotation(direction);

       transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 6f * Time.deltaTime);
       
    }
}
