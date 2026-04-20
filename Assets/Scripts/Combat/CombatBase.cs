using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatBase : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 2;
    public float attackRange = 2f;
    public float attackRadius = 1.5f;

    [Header("Timing")]
    public float windUpTime = 0.3f;
    public float hitDelay = 0.1f;
    public float recoveryTime = 0.4f;
    public float cooldown = 1f;

    protected bool isAttacking;
    protected float lastAttackTime;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TryAttack()
    {
        Debug.Log("[TRY ATTACK CALLED]");

        //prevent mech attacking in wrong mode
        if (this is MechCombat)
        {
            if (!GameModeManager.Instance.IsControllingSecondary())
            {
                Debug.Log($"[BLOCKED] {name} not in control mode");
                return;
            }
               
        }

        if (isAttacking)
        {
            Debug.Log($"[BLOCKED {name} already attacking]");
            return;
        }

        if (Time.time < lastAttackTime + cooldown)
        {
            Debug.Log($"[BLOCKED] {name} on cooldown");
            return;
        }

        Debug.Log($"[ATTACK START] {name} ({GetType().Name})");
        
        StartCoroutine(AttackRoutine());

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

        //Hit Detection
        PerformHit();

        //Recovery
        yield return new WaitForSeconds(recoveryTime / signalMultiplier);

        lastAttackTime = Time.time;
        isAttacking = false;

        Debug.Log($"[ATTACK END] {name}");

    }

    void PerformHit()
    {
        //Define attack origin slightly forward
        Vector3 origin = transform.position + Vector3.up;
        //Hit point in front
        Vector3 hitPoint = origin + transform.forward * attackRange;

        Debug.Log($"[PERFORM HIT CALLED] {name}");

        //Detect everything in range
        Collider[] hits = Physics.OverlapSphere(hitPoint, attackRadius);

        Debug.Log($"[HITS FOUND] {hits.Length}");

        Debug.Log($"[HIT CHECK] {name} checking {hits.Length} colliders");

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

    }
    
  float GetSignalMultiplier()
    {
        return 1f; // temporary until hook real system

        //replace with actual signal system later
        //float signal = signalSystem.Instance != null ? signalSystem.Instance.GetSignalStrength() : 1f;

        //clamp so it doesn't break gameplay
        //return Mathf.Clamp(signal, 0.5f, 1.5f);
    }

    protected virtual void OnWindUp()
    {
        
    }

    protected virtual void OnHit(Transform hitTarget)
    {
        
    }
}
