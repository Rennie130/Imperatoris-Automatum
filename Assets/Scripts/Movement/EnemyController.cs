using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Security;
//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header ("References")]
    // sets the target we are chasing
    public Transform target;
    // a Transform array to store patrol point locations
    public Transform[] patrolPoints;


    [Header ("Settings")]
    // Distance enemy stops from mech to attack
    public float attackDistance;
    // time an enemy stops at each patrol point
    public float patrolWaitTime = 2f;
    // used in "Patrol" method to check the distance between player and next patrol point
    public float stopAtDistance = 0.5f;

    private NavMeshAgent agent;
    // distance between enemy and mech
    private float distance;
    private int currentPatrolIndex;
    private bool isWaiting;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // send enemy to first patrol point
        GoToNext();
    }


    // Update is called once per frame
    void Update()
    {
        Patrol();
        // if (target == null) return;
        

        /*
        // checks and assigns the distance between enemy and mech
        distance = UnityEngine.Vector3.Distance(agent.transform.position, target.position);
        // if distance is less than specified attack distance, enemy stops.
        if (distance <= attackDistance)
        {
            agent.isStopped = true;
        }
        //otherwise, if distance is not less than, enemy keeps moving to mech position
        else
        {
            agent.isStopped = false;
            //updates the enemy destination to player mech's location
            agent.destination = target.position;
        } 
        */
    }


    // method to patrol between points
    private void Patrol()
    {
        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance <= stopAtDistance)
        {
            UnityEngine.Debug.Log("Initiate Coroutine");
            //start the coroutine
            StartCoroutine(WaitAtPatrolPoint());
        }

    }


    // A coroutine method that can suspend execution and resume at a later time
    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true; 

        // stop/wait for the amount of time set in the patrolWaitTime variable
        // Suspends the coroutine execution for the specified scaled time in seconds
        yield return new WaitForSecondsRealtime(patrolWaitTime);

        // after set time is up, continue moving to next point.
        UnityEngine.Debug.Log("Done waiting");
        agent.isStopped = false;
        GoToNext();
        isWaiting = false;
    }


    // method that allows us to go to next patrol point
    private void GoToNext()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        UnityEngine.Debug.Log("On way to patrol point");

        // loop to next item in array (increasing currentPatrolIndex by 1) or; 
        // change it back to 0 if no more points to shift through.Ensures resulting value is always between 0 and array.Length
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        UnityEngine.Debug.Log("Patrol Point updated");
    }

}
