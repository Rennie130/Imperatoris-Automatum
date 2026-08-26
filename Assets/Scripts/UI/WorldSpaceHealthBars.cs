using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WorldSpaceHealthBars : MonoBehaviour
{
    public Health enemyHealth;
    public TMP_Text enemyHealthText;
    public Slider enemyHealthSlider;
    public Transform canvasTransform;

    // Start is called before the first frame update
    void Start()
    {
        // Max Value
        enemyHealthSlider.maxValue = enemyHealth.maxHealth;

        // Current Value
        enemyHealthSlider.value = enemyHealth.currentHealth;
    }    

    // Update is called once per frame
    void Update()
    {
        enemyHealthSlider.value = enemyHealth.currentHealth;
        enemyHealthText.text = "Enemy Health " + enemyHealth.currentHealth.ToString() + "/10";
    }

    // Late update runs after all other updates to ensure canvas moves with the enemy's current position
    private void LateUpdate()
    {
        //tells the canvas to look at the main camera's transform.
        canvasTransform.LookAt(Camera.main.transform);
    }
}
