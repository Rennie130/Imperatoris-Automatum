using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : HealthBase
{
    [Header("Targeting")]
    public Transform targetPoint;

    public Transform GetTargetPoint()
    {
        return targetPoint != null ? targetPoint : transform;
    }

    protected override void Awake()
    {
        base.Awake();
        
    }

    private void Start() {
        DistrictManager.Instance?.RegisterBuilding(this);
    }

    public override void Hurt(int damage, Transform attacker)
    {
        base.Hurt(damage, attacker);

        // Building-specific
        DistrictManager.Instance?.RecalculateDistrictHealth();
    }

    protected override void Die()
    {
        Debug.Log($"[BUILDING DESTROYED] {name}");

        DistrictManager.Instance?.UnregisterBuilding(this);

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Dead");

        StartCoroutine(DestroyRoutine());
    }

    IEnumerator DestroyRoutine()
    {
        // Disable targeting
        enabled = false;

        // Disable children
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Disable rigidbody physics
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Disable new obstacles
        UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();

        if (obstacle != null)
        {
            obstacle.enabled = false;
        }

        // Change layer
        gameObject.layer = LayerMask.NameToLayer("Dead");

        Debug.Log($"[BUILDING DESTROYED] {name}");

        // Wait before despawn
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }

}
