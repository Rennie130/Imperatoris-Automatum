using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour, Damageable
{

    public int maxHealth = 20;
    int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        DistrictManager.Instance?.RegisterBuilding(this);
    }

    public void Hurt(int damage, Transform attacker)
    {
        currentHealth -= damage;

        Debug.Log($"[Building HIT] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        DistrictManager.Instance?.ReportDamage(damage);

        if (currentHealth <= 0)
        {
            DistrictManager.Instance?.UnregisterBuilding(this);
            Destroy(gameObject);
        }
    }

    void DestroyBuilding()
    {
        Debug.Log($"[BUILDING DESTROYED] {name}");

        DistrictManager.Instance?.UnregisterBuilding(this);

        //optional: play VFX
        Destroy(gameObject);
    }

}
