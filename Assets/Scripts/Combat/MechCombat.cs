using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechCombat : CombatBase
{
    int comboStep;
    float lastComboTime;
    public float comboResetTime = 1f;

    void Update()
    {
        if (!GameModeManager.Instance.IsControllingSecondary())
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[MECH INPUT DETECTED]");
            TryAttack();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[MECH INPUT] Attack pressed");

            TryAttack();
        }
    }

   // void HandleCombo()
   // {
  //      if (Time.time > lastComboTime + comboResetTime)
  //      {
  //          comboStep = 0;
    //    }
//
 //       comboStep++;
//        lastComboTime = Time.time;

 //       TryAttack();
 //   }

  //  public void Interrupt()
  //  {
  //      comboStep = 0;
  //  }

    protected override void OnWindUp()
    {
        Debug.Log($"[MECH WIND-UP] {name} preparing attack");
    }

    protected override void OnHit(Transform target)
    {
        Debug.Log($"[MECH HIT CONFIRMED] {name} hit {target.name}");
    }

}
