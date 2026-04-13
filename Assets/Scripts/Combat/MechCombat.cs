using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechCombat : CombatBase
{
    void Update()
    {
        if (!GameModeManager.Instance.secondaryController)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack(transform);
        }
    }

    protected override void OnWindUp()
    {
        Debug.Log("Player wind-up");
    }

    protected override void OnHit(Transform hitTarget)
    {
        Debug.Log("Player hit: " + hitTarget.name);

        //Hit Feedback
        Time.timeScale = 0.2f;
        Invoke(nameof(ResetTime), 0.05f);
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
    }
}
