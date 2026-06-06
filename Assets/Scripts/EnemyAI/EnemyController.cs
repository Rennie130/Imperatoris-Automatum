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

    [Header("Detection")]
    public float detectionRange = 20f;
    public float loseInterestRange = 45f;
    public float fieldOfView = 120f;
    public LayerMask obstructionMask;

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
        Building building = DistrictManager.Instance?.GetClosestBuilding(transform.position);

        if (building != null)
        {
            SetTarget(building);

            return true;
        }

        return false;
    }

    void OnDamaged(Transform attacker)
    {
        if (attacker == null) return;

        ITargetable target = attacker.GetComponent<ITargetable>();

        if (target != null)
        {
            SetTarget(target);
        }
    }

    public bool HasReachedDestination()
    {
        if (Agent.pathPending) return false;

        return Agent.remainingDistance <= Agent.stoppingDistance;
    }
}