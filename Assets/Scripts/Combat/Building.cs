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

        Destroy(gameObject, 0.1f);
    }

}
