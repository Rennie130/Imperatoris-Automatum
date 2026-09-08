using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IEnemyState
{
    EnemyController enemy;

    Vector3 patrolPoint;

    public PatrolState(EnemyController enemy)
    {
        this.enemy = enemy;
    } 

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Patrol;

        patrolPoint = enemy.GetRandomPatrolPoint();

        enemy.Agent.SetDestination(patrolPoint);
        
        enemy.Agent.isStopped = false;
    }

    public void Tick()
    {
        if (enemy.TryFindTarget())
        {
            enemy.ChangeState(new ChaseState(enemy));

            return;
        }

        if (!enemy.Agent.pathPending && enemy.HasReachedDestination())
        {
            patrolPoint = enemy.GetRandomPatrolPoint();

            enemy.Agent.SetDestination(patrolPoint);

        }
    }

    public void Exit()
    {

    }
}
