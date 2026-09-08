using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : IEnemyState
{
    EnemyController enemy;

    float timer;

    Vector3 searchPosition;

    public SearchState(EnemyController enemy, Vector3 lastKnownPosition)
    {
        this.enemy = enemy;

        searchPosition = lastKnownPosition;
    }

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Search;

        timer = enemy.searchDuration;

        enemy.Agent.SetDestination(searchPosition);
    }

    public void Tick()
    {
        timer -= Time.deltaTime;

        if (enemy.TryFindTarget())
        {
            enemy.ChangeState(new ChaseState(enemy));

            return;
        }

        if (timer <= 0)
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    public void Exit()
    {

    }
}
