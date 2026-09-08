using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AttackTempleTask : ITask
{
    public bool IsTaskValid(EnemyBehaviour behaviourManager)
    {
        //if there is a temple nearby and visible
        //find all nearby temple object
        //find nearest that is alive
        //perform line of sight check
        return false;
    }

    public void Execute(EnemyBehaviour behaviourManager)
    {
        
        //Logic for moving to temple and attacking (via combat scripts)
    }
}