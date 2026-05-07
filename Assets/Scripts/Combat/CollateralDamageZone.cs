using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollateralDamageZone : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float damageInterval = 1f;
    [SerializeField] float pushForce = 5f;

    float timer;

    private void OnTriggerStay(Collider other)
    {
        timer -= Time.deltaTime;

        if (timer > 0)
        {
            return;
        }

        // Only affect Emperor
        PrimaryController player = other.GetComponent<PrimaryController>();

        if (player == null)
        {
            return;
        }

        Damageable dmg = other.GetComponent<Damageable>() ?? other.GetComponentInParent<Damageable>();

        if (dmg != null)
        {
            dmg.Hurt(damage, transform);

            Rigidbody rb = other.GetComponent<Rigidbody>();
        

            if (rb != null)
            {
                Vector3 dir = (other.transform.position - transform.position).normalized;

                rb.AddForce(dir * pushForce, ForceMode.Impulse);
            }

            timer = damageInterval;
        }
    }
}

