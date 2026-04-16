using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatBase : MonoBehaviour
{
    [Header("Attack")]
    public int damage = 2;
    public float attackRange = 2f;
    public float attackRadius = 1.5f;

    [Header("Timing")]
    public float windUpTime = 0.3f;
    public float recoveryTime = 0.5f;
    public float attackCooldown = 1f;

    protected bool isAttacking;
    protected float lastAttackTime;

    public void TryAttack(Transform target)
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > attackRange + attackRadius)
            return;
        
        StartCoroutine(AttackRoutine(target));

        Debug.Log("Trying to attack...");
    }

    IEnumerator AttackRoutine(Transform target)
    {
        isAttacking = true;

        //Wind-Up
        OnWindUp();
        yield return new WaitForSeconds(windUpTime);

        //Hit Frame
        PerformHit();

        //Recovery
        yield return new WaitForSeconds(recoveryTime);

        lastAttackTime = Time.time;
        isAttacking = false;

        Debug.Log("Attack started");
    }

    void PerformHit()
    {
        //Define attack origin slightly forward
        Vector3 origin = transform.position + Vector3.up * 1.0f;

        //Direction forward
        Vector3 forward = transform.forward;

        //Hit point in front
        Vector3 hitPoint = origin + forward * attackRange;

        //Detect everything in range
        Collider[] hits = Physics.OverlapSphere(hitPoint, attackRadius);

        foreach (Collider col in hits)
        {
            if (col.transform == transform) continue;

            Health hp = col.GetComponent<Health>();

            if (hp != null)
            {
                hp.Hurt(damage);
                OnHit(col.transform);
            }
        }

    }
    
    //Hooks for customisation
    protected virtual void OnWindUp()
    {
        
    }

    protected virtual void OnHit(Transform hitTarget)
    {
        
    }
}
