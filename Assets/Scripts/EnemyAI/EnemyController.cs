 using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }

    public EnemyCombat Combat { get; private set; }
    
    //public Animator Animator { get; private set; }

    HealthBase health;

    ITargetable currentTarget;

    public ITargetable CurrentTarget => currentTarget;

    public EnemyStateType DebugState;

    public Vector3 LastKnownTargetPosition { get; private set; }

    public bool CanBeInterrupted => true;

    IEnemyState currentState;

    public ITargetable AggroTarget
    {
        get;
        private set;
    }

    float aggroTimer;

    [SerializeField] float aggroDuration = 8f;

    [SerializeField] float playerOverrideRange = 15f;

    [SerializeField] float patrolRadius = 20f;

    [Header("Detection")]
    [SerializeField] public float detectionRange = 20f;
    public float loseInterestRange = 45f;
    public float fieldOfView = 120f;
    public LayerMask obstructionMask;
    public float eyeHeight = 1.5f;

    [Header("Combat")]
    public float attackRange = 10f;

    [Header("Search")]
    public float searchDuration = 5f;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        
        Combat = GetComponent<EnemyCombat>();

        health = GetComponent<HealthBase>();

        health.OnDamaged += OnDamaged;

       //Animator GetComponent<Animator>();
    }

    void Start()
    {
        ChangeState(new PatrolState(this));
    }

    void Update()
    {
        currentState?.Tick();

        if (aggroTimer > 0)
        {
            aggroTimer -= Time.deltaTime;

            if (aggroTimer <= 0)
            {
                AggroTarget = null;
            }
        }
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit();

        currentState = newState;

        currentState?.Enter();

        Debug.Log($"State Changed -> {newState.GetType().Name}");
    }

    public void SetTarget(ITargetable target)
    {
        currentTarget = target;

        if (target != null)
        {
            LastKnownTargetPosition = target.GetTargetPoint().position;
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public Vector3 GetTargetPosition()
    {
        if (currentTarget == null)
            return transform.position;

        return currentTarget.GetTargetPoint().position;
    }

    public bool IsInAttackRange()
    {
        if (currentTarget == null) return false;

        if (!currentTarget.IsAlive) return false;

        return Vector3.Distance(transform.position, GetTargetPosition()) <= attackRange;
    }
        

    public void MoveToTarget()
    {
        if (currentTarget == null) return;

        Agent.SetDestination(GetTargetPosition());
    }
   
    public void FaceTarget()
    {
        if (currentTarget == null) return;

        Vector3 dir = GetTargetPosition() - transform.position;

        dir.y = 0f;

        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 100f * Time.deltaTime);
    }

    public bool TryFindTarget()
    {
        ITargetable target = FindBestTarget();

        if (target == null)
        {
            return false;
        }

        SetTarget(target);

        return true;
    }

    public ITargetable FindBestTarget()
    {
        ITargetable playerTarget = FindPlayerTarget();

        if (playerTarget != null)
        {
            return playerTarget;
        }

        ITargetable buildingTarget = FindBuildingTarget();

        if (buildingTarget != null)
        {
            return buildingTarget;
        }

        return null;
    }

    public ITargetable FindPlayerTarget()
    {
        Transform mech = GameManager.Instance.mech;

        if (mech == null) return null;

        float distance = Vector3.Distance(transform.position, mech.position);

        if (distance > detectionRange) return null;

        ITargetable target = mech.GetComponent<ITargetable>();

        if (target == null) return null;

        if (!target.IsAlive) return null;

        return target;
    }

    public ITargetable FindBuildingTarget()
    {
        Building[] buildings = FindObjectsOfType<Building>();

        Building best = null;

        float closest = Mathf.Infinity;

        foreach (Building building in buildings)
        {
            if (!building.IsAlive) continue;

            float distance = Vector3.Distance(transform.position, building.transform.position);

            if (distance > detectionRange) continue;

            if (distance < closest)
            {
                closest = distance;
                best = building;
            }
        }

        return best;
    }

    public bool ShouldSwitchToPlayer()
    {
        Transform mech = GameManager.Instance.mech;

        if (mech == null) return false;

        float distance = Vector3.Distance(transform.position, mech.position);

        return distance <= playerOverrideRange;
    }

    void OnDamaged(Transform attacker)
    {
        if (attacker == null) return;

        ITargetable target = attacker.GetComponent<ITargetable>();

        if (target != null)
        {
            GainAggro(target);
        }
    }

    public bool HasReachedDestination()
    {
        if (Agent.pathPending) return false;

        return Agent.remainingDistance <= Agent.stoppingDistance + 1.5f;

    }

    public void GainAggro(ITargetable target)
    {
        AggroTarget = target;

        aggroTimer = aggroDuration;

        SetTarget(target);
    }

    public void Stagger(float duration)
    {
        ChangeState(new StunState(this, duration));
    }

    Vector3 debugPatrolPoint;

    public Vector3 GetRandomPatrolPoint()
    {
        Vector3 random = Random.insideUnitSphere * patrolRadius;

        Vector3 point = transform.position + new Vector3(random.x, 0, random.z);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(point, out hit, patrolRadius, NavMesh.AllAreas))
        {
            debugPatrolPoint = hit.position;
            return hit.position;
        }

        return transform.position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(debugPatrolPoint, 1f);
    }

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


}