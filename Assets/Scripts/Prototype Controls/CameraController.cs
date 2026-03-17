using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform primaryTarget;
    public Transform secondaryTarget;

    [Header("Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -5);
    public float mouseSensitivity = 200f;
    public float followSpeed = 10f;

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
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -40f, 80f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = primaryTarget.position + rotation * thirdPersonOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(primaryTarget.position + Vector3.up * 1.5f);
    }

    void FirstPersonUpdate()
    {
        //Sit at Primary Head
        transform.position = primaryTarget.position + Vector3.up * 1.6f;
        transform.rotation = primaryTarget.rotation;
    }

    public void SwitchToThirdPerson()
    {
        firstPersonMode = false;
        yaw = primaryTarget.eulerAngles.y;
        pitch = 10f;
    }

    public void SwitchToFirstPerson()
    {
        firstPersonMode = true;
    }
}
