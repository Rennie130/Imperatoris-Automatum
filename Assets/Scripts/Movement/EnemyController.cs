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
    [SerializeField] Transform eyePoint;
    public float detectionRange = 20f;
    public float loseInterestRange = 45f;
    public float fieldOfView = 120;
    public LayerMask obstructionMask;

    [Header("Combat")]
    public float aggroMemoryDuration = 6f;
    [SerializeField] float attackEnterRange = 10f;
    [SerializeField] float attackExitRange = 12f;

    [Header("Patrol")]
    public float patrolRadius = 20f;
    public float idleDuration = 2f;

    [Header("Movement")]
    public float turnSpeed = 100f;

    Transform currentTarget;
    Transform currentTargetPoint;

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
        mechTarget = GameManager.Instance.mech;
        agent.stoppingDistance = combat.AttackRange * 0.8f;
        agent.updateRotation = true;
        agent.angularSpeed = turnSpeed;
        agent.autoBraking = false;

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
        EnemyState desiredState;

        // No target
        if (currentTarget == null)
        {
            desiredState = EnemyState.Patrol;
        }
        else
        {
            float dist = Vector3.Distance(transform.position, GetTargetPoint(currentTarget));

            // Lose target
            if (dist > loseInterestRange)
            {
                ClearTarget();
                return;
            }

            bool inAttackRange = Vector3.Distance(transform.position, GetTargetPoint(currentTarget)) <= attackEnterRange;
            bool shouldLeaveAttack = Vector3.Distance(transform.position, GetTargetPoint(currentTarget)) > attackExitRange;

            if (stateMachine.CurrentState.ID == EnemyState.Attack)
            {
                desiredState = shouldLeaveAttack ? EnemyState.Chase : EnemyState.Attack;
            }
            else
            {
                desiredState = inAttackRange ? EnemyState.Attack : EnemyState.Chase;
            }
        }

        // Only change if needed
        if (stateMachine.CurrentState.ID != desiredState)
        {
            //agent.ResetPath();

            stateMachine.SetState(desiredState, false);
        }
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

                //Debug.Log($"NEW PATROL TARGET: {wanderTarget}");

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
        Debug.Log($"State: {stateMachine.CurrentState.ID} | " + $"Locked: {combat.navigationLocked} | " + $"Stopped: {agent.isStopped}");

        agent.isStopped = false;

        Vector3 targetPos = GetTargetPoint(currentTarget);

        if (Vector3.Distance(agent.destination, targetPos) > 1.5f)
        {
            agent.SetDestination(targetPos);
        }

        Debug.DrawLine(transform.position, targetPos, Color.green);

        Debug.Log($"NAV LOCKED: {combat.navigationLocked}");

        Debug.Log($"Destination Valid: " + $"{agent.isOnNavMesh} | " + $"{agent.pathStatus}");

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

        if (!combat.IsAttacking)
        {
            combat.navigationLocked = false;
        }

        // Return tp chase if target escapes
        if (!IsInAttackRange())
        {
            stateMachine.SetState(EnemyState.Chase, false);
        }
    }

   // void ExitAttack()
   // {
   //     combat.navigationLocked = false;
   //     agent.isStopped = false;
   // }
//
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
                // Optional LOS check later
                SetTarget(building.transform);
                return;
            }
        }
    }

    void SetTarget(Transform target)
    {
        currentTarget = target;

        TargetPoint point = target.GetComponentInChildren<TargetPoint>();

        currentTargetPoint = point != null ? point.transform : target;

        aggroTimer = aggroMemoryDuration;

        idleTimer = 0f;
        
        agent.ResetPath();
    }

    void ClearTarget()
    {
        currentTarget = null;
        currentTargetPoint = null;

        agent.ResetPath();

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

        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position + Vector3.up * 3f;
        Vector3 targetPos = GetTargetPoint(target);

        float dist = Vector3.Distance(origin, targetPos);

        if (dist > detectionRange) return false;

        Vector3 dir = (targetPos - origin).normalized;
        
        // FOV
        float angle = Vector3.Angle(eyePoint.forward, dir);

        if (angle > fieldOfView * 0.5f) return false;

        // LOS
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstructionMask))
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

        return Vector3.Distance(transform.position, GetTargetPoint(currentTarget)) <= combat.AttackRange;
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
        if (target == null)
        {
            return transform.position;
        }

        // Try collider closest point
        Collider col = target.GetComponentInChildren<Collider>();

        if (col != null)
        {
            Vector3 dir = (transform.position - col.bounds.center).normalized;
//
            return col.bounds.center + dir * combat.AttackRange * 0.8f;
        }

        // Fallback to TargetPoint marker
        TargetPoint point = target.GetComponentInChildren<TargetPoint>();

        if (point != null)
        {
            return point.transform.position;
        }
        
        // Final fallback
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

    void OnDrawGizmosSelected()
    {
        // Detection Range
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack Range
        if (combat != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, combat.AttackRange);
        }

        if (eyePoint != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * eyePoint.forward;

            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * eyePoint.forward;

            Gizmos.DrawRay(eyePoint.position, leftBoundary * detectionRange);

            Gizmos.DrawRay(eyePoint.position, rightBoundary * detectionRange);

            // Forward direction
            Gizmos.color = Color.blue;

            Gizmos.DrawRay(eyePoint.position, eyePoint.forward * detectionRange);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(eyePoint.position, GetTargetPoint(currentTarget));
        }
    }
}