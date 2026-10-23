using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseParallaxCamera : MonoBehaviour
{
    [SerializeField] private float maxOffset = 0.2f;
    [SerializeField] private float smoothness = 5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Vector3 mouse = Input.mousePosition;

        float x = (mouse.x / Screen.width - 0.5f) * 2;
        float y = (mouse.y / Screen.height - 0.5f) * 2;

        Vector3 target = startPos + new Vector3(x * maxOffset, y * maxOffset, 0);

        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * smoothness);
    }
}
