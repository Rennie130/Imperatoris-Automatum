using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : HealthBase
{
    [Header("Death Animation")]
    [SerializeField] private Animator deathAnimator;
    [SerializeField] private float deathAnimatorDuration = 2f;

    private PrimaryController primaryController;
    private SecondaryController secondaryController;
    private bool isDying;

    protected override void Awake()
    {
        base.Awake();

        primaryController = GetComponent<PrimaryController>();
        secondaryController = GetComponent<SecondaryController>();

        if (deathAnimator == null)
        {
            deathAnimator = GetComponentInChildren<Animator>();
        }
    }
    protected override void Die()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log($"[DEATH] {name} died");

        // Stop combat.
        CombatBase combat = GetComponent<CombatBase>();
        if (combat != null)
        {
            combat.CancelAttack();
        }

        // Disable player and mech controls.
        if (primaryController != null)
        {
            primaryController.EnableMovement(false);
        }

        if (secondaryController != null)
        {
            secondaryController.EnableControl(false);
        }

        // Disable enemy ai
        EnemyController ai = GetComponent<EnemyController>();
        if (ai != null)
        {
            ai.enabled = false;
        }

        // Stop NavMesh movement.
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Disable colliders.
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Stop physics movement.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Prevent dead from being targeted
        gameObject.layer = LayerMask.NameToLayer("Dead");


        // Remove from wave tracking
        if (primaryController == null && secondaryController == null)
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.aliveEnemies.Remove(gameObject);
            }
        }
        
        StartCoroutine(DeathRoutine());

    }

    private IEnumerator DeathRoutine()
    {
        if (deathAnimator != null)
        {
            deathAnimator.SetTrigger("Die");

            yield return new WaitForSeconds(deathAnimatorDuration);

            // Vulcani death.
            if (primaryController != null || secondaryController != null)
            {
                OnDeath?.Invoke();

                if (primaryController != null)
                {
                    primaryController.OnPlayerDeath();
                }

                if (secondaryController != null)
                {
                    GameManager.Instance.GameOver();
                }

                yield break;
            }

            // Enemy death.
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    public override Transform GetTargetPoint()
    {
        return transform;
    }

    // No duplicate damage logic
    // Only defines what happens on death
}