using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateUIStats : MonoBehaviour
{
   [Header("UI Text")]
    public TMP_Text playerHealthText;
    public TMP_Text signalStrengthText;
    public TMP_Text mechHealthText;
    public TMP_Text districtHealthText;

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.playerHealth)
        {
            //Update Player Health display in UI
            playerHealthText.text = "Player Health " + GameManager.Instance.playerHealth.currentHealth.ToString() + "/10";
        }        

        if(GameManager.Instance.mechHealth)
        {
            //Update Mech Health display in UI
            mechHealthText.text = "Mech Health " + GameManager.Instance.mechHealth.currentHealth.ToString() + "/20";
        }

        if(GameManager.Instance.signalStrength)
        {
            //Update Signal Strength display in UI
            signalStrengthText.text = "Signal Strength " + GameManager.Instance.signalStrength.signalStrength.ToString("P0");
        }

        if(GameManager.Instance.districtHealth)
        {
            //Update Mech Health display in UI
            districtHealthText.text = "District Health " + GameManager.Instance.districtHealth.currentDistrictHealth.ToString() + "/160";
        }

    }
}
