using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour, Damageable
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    public Action OnDeath; // Used by MissionManager

   // [Header("Hit Reaction")]
//    public float staggerForce = 3f;
  //  public float knockbackForce = 8f;

    Rigidbody rb;

    //int hitCount = 0;
    //float lastHitTime;
   // public float comboResetTime = 1f;

    void Start()
    {
        Debug.Log($"[HEALTH ACTIVE] {name} HP: {currentHealth}");
    }

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    // Function to call on individual combat scripts to deal damage (reduce other object's current health value)
    public void Hurt(int damage, Transform attacker)
    {
        currentHealth -= damage;

        Debug.Log($"[DAMAGE] {name} took {damage} from {attacker?.name}. HP: {currentHealth}/{maxHealth}");

        //HitReaction(attacker);

        if (currentHealth <= 0)
        {
            Debug.Log($"[DEATH] {name} died");
            Die();
        }
            
    }

    private void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
            Debug.Log($"{name} Died ({currentHealth})");
        }
    }

    // void HitReaction(Transform attacker)
   // {
        //float timeSinceLastHit = Time.time - lastHitTime;

       // if (timeSinceLastHit > comboResetTime)
          //  hitCount = 0;
        
       // hitCount++;
       // lastHitTime = Time.time;

       // if (hitCount < 3)
       // {
        //    Stagger(attacker);
       // }
       // else
       // {
       //     Knockback(attacker);
      //      hitCount = 0;
      //  }
  //  }      

  //  void Stagger(Transform attacker)
  //  {
   //     if (rb == null || attacker == null) return;

  //      Vector3 dir = (transform.position - attacker.position).normalized;
  //      rb.AddForce(dir * staggerForce, ForceMode.Impulse);

  //      Debug.Log(name + " staggered");

        //optional: small movement interrupt
   // }

   // void Knockback(Transform attacker)
   // {
    //    if (rb == null || attacker == null) return;

   //     Vector3 dir = (transform.position - attacker.position). normalized;
   //     rb.AddForce(dir * knockbackForce, ForceMode.Impulse);

  //     Debug.Log(name + " knocked back");
  //  }

   