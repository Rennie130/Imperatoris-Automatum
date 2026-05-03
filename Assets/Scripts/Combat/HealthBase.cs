using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class HealthBase : MonoBehaviour, Damageable
{
   [Header("Health")]
   public int maxHealth = 10;
   protected int currentHealth;

   public int CurrentHealth => currentHealth;
   public bool IsAlive => currentHealth > 0;

   public Action<Transform> OnDamaged;
   public Action OnDeath;

   protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void Hurt(int damage, Transform attacker)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log($"[DAMAGE] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        OnDamaged?.Invoke(attacker);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected abstract void Die();
}
