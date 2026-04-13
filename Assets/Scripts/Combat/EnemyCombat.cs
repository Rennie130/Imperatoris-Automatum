using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    public Transform target;

    void Update()
    {
        if (target == null) return;

        TryAttack(target);
    }

     protected override void OnWindUp()
    {
        Debug.Log(name + " wind-up");
    }

    protected override void OnHit(Transform hitTarget)
    {
        Debug.Log(name + " hit: " + hitTarget.name);
    }
}
