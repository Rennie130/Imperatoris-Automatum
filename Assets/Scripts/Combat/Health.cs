using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : HealthBase
{
    protected override void Die()
    {
        Debug.Log($"[DEATH] {name} died");

        OnDeath?.Invoke();

        // Disable AI

        var ai = GetComponent<EnemyController>();
        if (ai) ai.enabled = false;

        // Stop NavMesh
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Stop Combat
        var combat = GetComponent<CombatBase>();
        if (combat) combat.CancelAttack();

        // Disable Collider
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // Freeze Physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Dead");

        // Destroy after delay
        Destroy(gameObject, 2f);  
    }

    // No duplicate damage logic
    // Only defines what happens on death
}