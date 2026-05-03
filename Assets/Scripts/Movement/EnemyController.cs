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
    StateMachine<EnemyController, EnemyBehaviourState> StateMachine;

    [Header("References")]
    public Transform mechTarget;         // assign mech in inspector
    EnemyCombat combat;
    NavMeshAgent agent;

    [Header("Vision")]
    public LayerMask obstructionMask;
    public float eyeHeight = 1.5f;

    [Header("Ranges")]
    public float detectionRange = 15f;
    private float attackRange = 2.5f;
    public float loseInterestRange =25f;

    [Header("Patrol")]
    public float patrolRadius = 15f;

    Transform currentTarget; // mech OR building
    EnemyBehaviourState desiredState;

    Vector3 wanderTarget;

    float aggroTimer = 0f;
    public float aggroDuration = 3f;

    float idleTimer = 0f;
    float idleDuration = 2f;
    float attackTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<EnemyCombat>();

        GetComponent<Health>().OnDamaged += OnDamaged;

        StateMachine = new StateMachine<EnemyController, EnemyBehaviourState>(this, EnemyBehaviourState.Patrol);

        //PATROL
        StateMachine.ForState(EnemyBehaviourState.Patrol)
            .OnTick(_ => HandlePatrol());

        //PLAYER
        StateMachine.ForState(EnemyBehaviourState.ChasePlayer)
            .OnTick(_ => HandleChase());


        StateMachine.ForState(EnemyBehaviourState.AttackPlayer)
            .OnTick(_ => HandleAttack());

        //BUILDING
        StateMachine.ForState(EnemyBehaviourState.ChaseBuilding)
            .OnTick(_ => HandleChase());

        StateMachine.ForState(EnemyBehaviourState.AttackBuilding)
            .OnTick(_ => HandleAttack());
    }

    void Start()
    {
        //Agent is properly placed
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.Warp(hit.position);
        }

        agent.stoppingDistance = attackRange + 0.2f;

       var rb = GetComponent<Rigidbody>();
       if (rb != null)
        {
            rb.isKinematic = true;
        }

        rb.freezeRotation = true;
        rb.mass = 1000f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (mechTarget == null) return;

        aggroTimer -= Time.deltaTime;

        EvaluateDecision();

        if (StateMachine.CurrentState.ID != desiredState)
        {
            StateMachine.SetState(desiredState, false);
        }

        StateMachine.Tick();
        stateDebug = StateMachine.CurrentState.ID;

        //Debug.Log($"State: {StateMachine.CurrentState.ID} | HasPath: {agent.hasPath}");

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TEST] forcing attack");
            combat.TryAttack();
        }
         
    }

    /// =========================
    ///     DECISION SYSTEM
    /// =========================
    
    void EvaluateDecision()
    {
        //aggroTimer = Time.deltaTime;

        Transform bestTarget = GetBestTarget();

        if (bestTarget == null)
        {
            currentTarget = null;
            desiredState = EnemyBehaviourState.Patrol;
            return;
        }

        currentTarget = bestTarget;

        bool isPlayer = currentTarget == mechTarget;

        if (IsInAttackRange())
        {
            desiredState = isPlayer
                ? EnemyBehaviourState.AttackPlayer
                : EnemyBehaviourState.AttackBuilding;
        }
        else
        {
            desiredState = isPlayer
                ? EnemyBehaviourState.ChasePlayer
                : EnemyBehaviourState.ChaseBuilding;
        }
    }

    bool IsInAttackRange()
    {
        if (currentTarget == null) return false;

        Vector3 targetPoint = GetTargetPoint(currentTarget);
        float dist = Vector3.Distance(transform.position, targetPoint);
        
        return dist <= attackRange;
    }

    Transform GetBestTarget()
    {
        //Aggro priority
        if (aggroTimer > 0 && currentTarget != null)
            return currentTarget;

        //Player (LOS required)
        if (mechTarget != null && CanDetectTarget(mechTarget))
        {
            return mechTarget;
        }

        var health = mechTarget.GetComponent<HealthBase>();
        if (health != null && !health.IsAlive)
            return null;

        //Building fallback
        var building = DistrictManager.Instance?.GetClosestBuilding(transform.position);

        if (building != null && CanDetectTarget(building.transform))
        {
            return building.transform;
        }

        return null;
    }

    /// =========================
    ///     LINE OF SIGHT
    /// =========================
    
    bool CanDetectTarget(Transform target)
    {
        if (target == null) return false;

        float viewAngle = 120f;

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);

        if (angle > viewAngle * 0.5f)
            return false;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > detectionRange) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = target.position + Vector3.up * eyeHeight;
        Vector3 dir = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, detectionRange, obstructionMask))
        {
            if (!hit.transform.IsChildOf(target))
                return false;
        }

        Debug.DrawLine(origin, targetPos, Color.red, 0.1f);

        return true;
    }

    /// =========================
    ///     PATROL STATE
    /// =========================
    
    void HandlePatrol()
    {
        if (StateMachine.CurrentState.ID != EnemyBehaviourState.Patrol)
            return;

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f || !agent.hasPath)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {
                wanderTarget = GetRandomPoint(transform.position, patrolRadius);
                agent.SetDestination(wanderTarget);
                idleTimer = 0f;
            }
            

            //Debug.Log($"[PATROL] New wander target: {wanderTarget}");
        }
    }

    Vector3 GetRandomPoint(Vector3 centre, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = centre + Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(random, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(transform.position, hit.position);

                if (dist > 5f) //minimum distance
                    return hit.position;
            }
        }
        
        return transform.position + transform.forward * 5f;
    }
               
    /// =========================
    ///     CHASE STATE
    /// =========================
    
    void HandleChase()
    {
        if (currentTarget == null) return;

        if (IsInAttackRange())
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(GetTargetPoint(currentTarget));
          
    }

    /// =========================
    ///     ATTACK STATE
    /// =========================

    void HandleAttack()
    {
        if (currentTarget == null) return;

        agent.isStopped = true;
        agent.ResetPath();

        // Face target
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 8f * Time.deltaTime);
        }

        attackTimer -= Time.deltaTime;

        Debug.Log($"[ATTACK] {name} attempting attack on {currentTarget.name}");

        if (attackTimer <= 0f)
        {
            combat.TryAttack();
            attackTimer = 1.0f; //attack rate
        }
 
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

    /// =========================
    ///     DAMAGE / AGGRO
    /// =========================
    
    void OnDamaged (Transform attacker)
    {
        if (attacker == null) return;

        currentTarget = attacker;
        aggroTimer = aggroDuration;
        
        attackTimer = 0f; //force immediate response
    }

}