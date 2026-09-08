using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AttackMechTask : ITask
{

    private ITargetable _mechTarget;

    public bool IsTaskValid(EnemyBehaviour behaviourManager)
    {
        //if there is a player nearby and visible
        _mechTarget = FindMechTarget(behaviourManager);
        return _mechTarget != null;
    }

    public void Execute(EnemyBehaviour behaviourManager)
    {
        //Logic for moving to player and attacking (via combat scripts)
        
        
    }


    //Returns null if target is invalid
    private ITargetable FindMechTarget(EnemyBehaviour behaviourManager)
    {
        Transform mech = GameManager.Instance.mech;

        if (mech == null) return null;

        float distance = Vector3.Distance(behaviourManager.transform.position, mech.position);

        if (distance > behaviourManager.detectionRange) return null;

        ITargetable target = mech.GetComponent<ITargetable>();

        if (target == null) return null;

        if (!target.IsAlive) return null;

        return target;
    }

}