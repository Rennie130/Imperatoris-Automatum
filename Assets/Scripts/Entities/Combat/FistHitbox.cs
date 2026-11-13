using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistHitbox : MonoBehaviour
{
    private CombatBase owner;

    private bool hitboxActive;


    // =======================================================
    //    INITIALISATION
    // =======================================================

    public void Initialise(CombatBase combat)
    {
        owner = combat;
    }

    // =======================================================
    //    HITBOX STATE
    // =======================================================

    public void SetHitboxActive(bool active)
    {
        hitboxActive = active;
    }

    public bool IsActive()
    {
        return hitboxActive;
    }
    
    // =======================================================
    //    COLLISION
    // =======================================================
    
    private void OnTriggerEnter(Collider other)
    {
        if (!hitboxActive)
        {
            return;
        }

        if (owner == null)
        {
            return;
        }
    

        // -------------------------------------------------------
        //    Don't Hit Self
        // -------------------------------------------------------
    
        if (other.transform.root == owner.transform.root)
        {
            return;
        }

        Debug.Log("[FIST COLLISION] Hit: " + other.name);

        // ------------------------------------------------------
        //    Find Damageable
        // ------------------------------------------------------
        Damageable damageable = null;

        MonoBehaviour[] components = other.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour component in components)
        {
            if (component is Damageable foundDamageable)
            {
                damageable = foundDamageable;
                break;
            }
        }

        if (damageable == null)
        {
            Debug.LogWarning("[FIST] No Damageable found on " + other.name);
            return;
        }

        // -------------------------------------------------------
        //    Send Hit to Combat System
        // -------------------------------------------------------

        owner.RegisterFistHit(damageable, other);
    }
}