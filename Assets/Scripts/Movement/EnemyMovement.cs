using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform target;
    public float attackDistance;

    private NavMeshAgent agent;
    private float distance;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
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
    }
}
