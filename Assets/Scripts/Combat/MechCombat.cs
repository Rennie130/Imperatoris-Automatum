using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechCombat : CombatBase
{
    protected override void Update()
    {
        base.Update();

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

    public void AnimationHitFrame_Event()
    {
        AnimationHitFrame();
    }

    protected override void OnWindUp()
    {
        Debug.Log($"[MECH WIND-UP] {name} preparing attack");
    }

    protected override void OnHit(Transform target)
    {
        Debug.Log($"[MECH HIT CONFIRMED] {name} hit {target.name}");
    }

}
