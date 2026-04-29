using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour, Damageable
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    public Action OnDeath; // Used by MissionManager
    public Action<Transform> OnDamaged;

    Rigidbody rb;

    public bool IsAlive => currentHealth > 0;

      void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Debug.Log($"[HEALTH ACTIVE] {name} HP: {currentHealth}");
    }

    // Function to call on individual combat scripts to deal damage (reduce other object's current health value)
    public void Hurt(int damage, Transform attacker)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log($"[DAMAGE] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        OnDamaged?.Invoke(attacker);

        if (currentHealth <= 0)
        {
            Debug.Log($"[DEATH] {name} died");
            Die();
        }
            
    }

    private void Die()
    {
    
        OnDeath?.Invoke();
            
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        gameObject.layer = LayerMask.NameToLayer("Dead");

        Debug.Log($"{name} Died ({currentHealth})");
        
    }
}