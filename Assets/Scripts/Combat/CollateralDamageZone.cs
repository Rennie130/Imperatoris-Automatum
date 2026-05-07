using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollateralDamageZone : MonoBehaviour
{
    public int damage = 1;
    public float damageCooldown = 1f;

    float timer;

    private void OnTriggerStay(Collider other)
    {
        timer -= Time.deltaTime;

        if (timer > 0)
        {
            return;
        }

        PrimaryController player = other.GetComponent<PrimaryController>();

        if (player == null)
        {
            return;
        }

        Damageable dmg = other.GetComponent<Damageable>() ?? other.GetComponentInParent<Damageable>();

        if (dmg != null)
        {
            dmg.Hurt(damage, transform);
            timer = damageCooldown;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;

            rb.AddForce(dir * 5f, ForceMode.Impulse);
        }
    }
}
