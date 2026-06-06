using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    protected override void OnWindUp()
    {
        Debug.Log($"[ENEMY WIND-UP] {name}");
    }

    protected override void OnHit(Transform hitTarget)
    {
        Debug.Log($"[ENEMY HIT] {name} hit {hitTarget.name}");
    }

}
