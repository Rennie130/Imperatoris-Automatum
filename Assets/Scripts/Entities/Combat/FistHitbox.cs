using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistHitbox : MonoBehaviour
{
    private CombatBase owner;
    private Collider fistCollider;
    private bool hitboxActive;


    // =======================================================
    //    INITIALISATION
    // =======================================================

    private void Awake()
    {
        fistCollider = GetComponent<Collider>();

        if (fistCollider == null)
        {
            Debug.LogError("[FIST] Missing Collider on " + name);
        }
    }

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

        // Check when punch becomes active
        if (active)
        {
            DetectHits();
        }
    }

    public bool IsActive()
    {
        return hitboxActive;
    }

    // =======================================================
    //    COLLISION
    // =======================================================

    private void FixedUpdate()
    {
        if (!hitboxActive)
        {
            return;
        }

        DetectHits();
    }

    private void DetectHits()
    {
        if (owner == null || fistCollider == null)
        {
            return;
        }

        if (!fistCollider.enabled)
        {
            return;
        }

        Bounds bounds = fistCollider.bounds;

        // Slightly expand detection area
        Vector3 detectionSize = bounds.extents * 1.5f;

        Collider[] hits = Physics.OverlapBox(bounds.center, detectionSize, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

        foreach (Collider other in hits)
        {
            ProcessHit(other);
        }
    }

    private void ProcessHit(Collider other)
    {
        if (other == null || owner == null)
            return;

        if (other.transform.root == owner.transform.root)
            return;

        Debug.Log("[FIST DEBUG] Detected collider: " + other.name);

        Damageable damageable = null;

        MonoBehaviour[] components =
            other.GetComponentsInParent<MonoBehaviour>();

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
            Debug.Log("[FIST DEBUG] No Damageable found on: " + other.name);
            return;
        }

        Debug.Log("[FIST DEBUG] Found Damageable on: " + other.name);

        owner.RegisterFistHit(damageable, other);
    }

    //private void ProcessHit(Collider other)
    //{
    //    if (other == null || owner == null)
    //    {
    //        return;
    //    }
//
    //    // Ignore own colliders
    //    if (other.transform.root == owner.transform.root)
    //    {
    //        return;
    //    }
//
    //    Damageable damageable = null;
//
    //    MonoBehaviour[] components = other.GetComponentsInParent<MonoBehaviour>();
//
    //    foreach (MonoBehaviour component in components)
    //    {
    //        if (component is Damageable foundDamageable)
    //        {
    //            damageable = foundDamageable;
    //            break;
    //        }
    //    }
//
    //    if (damageable == null)
    //    {
    //        owner.RegisterFistHit(damageable, other);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!hitboxActive)
        {
            return;
        }

        ProcessHit(other);
    }
    
}