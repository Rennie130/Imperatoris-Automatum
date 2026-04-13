using System.Collections;
using System.Collections.Generic;
//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header ("References")]
    // sets the target we are chasing
    public Transform target;
    public float detectionRange = 25f;
    // a Transform array to store patrol point locations
    public Transform[] patrolPoints;
    public float fieldOfView = 120f;


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

    EnemyCombat combat;

    enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }
    
    EnemyState currentState;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // send enemy to first patrol point
        GoToNext();

        combat = GetComponent<EnemyCombat>();
        //ensure combat always knows target
        if (combat != null)
        {
            combat.target = target;
        }

        currentState = EnemyState.Patrol;
    }


    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        float effectiveAttackRange = combat.attackRange + combat.attackRadius;
        
        if (distance > detectionRange || !CanSeeTarget())
        {
            currentState = EnemyState.Patrol;
        }
        else if (distance > effectiveAttackRange)
        {
            currentState = EnemyState.Chase;     
        }
        else
        {
            currentState = EnemyState.Attack;
        }
        
        if (combat !=null)
        {
            combat.target = target;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;

            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
        }
    }

    void HandlePatrol()
    {
        if (agent.isStopped)
            agent.isStopped = false;
        
        Patrol();
    }

    void HandleChase()
    {
        if (target == null) return;

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    void HandleAttack()
    {
        if (target == null) return;

        agent.isStopped = true;

        //Face the player
        Vector3 dir = (target.position - transform.position);
        dir.y = 0;

        if (dir.magnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }

        if (combat != null)
        {
            combat.TryAttack(target);
        }

        Debug.Log("Enemy in ATTACK state");
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

    bool CanSeeTarget()
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, dirToTarget);

        return angle < fieldOfView * 0.5f;
    }

}
