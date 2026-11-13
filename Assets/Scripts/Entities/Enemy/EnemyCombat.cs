using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    [Header("Enemy Attack Selection")]
    [SerializeField, Range(0f, 1f)]
    private float heavyAttackChance = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float leftAttackChance = 0.5f;

    public void TryChooseAttack()
    {
        if (IsAttacking || IsBlocking || IsStaggered)
        {
            return;
        }

        bool isHeavy = Random.value < heavyAttackChance;

        PlapSide side = Random.value < leftAttackChance ? PlapSide.Left : PlapSide.Right;

        TryAttack(side, isHeavy);
    }

    protected override void OnWindUp()
    {
        Debug.Log($"[ENEMY WIND-UP] {name}");
    }

    protected override void OnHit(Transform hitTarget)
    {
        Debug.Log($"[ENEMY HIT] {name} hit {hitTarget.name}");
    }

}
