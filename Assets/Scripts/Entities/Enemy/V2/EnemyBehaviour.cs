using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{

    public NavMeshAgent Agent { get; private set; }

    [Header("Detection")]
    [SerializeField] public float detectionRange = 20f;


    private ITask[] _tasks = new ITask[] //list of tasks, in the order of their priority
    {
        new AttackMechTask(),
        new AttackTempleTask(),
        new PatrolTask()
    };

    private void FixedUpdate() 
    {
        foreach(ITask task in _tasks) //Loop over all available tasks
        {
            if(task.IsTaskValid(this)) //is this task valid
            {    
                task.Execute(this); //perform this task, and stop looking for a valid task
                break;
            }
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        Agent.SetDestination(position);
    }
   
    public void FacePosition(Vector3 position)
    {
        Vector3 dir = position - transform.position;

        dir.y = 0f;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 100f * Time.deltaTime);
    }

}
