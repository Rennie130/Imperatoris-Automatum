using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    public Transform target;

    void Start()
    {
        Debug.Log($"[ENEMY COMBAT ACTIVE] {name}");
    }

     protected override void OnWindUp()
    {
        Debug.Log($"[ENEMY WIND-UP] {name} preparing attack");
    }

    protected override void OnHit(Transform hitTarget)
    {
        Debug.Log($"[ENEMY HIT CONFIRMED] {name} hit {target.name}");
    }

}
