using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, Damageable
{

    public int maxHealth = 20;
    int currentHealth; // should this be a public or private variable?

    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    public System.Action<Transform> OnDamaged;

    [Header("Targeting")]
    public Transform targetPoint;

    [SerializeField] Transform[] attackPoints;

    public Transform GetTargetPoint()
    {
        return targetPoint != null ? targetPoint : transform;
    }

    void Start()
    {
        currentHealth = maxHealth;
        DistrictManager.Instance?.RegisterBuilding(this);
    }

    public void Hurt(int damage, Transform attacker)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        Debug.Log($"[Building HIT] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        OnDamaged?.Invoke(attacker);

        DistrictManager.Instance?.RecalculateDistrictHealth();

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }

    void DestroyBuilding()
    {
        Debug.Log($"[BUILDING DESTROYED] {name}");

        DistrictManager.Instance?.UnregisterBuilding(this);

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        gameObject.layer = LayerMask.NameToLayer("Dead");

        //Delay destroy slightly (gives enemy time to react)
        Destroy(gameObject, 0.1f); //should we make this SetActive = false instead so it's just disabled/gone from view but the enemy isn't throwing out null errors.
    }

}
