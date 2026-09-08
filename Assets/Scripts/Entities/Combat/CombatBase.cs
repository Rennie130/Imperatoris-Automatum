using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class CombatBase : MonoBehaviour
{
    [System.Serializable]
    public struct HitStopData
    {
        public float freezeTime;
        public float timeScale;
    }

    [Header("Attack Settings")]
    [SerializeField] float attackRadius = 2.5f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask damageMask;

    public float AttackRange => attackRange;
    public bool CanBeInterrupted => true;

    [Header("Timing")]
    [SerializeField] public float windUpTime = 0.3f;
    [SerializeField] public float hitDelay = 0.1f;
    [SerializeField] public float recoveryTime = 0.4f;

    [Header("Combo")]
    [SerializeField] int maxCombo = 3;
    [SerializeField] float comboResetTime = 1f;
    
    [Header("Lunge")]
    [SerializeField] float lungeForce = 5f;
    [SerializeField] float lungeDuration = 0.12f;

    [Header("Hit Stop")]
    [SerializeField] protected HitStopData normalHit;
    [SerializeField] protected HitStopData heavyHit;
    [SerializeField] protected HitStopData finisherHit;

    [Header("Combat Points")]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected Transform hitCentre;
    
    protected bool isAttacking;
    protected bool queuedNextAttack;
    void QueueNextCombo()
    {
        queuedNextAttack = true;
    }

    protected int comboStep;
    protected float comboTimer;
    protected float lastAttackTime;
    protected NavMeshAgent agent;
    public bool navigationLocked;

    public bool IsAttacking => isAttacking;
   
    protected Rigidbody rb;
    protected Animator animator;

    Coroutine attackRoutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();

        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        // Reset combo if player waits too long
        if (!isAttacking && comboStep > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0)
            {
                comboStep = 0;
            }
        }
    }

