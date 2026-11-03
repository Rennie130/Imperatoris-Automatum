using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [Header("Animation Settings")]
    [SerializeField] private float runThreshold = 0.15f;
    [SerializeField] private float accelerationDamp = 0.03f;
    [SerializeField] private float decelerationDamp = 0.02f;

    private Vector3 previousPosition;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();

            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }
    }

    private void Update()
    {
        if (animator == null || rb == null)
        {
            return;
        }

        // Measure actual horizontal movement.
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        // Ignore tiny movements.
        if (speed < runThreshold)
            speed = 0f;
        
        float damp = speed > animator.GetFloat("Speed") ? accelerationDamp : decelerationDamp;

        animator.SetFloat("Speed", speed, damp, Time.deltaTime);
    }

    public void PlayJump()
    {
        animator.SetTrigger("Jump");
    }

    public void SetGrounded(bool grounded)
    {
        animator.SetBool("Grounded", grounded);
    }

    public void PlayDoubleJump()
    {
        if (animator != null)
        {
            animator.SetTrigger("DoubleJump");
        }
    }
}
