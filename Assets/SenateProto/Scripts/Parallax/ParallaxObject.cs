using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [SerializeField] float strength = 0.5f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float x = (Input.mousePosition.x / Screen.width - 0.5f) * 2;
        float y = (Input.mousePosition.y / Screen.height - 0.5f) * 2;

        transform.position = Vector3.Lerp(transform.position, startPos + new Vector3(x, y, 0) * strength, Time.deltaTime * 5f);
    }
}
