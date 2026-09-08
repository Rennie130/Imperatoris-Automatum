using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PatrolTask : ITask
{
    public bool IsTaskValid(EnemyBehaviour behaviourManager)
    {
        return true;
    }

    public void Execute(EnemyBehaviour behaviourManager)
    {
        //Logic for moving to random points
    }
}