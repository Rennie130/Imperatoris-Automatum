using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WorldSpaceHealthBars : MonoBehaviour
{
    [Header("Enemy References")]
    public Health enemyHealth;
    

    [Header("Destructable Object References")]
    public Building destructableObjectHealth;

    [Header("Shared References")]
    public TMP_Text objectHealthText;
    public Slider objectHealthSlider;
    public Transform canvasTransform;

    // Start is called before the first frame update
    void Start()
    {
        if(gameObject.CompareTag("Enemy"))
        {
            // Enemy Max Value
            objectHealthSlider.maxValue = enemyHealth.maxHealth;

            // Enemy Current Value
            objectHealthSlider.value = enemyHealth.currentHealth; 
        }

        if(gameObject.CompareTag("Temple"))
        {
            // Temple Max Value
            objectHealthSlider.maxValue = destructableObjectHealth.maxHealth;

            // Temple Current Value
            objectHealthSlider.value = destructableObjectHealth.currentHealth;
        }
        

    }    

    // Update is called once per frame
    void Update()
    {
        if(gameObject.CompareTag("Enemy"))
        {
            objectHealthSlider.value = enemyHealth.currentHealth;
            objectHealthText.text = gameObject.tag + " Health " + enemyHealth.currentHealth.ToString() + "/10"; 
        }
            

        if(gameObject.CompareTag("Temple"))
        {
            objectHealthSlider.value = destructableObjectHealth.currentHealth;
            objectHealthText.text = gameObject.tag + " Health " + destructableObjectHealth.currentHealth.ToString() + "/40";
        }
        
        
    }

    // Late update runs after all other updates to ensure canvas moves with the object's current position
    private void LateUpdate()
    {
        //tells the canvas to look at the main camera's transform.
        canvasTransform.LookAt(Camera.main.transform);
    }
}
