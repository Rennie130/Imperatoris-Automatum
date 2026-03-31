using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform primaryTarget;
    public Transform secondaryTarget;

    [Header("Third Person")]
    public Vector3 Offset = new Vector3(0, 2, -4);
    public float followSpeed = 5f;
    public float rotationSpeed = 120f;

    float yaw;
    float pitch;

    bool secondPersonMode = false;

    void Start()
    {
        yaw = primaryTarget.eulerAngles.y;
        pitch = 10f;
    }

    void LateUpdate()
    {
        if (secondPersonMode)
            SecondPersonUpdate();
        else
            ThirdPersonUpdate();
    }

    void ThirdPersonUpdate()
    {
        //mouse input
        yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
         //clamp vertical look
         pitch = Mathf.Clamp(pitch, -30f, 70f);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);

        //desired camera position
        Vector3 desired = primaryTarget.position + rot * Offset;

        //smooth movement
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        //always look at player
        Vector3 lookTarget = primaryTarget.position + Vector3.up * 1.5f;

        Quaternion lookRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, followSpeed * Time.deltaTime);
    }

    void SecondPersonUpdate()
    {
        Transform secondary = secondaryTarget;
        Transform primary = primaryTarget;
        
        //first-person position (head)
        Vector3 headPosition = primary.position + Vector3.up * 1.6f;

        transform.position = Vector3.Lerp(transform.position, headPosition, followSpeed * Time.deltaTime);

       //always look at secondary
       Vector3 lookTarget = secondary.position + Vector3.up * 2f;

       Vector3 direction = (lookTarget - transform.position).normalized;
       
       Vector3 flatDirection = direction;
       flatDirection.y = Mathf.Clamp(flatDirection.y, -0.5f, 0.7f);
       
       Quaternion targetRotation = Quaternion.LookRotation(flatDirection);

       //Quaternion targetRotation = Quaternion.LookRotation(direction);

       transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, (followSpeed * 0.5f) * Time.deltaTime);

       

       
    }

    public void SwitchToThirdPerson()
    {
        secondPersonMode = false;
    }

    public void SwitchToSecondPerson()
    {
        secondPersonMode = true;
    }

}
