using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class HealthBase : MonoBehaviour, Damageable, ITargetable
{
   [Header("Health")]
   public int maxHealth = 10;
   
   public int currentHealth = 10;
   //public int CurrentHealth => currentHealth;
   public bool IsAlive => currentHealth > 0;

   [Header("Hit Reaction")]
   [SerializeField] protected float hitKnockbackForce = 0.3f;
   [SerializeField] protected float knockbackDuration = 0.06f;
   [SerializeField] protected bool canBeInterrupted = true;

   public Action<Transform> OnDamaged;
   public Action OnDeath;

   protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    /// ==================
    ///     DAMAGE
    /// ==================

    public virtual void Hurt(int damage, Transform attacker)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log($"[DAMAGE] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        OnDamaged?.Invoke(attacker);

        // Interrupt attack if allowed
        CombatBase combat = GetComponent<CombatBase>();

        if (combat != null && canBeInterrupted)
        {
            combat.CancelAttack();
        }

        // Apply knockback
        ApplyHitReaction(attacker);

        // Death check
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// ========================
    ///     HIT REACTION
    /// =======================

    protected virtual void ApplyHitReaction(Transform attacker)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb == null || attacker == null)
        {
            return;
        }

        StartCoroutine(HitReactionRoutine(attacker));
    }

    IEnumerator HitReactionRoutine(Transform attacker)
    {
        CombatBase combat = GetComponent<CombatBase>();

        if (combat != null)
        {
            combat.LockNavigation();
        }

        float timer = 0f;

        Vector3 start = transform.position;

        Vector3 dir = (transform.position - attacker.position).normalized;

        dir.y = 0;

        Vector3 target = start + dir * hitKnockbackForce;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(start, target, timer / knockbackDuration);

            yield return null;
        }

        if (combat != null)
        {
            combat.UnlockNavigation();
        }
    }

    /// =================
    ///     DEATH
    /// =================

    public abstract Transform GetTargetPoint();
    
    protected abstract void Die();
}
