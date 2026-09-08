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
    
    // Update is called once per frame
    void Update()
    {
        var gm = GameManager.Instance;
        
        if(GameManager.Instance.playerHealth)
        {
            //Update Player Health display in UI
            playerHealthSlider.value = gm.playerHealth.GetHealthPercentage();
            playerHealthText.text = "Player Health " + GameManager.Instance.playerHealth.currentHealth.ToString() + "/10";
        }        

        if(GameManager.Instance.mechHealth)
        {
            //Update Mech Health display in UI


            mechHealthSlider.value = gm.mechHealth.GetHealthPercentage();
            mechHealthText.text = "Mech Health " + GameManager.Instance.mechHealth.currentHealth.ToString() + "/30";
        }

        if(GameManager.Instance.signalStrength)
        {
            //Update Signal Strength display in UI
            signalStrengthSlider.value = gm.signalStrength.signalStrengthPercentage;
            signalStrengthText.text = "Signal Strength " + GameManager.Instance.signalStrength.signalStrengthPercentage.ToString("P0");
        }

        if(GameManager.Instance.districtHealth)
        {
            //Update District Health display in UI
            districtHealthSlider.value = gm.districtHealth.GetHealthPercentage();
            districtHealthText.text = "District Health " + GameManager.Instance.districtHealth.currentDistrictHealth.ToString() + "/132";
        }

    }
}
