using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WorldSpaceHealthBars : MonoBehaviour
{
    [Header("Enemy References")]
    public Health objectHealth;
    

    [Header("Destructible Object References")]
    public Building destructibleObjectHealth;

    [Header("Shared References")]
    public TMP_Text objectHealthText;
    public Slider objectHealthSlider;
    public Transform canvasTransform;

    // Start is called before the first frame update
    void Start()
    {
        // Enemy Max Value
        objectHealthSlider.maxValue = objectHealth.maxHealth;

        // Enemy Current Value
        objectHealthSlider.value = objectHealth.currentHealth;

    }    

    // Update is called once per frame
    void Update()
    {
        
        objectHealthSlider.value = objectHealth.currentHealth;
        objectHealthText.text = gameObject.tag + " Health " + objectHealth.currentHealth.ToString() + "/10"; 
        
    }

    // Late update runs after all other updates to ensure canvas moves with the object's current position
    private void LateUpdate()
    {
        //tells the canvas to look at the main camera's transform.
        canvasTransform.LookAt(Camera.main.transform);
    }
}
