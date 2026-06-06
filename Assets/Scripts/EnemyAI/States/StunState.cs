using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunState : IEnemyState
{
    EnemyController enemy;

    float timer;

    public StunState(EnemyController enemy, float duration)
    {
        this.enemy = enemy;

        timer = duration;
    }

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Stun;

        enemy.Agent.isStopped = true;

        enemy.Combat.CancelAttack();
    }

    public void Tick()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    public void Exit()
    {
        enemy.Agent.isStopped = false;
    }
}
