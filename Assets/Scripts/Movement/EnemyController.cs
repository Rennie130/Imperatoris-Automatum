using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Header("References")]
    public Transform mechTarget;

    NavMeshAgent agent;
    EnemyCombat combat;
    HealthBase health;

    StateMachine<EnemyController, EnemyState> stateMachine;

    [Header("Debug")]
    public EnemyState debugState;

    [Header("Detection")]
    public float detectionRange = 20f;
    public float loseInterestRange = 45f;
    public float fieldOfView = 120;
    public float eyeHeight = 1.5f;
    public LayerMask obstructionMask;

    [Header("Combat")]
    public float attackRange = 3f;
    public float aggroMemoryDuration = 6f;

    [Header("Patrol")]
    public float patrolRadius = 20f;
    public float idleDuration = 2f;

    [Header("Movement")]
    public float turnSpeed = 100f;

    Transform currentTarget;

    Vector3 wanderTarget;

    float idleTimer;
    float aggroTimer;

    public Transform CurrentTarget => currentTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<HealthBase>();

        stateMachine = new StateMachine<EnemyController, EnemyState> (this, EnemyState.Patrol);

        // PATROL
        stateMachine.ForState(EnemyState.Patrol)
            .OnTick(_ => HandlePatrol());
        
        // CHASE
        stateMachine.ForState(EnemyState.Chase)
            .OnTick(_ => HandleChase());
        
        // ATTACK
        stateMachine.ForState(EnemyState.Attack)
            .OnTick(_ => HandleAttack());
        
        // DAMAGE AGGRO
        health.OnDamaged += OnDamaged;
    }

    void Start()
    {
        agent.stoppingDistance = attackRange * 0.8f;

        Debug.Log($"{name} On NavMesh: {agent.isOnNavMesh}");

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);

                Debug.Log($"{name} warped to NavMesh");
            }
        }
    }

    void Update()
    {
        if (stateMachine.CurrentState.ID == EnemyState.Dead) return;

        UpdateAggro();

        EvaluateState();

        stateMachine.Tick();

        debugState = stateMachine.CurrentState.ID;
    }

    /// =========================================
    ///     DECISION SYSTEM
    /// =========================================
    void EvaluateState()
    {
        // Dead target cleanup
        if (currentTarget == null)
        {
            stateMachine.SetState(EnemyState.Patrol, false);
            return;
        }

        // Lose target completely
        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist > loseInterestRange)
        {
            ClearTarget();
            return;
        }

        // Attack
        if (IsInAttackRange())
        {
            stateMachine.SetState(EnemyState.Attack, false);
            return;
        }

        // Chase
        stateMachine.SetState(EnemyState.Chase, false);
    }

    /// =========================================
    ///     PATROL
    /// =========================================
    void HandlePatrol()
    {
        agent.isStopped = false;

        TryFindTarget();

        if (currentTarget != null) return;

        // Debug current path state
       // Debug.Log($"Path Status: {agent.pathStatus} | " + $"Remaining: {agent.remainingDistance}");

        if (!agent.hasPath || agent.remainingDistance <= 1f)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {
                wanderTarget = GetRandomPoint(transform.position, patrolRadius);

                agent.SetDestination(wanderTarget);

                Debug.Log($"NEW PATROL TARGET: {wanderTarget}");

                idleTimer = 0f;
            }
        }
    }

    /// =========================================
    ///     CHASE
    /// =========================================
    void HandleChase()
    {
        if (currentTarget == null) return;

        // Stop nav conflict during lungeDuration
        if (combat.navigationLocked) return;

        agent.isStopped = false;

        Vector3 targetPos = GetTargetPoint(currentTarget);

        // Prevent SetDestination spam
        if (Vector3.Distance(agent.destination, targetPos) > 1f)
        {
            agent.SetDestination(targetPos);
        }

        FaceTarget();
    }

    /// =========================================
    ///     ATTACK 
    /// =========================================
    void HandleAttack()
    {
        if (currentTarget == null) return;

        agent.isStopped = true;

        FaceTarget();

        // Attack only if not already attacking
        if (!combat.IsAttacking)
        {
            combat.TryAttack();
        }

        // Return tp chase if target escapes
        if (!IsInAttackRange())
        {
            stateMachine.SetState(EnemyState.Chase, false);
        }
    }

    /// =========================================
    ///     TARGETING
    /// =========================================
    void TryFindTarget()
    {
        // Existing target memory
        if (currentTarget != null) return;

        // PRIORITY 1: Vulcani
        if (CanSeeTarget(mechTarget))
        {
            SetTarget(mechTarget);
            return;
        }

        // PRIORITY 2: Building
        Building building = DistrictManager.Instance?.GetClosestBuilding(transform.position);

        // Buildings ignore LOS/FOV
        if (building != null && building.IsAlive)
        {
            float dist = Vector3.Distance(transform.position, building.transform.position);

            if (dist <= detectionRange)
            {
                SetTarget(building.transform);
            }
        }
    }

    void SetTarget(Transform target)
    {
        currentTarget = target;
        aggroTimer = aggroMemoryDuration;
    }

    void ClearTarget()
    {
        currentTarget = null;
        stateMachine.SetState(EnemyState.Patrol, false);
    }

    /// =========================================
    ///     AGGRO MEMORY
    /// =========================================
    void UpdateAggro()
    {
        if (currentTarget == null) return;

        // Buildings never lose aggro
        if (currentTarget.GetComponent<Building>() != null) return;

        bool canSee = CanSeeTarget(currentTarget);

        if (canSee)
        {
            aggroTimer = aggroMemoryDuration;
        }
        else
        {
            aggroTimer -= Time.deltaTime;

            if (aggroTimer <= 0f)
            {
                ClearTarget();
            }
        }
    }

    /// =========================================
    ///     DETECTION
    /// =========================================
    bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > detectionRange) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = target.position + Vector3.up * eyeHeight;
        Vector3 dir = (targetPos - origin).normalized;

        // FOV
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle > fieldOfView * 0.5f) return false;

        // LOS
        if (Physics.Raycast(origin, dir, out RaycastHit hit, detectionRange, obstructionMask))
        {
            if (!hit.transform.IsChildOf(target)) return false;
        }

        Debug.DrawLine(origin, targetPos, Color.red);

        return true;
    }

    /// =========================================
    ///     MINIONS
    /// =========================================
    bool IsInAttackRange()
    {
        if (currentTarget == null) return false;

        return Vector3.Distance(transform.position, GetTargetPoint(currentTarget)) <= attackRange;
    }

    void FaceTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - transform.position;

        dir.y = 0f;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    Vector3 GetTargetPoint(Transform target)
    {
        Collider col = target.GetComponentInChildren<Collider>();

        if (col != null)
        {
            return col.ClosestPoint(transform.position);
        }

        return target.position;
    }

    Vector3 GetRandomPoint(Vector3 centre, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = centre + Random.insideUnitSphere * radius;

            random.y = centre.y;

            // Find nearest NavMesh point
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                // Prevent tiny little patrols
                float dist = Vector3.Distance(transform.position, hit.position);

                if (dist < 5f) continue;

                // Verify path is actually reachable
                NavMeshPath path = new NavMeshPath();

                bool validPath = agent.CalculatePath(hit.position, path);

                // Ensure path is valid
                if (validPath && path.status == NavMeshPathStatus.PathComplete)
                {
                    Debug.DrawLine(transform.position, hit.position, Color.green, 2f);
                    return hit.position;
                }
            }
        }

        // Fallback
        return transform.position;
    }

    /// =========================================
    ///     DAMAGE AGGRO
    /// =========================================
    void OnDamaged(Transform attacker)
    {
        if (attacker == null) return;

        SetTarget(attacker);
    }
}