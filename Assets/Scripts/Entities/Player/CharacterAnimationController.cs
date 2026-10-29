using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Animation Settings")]
    [SerializeField] private float runThreshold = 0.1f;
    [SerializeField] private float dampTime = 0.1f;

    private Vector3 previousPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();

            previousPosition = transform.position;
        }
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        // Measure actual horizontal movement.
        Vector3 movement = transform.position - previousPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        previousPosition = transform.position;

        // Ignore tiny movements.
        if (speed < runThreshold)
            speed = 0f;

        animator.SetFloat("Speed", speed, dampTime, Time.deltaTime);
    }
}
