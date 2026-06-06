using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IEnemyState
{
    EnemyController enemy;

    public PatrolState(EnemyController enemy)
    {
        this.enemy = enemy;
    } 

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Patrol;
    }

    public void Tick()
    {
        if (enemy.TryFindTarget())
        {
            enemy.ChangeState(new ChaseState(enemy));
        }
    }

    public void Exit()
    {

    }
}
