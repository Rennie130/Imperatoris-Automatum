using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    [Header("Health")]
    public int maxHeatlh = 10;


    private int currentHealth = 10;

    // Start is called before the first frame update
    void Start()
    {
        // Set the object's starting health to it's max health.
        currentHealth = maxHeatlh;
    }

    // Function to call on individual combat scripts to deal damage (reduce other object's current health value)
    public void Hurt(int damage)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 dir = (transform.position - Camera.main.transform.position).normalized;
            rb.AddForce(dir * 3f, ForceMode.Impulse);
        }

       // GetComponent<Animator>()?.SetTrigger("Attack");

        //subtract damage value from current heatlh value and update current health variable with new value.
        currentHealth -= damage;
        Debug.Log($"{name} took damage ({damage}; current health = ({currentHealth}))");
        
        if(currentHealth <= 0)
        {
            HandleDeath();
            
            gameManager.gameOver();
        }
    }

    private void HandleDeath()
    {
        gameObject.SetActive(false);
        Debug.Log($"{name} Died ({currentHealth})");

    }
}
