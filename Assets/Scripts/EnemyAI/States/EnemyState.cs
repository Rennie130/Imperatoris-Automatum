using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : IEnemyState
{
    protected EnemyController enemy;

    protected EnemyState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public virtual void Enter() {}
    public virtual void Tick() {}
    public virtual void Exit() {}
}
