using UnityEngine;
using UnityEngine.AI;


public class EnemyController : MonoBehaviour
{
    public enum EnemyBehaviourState
    {
        Patrol,
        ChasePlayer,
        ChaseBuilding,
        AttackPlayer,
        AttackBuilding
    }

    public EnemyBehaviourState stateDebug;
    EnemyBehaviourState currentState = EnemyBehaviourState.Patrol;

    StateMachine<EnemyController, EnemyBehaviourState> StateMachine;

    private Rigidbody rb;

    [Header("References")]
    public Transform mechTarget;         // assign mech in inspector
    EnemyCombat combat;
    NavMeshAgent agent;

    [Header("Ranges")]
    public float detectionRange = 15f;
    public float attackRange = 2.5f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int patrolIndex;

    public Transform currentTarget; // mech OR building

    Vector3 wanderTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<EnemyCombat>();

        StateMachine = new StateMachine<EnemyController, EnemyBehaviourState>(this, EnemyBehaviourState.Patrol);

        //PATROL
        StateMachine.ForState(EnemyBehaviourState.Patrol)
            .OnTick(_ => HandlePatrol())
            .WithTransition(EnemyBehaviourState.ChasePlayer, _ => Vector3.Distance(transform.position, mechTarget.position) <= detectionRange)
            .WithTransition(EnemyBehaviourState.ChaseBuilding, _ => GetDistanceToNearestBuilding() <= detectionRange);

        //PLAYER
        StateMachine.ForState(EnemyBehaviourState.ChasePlayer)
            .OnEnter(_ => currentTarget = mechTarget)
            .OnTick(_ => HandleChase())
            .WithTransition(EnemyBehaviourState.Patrol, _ => Vector3.Distance(transform.position, currentTarget.position) > detectionRange)
            .WithTransition(EnemyBehaviourState.AttackPlayer, _ => Vector3.Distance(transform.position, currentTarget.position) <= attackRange);

        StateMachine.ForState(EnemyBehaviourState.AttackPlayer)
            .OnTick(_ => HandleAttack())
            .WithTransition(EnemyBehaviourState.ChasePlayer, _ => Vector3.Distance(transform.position, currentTarget.position) > attackRange);

        //BUILDING
        StateMachine.ForState(EnemyBehaviourState.ChaseBuilding)
            .OnEnter(_ => currentTarget = DistrictManager.Instance?.GetClosestBuilding(transform.position).transform)
            .OnTick(_ => HandleChase())
            .WithTransition(EnemyBehaviourState.ChasePlayer, _ => Vector3.Distance(transform.position, mechTarget.position) <= detectionRange)
            .WithTransition(EnemyBehaviourState.AttackBuilding, _ => Vector3.Distance(transform.position, currentTarget.position) <= attackRange);

        StateMachine.ForState(EnemyBehaviourState.AttackBuilding)
            .OnTick(_ => HandleAttack())
            .WithTransition(EnemyBehaviourState.ChasePlayer, _ => Vector3.Distance(transform.position, mechTarget.position) <= detectionRange)
            .WithTransition(EnemyBehaviourState.ChaseBuilding, _ => Vector3.Distance(transform.position, currentTarget.position) > attackRange);

        //Auto find Mech (or building)

        if (combat == null)
        {
            Debug.Log($"[ERROR] EnemyCombat missing on {name}");
        }
    }

    void Start()
    {
        currentState = EnemyBehaviourState.Patrol;

        agent.stoppingDistance = attackRange * 0.9f;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.mass = 1000f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (mechTarget == null) return;

        StateMachine.Tick();
        stateDebug = StateMachine.CurrentState.ID;

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TEST] forcing attack");
            combat.TryAttack();
        }
    }

    private float GetDistanceToNearestBuilding()
    {
        Building buildingTarget = DistrictManager.Instance?.GetClosestBuilding(transform.position);
        return Vector3.Distance(transform.position, buildingTarget.transform.position);
    }


    // PATROL
    void HandlePatrol()
    {
        agent.isStopped = false;

        if (!agent.hasPath && agent.remainingDistance < 1f)
        {
            wanderTarget = GetRandomPoint(transform.position, 10f);
            agent.SetDestination(wanderTarget);

            //Debug.Log($"[PATROL] New wander target: {wanderTarget}");
        }

        Vector3 GetRandomPoint(Vector3 centre, float radius)
        {
            Vector3 random = centre + Random.insideUnitSphere * radius;

            NavMeshHit hit;
            NavMesh.SamplePosition(random, out hit, radius, NavMesh.AllAreas);

            return hit.position;
        }
    }

    // CHASE (mech or building)
    void HandleChase()
    {
        if (currentTarget == null) return;
        //if(!agent.hasPath)
            agent.SetDestination(currentTarget.position);
    }

    //ATTACK
    void HandleAttack()
    {
        if (currentTarget == null) return;


        agent.ResetPath();

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