using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateUIStats : MonoBehaviour
{
   [Header("UI Text")]
    public TMP_Text playerHealthText;
    public TMP_Text signalStrengthText;
    public TMP_Text mechHealthText;
    public TMP_Text districtHealthText;

    public Slider playerHealthSlider;
    public Slider mechHealthSlider;
    public Slider signalStrengthSlider;
    public Slider districtHealthSlider;
    public Slider enemyHealthSlider;

    void Start()
    {
        //Max Values
        playerHealthSlider.maxValue = GameManager.Instance.playerHealth.maxHealth;
        mechHealthSlider.maxValue = GameManager.Instance.mechHealth.maxHealth;

        //Current Values
        playerHealthSlider.value = GameManager.Instance.playerHealth.currentHealth;
        mechHealthSlider.value = GameManager.Instance.mechHealth.currentHealth;
        signalStrengthSlider.value = GameManager.Instance.signalStrength.signalStrength;
        districtHealthSlider.value = GameManager.Instance.districtHealth.currentDistrictHealth;
    }
    
    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.playerHealth)
        {
            //Update Player Health display in UI
            playerHealthSlider.value = GameManager.Instance.playerHealth.currentHealth;
            //playerHealthText.text = "Player Health " + GameManager.Instance.playerHealth.currentHealth.ToString() + "/10";
        }        

        if(GameManager.Instance.mechHealth)
        {
            //Update Mech Health display in UI
            mechHealthSlider.value = GameManager.Instance.mechHealth.currentHealth;
            //mechHealthText.text = "Mech Health " + GameManager.Instance.mechHealth.currentHealth.ToString() + "/20";
        }

        if(GameManager.Instance.signalStrength)
        {
            //Update Signal Strength display in UI
            signalStrengthSlider.value = GameManager.Instance.signalStrength.signalStrength;
            //signalStrengthText.text = "Signal Strength " + GameManager.Instance.signalStrength.signalStrength.ToString("P0");
        }

        if(GameManager.Instance.districtHealth)
        {
            //Update District Health display in UI
            districtHealthSlider.value = GameManager.Instance.districtHealth.currentDistrictHealth;
            //districtHealthText.text = "District Health " + GameManager.Instance.districtHealth.currentDistrictHealth.ToString() + "/160";
        }

    }
}
