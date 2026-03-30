using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other) 
    {
        Debug.Log("player collided");
        if(other.gameObject.GetComponent<SecondaryController>())
        {
            var mechHealth = other.gameObject.GetComponent<Health>();
            mechHealth.Hurt(1);
            Debug.Log("player found");
        }
    }
}
