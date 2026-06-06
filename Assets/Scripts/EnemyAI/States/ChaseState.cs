using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IEnemyState
{
   EnemyController enemy;

   public ChaseState(EnemyController enemy)
   {
    this.enemy = enemy;
   }

   public void Enter()
   {
    enemy.DebugState = EnemyStateType.Chase;

    enemy.Agent.isStopped = false;
   }

   public void Tick()
   {
    if (enemy.CurrentTarget == null || !enemy.CurrentTarget.IsAlive)
    {
        enemy.ClearTarget();

        enemy.ChangeState(new SearchState(enemy, enemy.LastKnownTargetPosition));

        return;
    }

    enemy.MoveToTarget();

    if (enemy.IsInAttackRange())
    {
        enemy.ChangeState(new AttackState(enemy));
    }

   }
   
   public void Exit()
   {
        enemy.Agent.ResetPath();
   }

}

