using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IEnemyState
{
    private EnemyController enemy;
    private Vector3 patrolPoint;
    private float patrolDelayCooldown = 0;

    public PatrolState(EnemyController enemy)
    {
        this.enemy = enemy;
    } 

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Patrol;

        NavigateToNextPoint();
    }

    public void Tick()
    {
        if (enemy.TryFindTarget())
        {
            enemy.ChangeState(new ChaseState(enemy));

            return;
        }

        patrolDelayCooldown -= Time.deltaTime;

        
        
        if (enemy.Agent.isStopped && patrolDelayCooldown <= 0)
        {
            enemy.Agent.isStopped = false;
        }

        if (!enemy.Agent.pathPending && enemy.HasReachedDestination())
        {
            enemy.Agent.isStopped = true;
            NavigateToNextPoint();
            //decide if we want to pause at this navigation point
            var chance = Random.Range(0, 100);
            Debug.Log(chance);
            if (chance < enemy.PatrolDelayChance)
            {
                patrolDelayCooldown = enemy.PatrolDelayDurationSeconds;
                //play look animation
            }
        }
    }

    private void NavigateToNextPoint()
    {
        //resume patrol behaviour
        patrolPoint = enemy.GetRandomPatrolPoint();
        enemy.Agent.SetDestination(patrolPoint);
    }

    public void Exit()
    {

    }
}
