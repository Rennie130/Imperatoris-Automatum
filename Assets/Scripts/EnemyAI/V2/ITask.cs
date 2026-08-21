using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITask
{
    public bool IsTaskValid(EnemyBehaviour behaviourManager);

    public void Execute(EnemyBehaviour behaviourManager);

}
