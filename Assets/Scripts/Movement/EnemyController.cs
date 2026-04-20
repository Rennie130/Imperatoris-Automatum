using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum State
    {
        Patrol,
        Chase,
        Attack,
        AttackBuilding
    }

    State currentState;

    private Rigidbody rb;

    [Header("References")]
    public Transform mechTarget;            // assign mech in inspector
    EnemyCombat combat;
    NavMeshAgent agent;

    [Header("Ranges")]
    public float detectionRange = 15f;
    public float attackRange = 2.5f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int patrolIndex;

    Transform currentTarget; // mech OR building

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<EnemyCombat>();

        //Auto find Mech (or building)

        if (combat == null)
        {
            Debug.Log($"[ERROR] EnemyCombat missing on {name}");
        }
    }

    void Start()
    {
        currentState = State.Patrol;
        GoToNextPatrolPoint();

        agent.stoppingDistance = attackRange * 0.9f;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.mass = 1000f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (mechTarget == null) return;

        UpdateState();
        HandleState();

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TEST] forcing attack");
            combat.TryAttack();
        }
    }

    // DECIDE WHAT STATE WE SHOULD BE IN
    void UpdateState()
    {
        State previous = currentState;

        float distToMech = Vector3.Distance(transform.position, mechTarget.position);

        if (distToMech <= attackRange)
        {
            currentState = State.Attack;
            currentTarget = mechTarget;
        }

        if (distToMech <= detectionRange)
        {
            currentState = State.Chase;
            currentTarget = mechTarget;
        }

        // fallback to buildings
        Building b = DistrictManager.Instance?.GetClosestBuilding(transform.position);

        if (b != null)
        {
            float distToBuilding = Vector3.Distance(transform.position, b.transform.position);

            if (distToBuilding <= attackRange)
            {
                currentState = State.AttackBuilding;
                currentTarget = b.transform;
            }
            else
            {
                currentState = State.Chase;
                currentTarget = b.transform;
            }

            return;
        }

        if (previous != currentState)
        {
            Debug.Log($"[AI STATE] {name}: {previous} -> {currentState}");

            agent.isStopped = false;
            agent.ResetPath();
        }

        // default
        currentState = State.Patrol;
    }

    //EXECUTE STATE BEHAVIOUR
    void HandleState()
    {
        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;

            case State.Chase:
                HandleChase();
                break;

            case State.Attack:
                HandleAttack();
                break;

            case State.AttackBuilding:
                HandleAttack();
                break;
        }
    }

    Vector3 wanderTarget;

    // PATROL
    void HandlePatrol()
    {
        agent.isStopped = false;

        if (!agent.hasPath && agent.remainingDistance < 1f)
        {
            wanderTarget = GetRandomPoint(transform.position, 10f);
            agent.SetDestination(wanderTarget);

            Debug.Log($"[PATROL] New wander target: {wanderTarget}");
        }

        Vector3 GetRandomPoint(Vector3 centre, float radius)
        {
            Vector3 random = centre + Random.insideUnitSphere * radius;

            NavMeshHit hit;
            NavMesh.SamplePosition(random, out hit, radius, NavMesh.AllAreas);

            return hit.position;
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[patrolIndex].position;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // CHASE (mech or building)
    void HandleChase()
    {
        if (currentTarget == null) return;

        agent.isStopped = false;
        agent.SetDestination(currentTarget.position);
    }

    //ATTACK
    void HandleAttack()
    {
        if (currentTarget == null) return;

        agent.isStopped = true;

        // face target
        Vector3 dir = (currentTarget.position - transform.position);
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            transform.forward = dir.normalized;
        }

        Debug.Log($"[AI] {name} attempting attack on {currentTarget.name}");

        // attack
        combat.TryAttack();
    }
}