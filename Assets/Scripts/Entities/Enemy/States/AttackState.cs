using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IEnemyState
{
    EnemyController enemy;

    public AttackState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Attack;

        enemy.Agent.isStopped = true;
    }

    public void Tick()
    {
        if (enemy.CurrentTarget == null || !enemy.CurrentTarget.IsAlive)
        {
            enemy.ClearTarget();

            enemy.ChangeState(new SearchState(enemy, enemy.LastKnownTargetPosition));

            return;
        }

        enemy.FaceTarget();

        if (!enemy.Combat.IsAttacking)
        {
            enemy.Combat.TryAttack();
        }

        if (!enemy.IsInAttackRange())
        {
            enemy.ChangeState(new ChaseState(enemy));
        }
    }

    public void Exit()
    {
        enemy.Combat.CancelAttack();

        enemy.Agent.isStopped = false;
    }
}