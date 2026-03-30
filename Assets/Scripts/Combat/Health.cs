using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private int currentHealth = 10;
    public int maxHeatlh = 10;

    // Start is called before the first frame update
    void Start()
    {
        // Set the object's starting health to it's max health.
        currentHealth = maxHeatlh;
    }

    // Function to call on individual combat scripts to deal damage (reduce other object's current health value)
    public void Hurt(int damage)
    {
        //subtract damage value from current heatlh value and update current health variable with new value.
        currentHealth -= damage;
        Debug.Log($"{name} took damage ({damage})");
        
        if(currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{name} Died");
    }
}
