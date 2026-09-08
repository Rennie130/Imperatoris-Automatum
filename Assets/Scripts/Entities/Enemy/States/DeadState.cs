using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : IEnemyState
{
    EnemyController enemy;

    public DeadState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.DebugState = EnemyStateType.Dead;

        enemy.Agent.isStopped = true;
    }

    public void Tick()
    {

    }

    public void Exit()
    {
        
    }
}
