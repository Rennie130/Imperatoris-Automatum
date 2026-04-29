using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatBase : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] float attackRadius = 1.5f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask damageMask;

    [Header("Timing")]
    public float windUpTime = 0.3f;
    public float hitDelay = 0.1f;
    public float recoveryTime = 0.4f;

    protected bool isAttacking;
    protected float lastAttackTime;
    protected Rigidbody rb;

    Coroutine attackRoutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void TryAttack()
    {
        Debug.Log("[COMBAT] TryAttack called");

        //Cooldown check
        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("[BLOCKED] {name} on cooldown");
            return;
        }
        
        Debug.Log("[TRY ATTACK CALLED]");

        //prevent mech attacking in wrong mode
        if (this is MechCombat)
        {
            if (!GameModeManager.Instance.IsControllingSecondary())
            {
                Debug.Log("[BLOCKED] {name} not in control mode");
                return;
            }
               
        }

        //Prevent overlap
        if (isAttacking)
        {
            Debug.Log("[BLOCKED {name} already attacking]");
            return;
        }

        Debug.Log("[ATTACK START] {name} ({GetType().Name})");
        
        attackRoutine = StartCoroutine(AttackRoutine());

    }  

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float signalMultiplier = GetSignalMultiplier();

        Debug.Log($"[WINDUP] {name}");
        //Wind-Up (telegraph)
        OnWindUp();
        yield return new WaitForSeconds(windUpTime / signalMultiplier);

        Debug.Log($"[HIT FRAME] {name}");
        //Small Delay Before Hit Frame
        yield return new WaitForSeconds(hitDelay / signalMultiplier);

        //Hit Frame
        PerformHit();

        //Recovery
        yield return new WaitForSeconds(recoveryTime / signalMultiplier);

        lastAttackTime = Time.time;
        isAttacking = false;
        attackRoutine = null;

        Debug.Log($"[ATTACK END] {name}");

    }

    void PerformHit()
    {
        Debug.Log($"[PERFORM HIT CALLED] {name}");
        
        //Vector3 origin = transform.position + Vector3.up;
        Vector3 hitPoint = transform.position;
        
        //Detect everything in range
        Collider[] hits = Physics.OverlapSphere(hitPoint, attackRadius, damageMask);

        Debug.Log($"[HITS FOUND] {hits.Length}");

        foreach (Collider col in hits)
        {
            Debug.Log($"[HIT SOMETHING] {col.name}");

            if (col.transform == transform) continue;

            Damageable dmg = col.GetComponentInParent<Damageable>();

            if (dmg != null)
            {
                Debug.Log($"[HIT] {name} hit {col.name}");

                dmg.Hurt(damage, transform);
                OnHit(col.transform);
            }
            
        }

        if (hits.Length == 0)
        {
            Debug.Log("[MISS] No targets in range");
        }

    }
    
  float GetSignalMultiplier()
    {
        return 1f; // temporary until hook real system

        //replace with actual signal system later
        //float signal = signalSystem.Instance != null ? signalSystem.Instance.GetSignalStrength() : 1f;

        //clamp so it doesn't break gameplay
        //return Mathf.Clamp(signal, 0.5f, 1.5f);
    }

    public virtual void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
    }

    public bool CanAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, damageMask);
        return hits.Length > 0;
    }

    protected virtual void OnWindUp()
    {
        
    }

    protected virtual void OnHit(Transform hitTarget)
    {
        
    }
}