/// =====================
///     ATTACK ENTRY
/// =====================

    public virtual void TryAttack()
    {
        //Debug.Log("[COMBAT] TryAttack called");

        //Prevent attack spam
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("[BLOCKED] {name} on cooldown");
            return;
        }
        
        //Debug.Log("[TRY ATTACK CALLED]");

        //prevent mech attacking in wrong mode
        if (this is MechCombat)
        {
            if (!GameModeManager.Instance.IsControllingSecondary())
            {
                Debug.Log("[BLOCKED] {name} not in control mode");
                return;
            }
               
        }

        //Queue combo cont.
        if (isAttacking)
        {
            Debug.Log("[BLOCKED {name} already attacking]");
            QueueNextCombo();
            return;
        }

        Debug.Log("[ATTACK START] {name} ({GetType().Name})");
        
        attackRoutine = StartCoroutine(AttackRoutine());

    }  

    /// =========================
    ///     MAIN ATTACK LOOP
    /// =========================

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float signalMultiplier = GetSignalMultiplier();

        Debug.Log($"[WINDUP] {name}");
        // Wind-Up (telegraph)
        OnWindUp();

        // Trigger animation
        PlayAttackAnimation();

        // Wait for animation to finish
        yield return new WaitForSeconds(recoveryTime);
        
        // Small forward movement
       // PerformLunge();
        
      //  yield return new WaitForSeconds(windUpTime / signalMultiplier);

      //  Debug.Log($"[HIT FRAME] {name}");
        // Small Delay Before Hit Frame
      //  yield return new WaitForSeconds(hitDelay / signalMultiplier);

        // Hit Frame
       // PerformHit();

        // Recovery
     //   yield return new WaitForSeconds(recoveryTime / signalMultiplier);

        lastAttackTime = Time.time;
        isAttacking = false;

        // Continue combo if queued
        if (queuedNextAttack)
        {
            queuedNextAttack = false;

            comboStep++;
            
            // Wrap combo back around
            if (comboStep >= maxCombo)
                comboStep = 0;

            attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            // Begin combo expiration timer
            comboTimer = comboResetTime;
        }

    }

    protected virtual void PlayAttackAnimation()
    {
        if (animator == null) return;

        animator.SetInteger("ComboStep", comboStep);
        animator.SetTrigger("Plap");
    }

    /// ======================
    ///     DAMAGE CHECK
    /// ======================

    protected virtual void PerformHit()
    {
        Debug.Log($"[PERFORM HIT CALLED] {name}");
        
        // Attack originates slightly in front
        Vector3 hitPoint = attackPoint != null ? attackPoint.position : transform.position + Vector3.up * 2f;
        
        //Detect everything in range
        Collider[] hits = Physics.OverlapSphere(hitPoint, attackRadius, damageMask);

        Debug.Log($"[HITS FOUND] {hits.Length}");

        foreach (Collider col in hits)
        {
            Debug.Log($"[HIT SOMETHING] {col.name}");

            // Prevents self-hit
            if (col.transform == transform) continue;

            Damageable dmg = col.GetComponent<Damageable>() ?? col.GetComponentInParent<Damageable>();

            if (dmg != null)
            {
                dmg.Hurt(GetComboDamage(), transform);

                // Trigger freeze-Frame
                HitStopData data = GetHitStopData();

                HitStopManager.Instance?.TriggerHitStop(data.freezeTime, data.timeScale);

                OnHit(col.transform);

                Debug.Log($"[HIT] {name} hit {col.name}");
            }
            
        }

    }

    /// =====================
    ///     COMBO DAMAGE
    /// =====================

    protected virtual int GetComboDamage()
    {
        // Combo finisher deals extra damage
        switch(comboStep)
        {
            case 0: return damage;
            case 1: return damage + 1;
            case 2: return damage + 2;
        }

        return damage;
    }

    protected virtual HitStopData GetHitStopData()
    {
        // Combo finisher
        if (comboStep >= maxCombo - 1)
        {
            return finisherHit;
        }

        // Middle combo hit
        if (comboStep == 1)
        {
            return heavyHit;
        }

        // First/basic hit
        return normalHit;
    }

    protected virtual float GetKnockbackForce()
    {
        if (comboStep >= maxCombo - 1)
        {
            return 2f;
        }

        if (comboStep == 1)
        {
            return 1f;
        }

        return 0.5f;
    }

    /// =================
    ///     LUNGE
    /// =================
    
    protected virtual void PerformLunge()
    {
        StartCoroutine(LungeRoutine());
    }

    // To avoid NavMesh instantly cancelling any movement
    IEnumerator LungeRoutine()
    {
        LockNavigation();

        float timer = 0f;

        Vector3 start = transform.position;

        Vector3 forward = attackPoint != null ? attackPoint.forward : transform.forward;

        Vector3 target = start + forward * lungeForce;

        while (timer < lungeDuration)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(start, target, timer / lungeDuration);

            yield return null;
        }

        UnlockNavigation();
    }

    protected virtual float GetLungeDistance()
    {
        switch(comboStep)
        {
            case 0: return 0.5f;
            case 1: return 0.8f;
            case 2: return 1.2f;
        }

        return 0.5f;
    }

    /// ======================
    ///     INTERRUPTION
    /// ======================
    
     public virtual void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        queuedNextAttack = false;

        comboStep = 0;

        isAttacking = false;
    }

    /// ===================
    ///     OTHER
    /// ===================
    
  float GetSignalMultiplier()
    {
        return 1f; // temporary until hook real system

        //replace with actual signal system later
        //float signal = signalSystem.Instance != null ? signalSystem.Instance.GetSignalStrength() : 1f;

        //clamp so it doesn't break gameplay
        //return Mathf.Clamp(signal, 0.5f, 1.5f);
    }

    public bool CanAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, damageMask);
        return hits.Length > 0;
    }

    public virtual void LockNavigation()
    {
        if (agent == null)
        {
            return;
        }

        navigationLocked = true;

        // Stop path movement
        agent.isStopped = true;

        // Prevent NavMesh overriding transform movement
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public virtual void UnlockNavigation()
    {
        if (agent == null)
        {
            return;
        }

        navigationLocked = false;

        // Re-sync NavMesh position
        agent.Warp(transform.position);

        agent.updatePosition = true;
        agent.updateRotation = true;

        agent.isStopped = false;
    }

    public void AnimationHitFrame()
    {
        PerformLunge();
        PerformHit();
    }

    protected virtual void OnWindUp()
    {
        
    }

    protected virtual void OnHit(Transform hitTarget)
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 hitPoint = transform.position + transform.forward * attackRadius;

        Gizmos.DrawWireSphere(hitPoint, attackRadius);
    }

}
