using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class CombatBase : MonoBehaviour
{
    [System.Serializable]
    public struct HitStopData
    {
        public float freezeTime;
        public float timeScale;
    }


    // =======================================================
    //    ATTACK SETTINGS
    // =======================================================

    [Header("Attack Settings")]
    [SerializeField] float attackRadius = 2.5f;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRange = 2.5f;
    [SerializeField] int damage = 1;
    [SerializeField] LayerMask damageMask;

    public float AttackRange => attackRange;
    public bool CanBeInterrupted => true;


    // =======================================================
    //    TIMING
    // =======================================================

    [Header("Timing")]
    [SerializeField] public float windUpTime = 0.3f;
    [SerializeField] public float hitDelay = 0.1f;
    [SerializeField] public float recoveryTime = 0.4f;


    // =======================================================
    //    HEAVY PLAP
    // =======================================================

    [Header("Heavy Attack")]

    [Tooltip("How long the mouse must be help before the attack becomes heavy.")]
    [SerializeField] protected float heavyPlapHoldTime = 0.5f;

    [Tooltip("Multiplier applied to damage when performing heavy attack.")]
    [SerializeField] protected float heavyDamageMultiplier = 2f;

    [Tooltip("How long the fist hitbox stays active after the animation hit frame.")]
    [SerializeField] protected float fistHitboxDuration = 0.15f;


    // =======================================================
    //    BLOCK AND STAGGER
    // =======================================================

    [Header("Block and Stagger")]
    [SerializeField] private float staggerDuration = 0.5f;

    private bool isBlocking;
    private bool blockChordUsed;
    private float staggeredUntil;

    public bool IsBlocking => isBlocking;
    public bool IsStaggered => Time.time < staggeredUntil;


    // =======================================================
    //    COMBO / LUNGE / HIT STOP
    // =======================================================

    [Header("Combo")]
    [SerializeField] int maxCombo = 3;
    [SerializeField] float comboResetTime = 1f;
    
    [Header("Lunge")]
    [SerializeField] float lungeForce = 5f;
    [SerializeField] float lungeDuration = 0.12f;

    [Header("Hit Stop")]
    [SerializeField] protected HitStopData normalHit;
    [SerializeField] protected HitStopData heavyHit;
    [SerializeField] protected HitStopData finisherHit;


    // =======================================================
    //    COMBAT POINTS
    // =======================================================

    [Header("Combat Points")]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected Transform hitCentre;
    
    // keeping this for now bc I'm not sure if anything else uses it
    // no longer the actual damage point


    // =======================================================
    //    FIST HITBOXES
    // =======================================================

    [Header("Fist Hitboxes")]

    [Tooltip("Trigger collider attached to the LEFT fist.")]
    [SerializeField] protected FistHitbox leftFistHitbox;

    [Tooltip("Trigger collider attached to the RIGHT fist.")]
    [SerializeField] protected FistHitbox rightFistHitbox;
    

    // =======================================================
    //    PLAYER INPUT
    // =======================================================

    [Header("Player Input")]
    [Tooltip("Enable mouse-controlled plaps.")]
    [SerializeField] protected bool enableMouseInput = true;


    // =======================================================
    //    STATE
    // =======================================================

    protected bool isAttacking;
    protected bool queuedNextAttack;
    void QueueNextCombo()
    {
        queuedNextAttack = true;
    }

     protected int comboStep;
    protected float comboTimer;
    protected float lastAttackTime;

     protected NavMeshAgent agent;

    public bool navigationLocked;

    public bool IsAttacking => isAttacking;

    protected Rigidbody rb;
    protected Animator animator;

    Coroutine attackRoutine;


    // =======================================================
    //    PLAP STATE
    // =======================================================
     public enum PlapSide
    {
        Left,
        Right
    }

    protected PlapSide currentPlapSide;

    protected bool currentPlapIsHeavy;

    private bool leftMouseHeld;
    private bool rightMouseHeld;

    private float leftMouseHoldTime;
    private float rightMouseHoldTime;

    // Prevent the same enemy being hit multiple times by the same plap
    protected readonly System.Collections.Generic.HashSet<Damageable> hitTargets = new System.Collections.Generic.HashSet<Damageable>();


    // =======================================================
    //    UNITY
    // =======================================================

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();

        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();

        // Give fist hitboxes a reference back to this combat component
        if (leftFistHitbox != null)
        {
            leftFistHitbox.Initialise(this);
            leftFistHitbox.SetHitboxActive(false);
        }

        if (rightFistHitbox != null)
        {
            rightFistHitbox.Initialise(this);
            rightFistHitbox.SetHitboxActive(false);
        }
    }


    protected virtual void Update()
    {
        // =======================================================
        //    COMBO TIMER
        // =======================================================

        if (!isAttacking && comboStep > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0)
            {
                comboStep = 0;
            }
        }

        // =======================================================
        //    MOUSE INPUT
        // =======================================================

        if (enableMouseInput)
        {
            HandleMouseInput();
        }
    }


    protected virtual void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseHeld = true;
            leftMouseHoldTime = 0f;
        }

        if (Input.GetMouseButtonDown(1))
        {
            rightMouseHeld = true;
            rightMouseHoldTime = 0f;
        }

        if (leftMouseHeld)
        {
            leftMouseHoldTime += Time.deltaTime;
        }

        if (rightMouseHeld)
        {
            rightMouseHoldTime += Time.deltaTime;
        }

        // Holding both activates Blocking
        if (leftMouseHeld && rightMouseHeld)
        {
            blockChordUsed = true;
            SetBlocking(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool heavy = leftMouseHoldTime >= heavyPlapHoldTime;

            leftMouseHeld = false;
            leftMouseHoldTime = 0f;

            // Don't punch if this press was part of block
            if (!blockChordUsed)
            {
                TryPlap(PlapSide.Left, heavy);
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            bool heavy = rightMouseHoldTime >= heavyPlapHoldTime;

            rightMouseHeld = false;
            rightMouseHoldTime = 0f;

            // Don't punch if block
            if (!blockChordUsed)
            {
                TryPlap(PlapSide.Right, heavy);
            }

        }

        // Blocking ends as soon as either button is released
        SetBlocking(leftMouseHeld && rightMouseHeld);

        // Clear only after both buttons are released
        if (!leftMouseHeld && !rightMouseHeld)
        {
            blockChordUsed = false;
        }

    }

    private void SetBlocking(bool blocking)
    {
        if (isBlocking == blocking)
        {
            return;
        }

        isBlocking = blocking;

        if (animator != null)
        {
            animator.SetBool("IsBlocking", isBlocking);
        }
    }

    public void OnBlockedHit()
    {
        if (animator != null)
        {
            animator.SetTrigger("BlockHit");
        }
    }


    // =======================================================
    //    PLAP ENTRY
    // =======================================================

    protected virtual void TryPlap(PlapSide side, bool heavy)
    {
        TryAttack(side, heavy);
    }


    // =======================================================
    //    ATTACK ENTRY
    // =======================================================

    public virtual void TryAttack()
    {
        // Keeps compatibility with any other code that calls TryAttack()

        // Default is a left light punch
        
        //Debug.Log("[TRY ATTACK CALLED]");

        TryAttack(PlapSide.Left, false);
    }

    public virtual void TryAttack(PlapSide side, bool heavy)
    {
        if (isBlocking || IsStaggered)
        {
            return;
        }
        // -------------------------------------------
        // COOLDOWN
        // -------------------------------------------

        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log($"[BLOCKED] {name} on cooldown");
            return;    
        }

        //prevent mech attacking in wrong mode
        if (this is MechCombat)
        {
            if (!GameModeManager.Instance.IsControllingSecondary())
            {
                Debug.Log("[BLOCKED] {name} not in control mode");
                return;
            }
               
        }

        //Queue combo cont.
        if (isAttacking)
        {
            Debug.Log("[BLOCKED {name} already attacking]");
            queuedNextAttack = true;
            return;
        }

        // -------------------------------------------
        // SET PLAP STATE
        // -------------------------------------------

        currentPlapSide = side;
        currentPlapIsHeavy = heavy;

        Debug.Log($"[ATTACK START] {name} | " + $"{side} |" + $"{(heavy ? "HEAVY" : "LIGHT")}");

        attackRoutine = StartCoroutine(AttackRoutine());

    }  

    /// =========================
    ///     MAIN ATTACK LOOP
    /// =========================

    protected virtual IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float signalMultiplier = GetSignalMultiplier();

        // Clear previous hit targets

        hitTargets.Clear();

        // Wind up

        Debug.Log($"[WINDUP] {name}");
        // Wind-Up (telegraph)
        OnWindUp();

        // Trigger animation
        PlayAttackAnimation();

        // Wait for animation to finish
        yield return new WaitForSeconds(recoveryTime);
        
        // Small forward movement
       // PerformLunge();
        
      //  yield return new WaitForSeconds(windUpTime / signalMultiplier);

      //  Debug.Log($"[HIT FRAME] {name}");
        // Small Delay Before Hit Frame
      //  yield return new WaitForSeconds(hitDelay / signalMultiplier);

        // Hit Frame
       // PerformHit();

        // Recovery
     //   yield return new WaitForSeconds(recoveryTime / signalMultiplier);

        lastAttackTime = Time.time;
        isAttacking = false;

        // Continue combo if queued
        if (queuedNextAttack)
        {
            queuedNextAttack = false;

            comboStep++;
            
            // Wrap combo back around
            if (comboStep >= maxCombo)
                comboStep = 0;

            attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            // Begin combo expiration timer
            comboTimer = comboResetTime;
        }

    }

    protected virtual void PlayAttackAnimation()
    {
        if (animator == null) return;

        animator.SetInteger("ComboStep", comboStep);
        

        /*
        New parameters.

        PlapSide:
        0 = Left
        1 = Right

        IsHeavy:
        false = Light
        true = Heavy
        */
        
        animator.SetInteger("PlapSide", currentPlapSide == PlapSide.Left ? 0 : 1);

        animator.SetBool("IsHeavy", currentPlapIsHeavy);

        animator.SetTrigger("Plap");
    }

    // =========================================================
    // ANIMATION HIT FRAME
    // =========================================================

    // Replaces the old OverlapSphere attack
    public virtual void AnimationHitFrame()
    {
        Debug.Log($"[HIT FRAME] {name} | " + $"{currentPlapSide} | " + $"{(currentPlapIsHeavy ? "HEAVY" : "LIGHT" )}");

        // Optional lunge
        PerformLunge();

        // Resets targets for this plap
        hitTargets.Clear();

        // Activate fist collider
        ActivateCurrentFist();
    }

    // =========================================================
    // ACTIVATE CURRENT FIST
    // =========================================================

    protected virtual void ActivateCurrentFist()
    {
        FistHitbox fist = GetCurrentFist();

        if (fist == null)
        {
            Debug.LogWarning($"[COMBAT] No fist hitbox assigned for {currentPlapSide} plap.");
            return;
        }

        fist.SetHitboxActive(true);

        StartCoroutine(DisableFistAfterDelay(fist, fistHitboxDuration));
    }

    protected virtual FistHitbox GetCurrentFist()
    {
        if (currentPlapSide == PlapSide.Left)
        {
            return leftFistHitbox;
        }

        return rightFistHitbox;
    }

    private IEnumerator DisableFistAfterDelay(FistHitbox fist, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (fist != null)
        {
            fist.SetHitboxActive(false);
        }
    }

    // =========================================================
    // FIST HIT CALLBACK
    // =========================================================

    // Calls when fist collider touches Damageable
    public virtual void RegisterFistHit(Damageable damageable, Collider hitCollider)
    {
        if (damageable == null || hitCollider == null)
        {
            return;
        }

        // Prevent multiple hits from one plap
        if (hitTargets.Contains(damageable))
        {
            return;
        }

        // Find target's combat component, even if collider is on child object
        CombatBase targetCombat = hitCollider.GetComponentInParent<CombatBase>();

        // Blocking prevents damage and normal stagger
        if (targetCombat != null && targetCombat.IsBlocking)
        {
            hitTargets.Add(damageable);
            targetCombat.OnBlockedHit();

            Debug.Log($"[BLOCKED HIT] {hitCollider.name}");
            return;
        }

        // Register target
        hitTargets.Add(damageable);

        // Damage
        int finalDamage = GetComboDamage();

        if (currentPlapIsHeavy)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * heavyDamageMultiplier);

        }

        damageable.Hurt(finalDamage, transform);

        Debug.Log($"[FIST HIT] {name} -> {hitCollider.name} | " + $"Damage: {finalDamage} | " + $"Side: {currentPlapSide} | " + $"{(currentPlapIsHeavy ? "HEAVY" : "LIGHT")}");

        // Hit Stop
        HitStopData data = GetHitStopData();

        HitStopManager.Instance?.TriggerHitStop(data.freezeTime, data.timeScale);

        // Hit Callback
        OnHit(hitCollider.transform);

        // Interrupt and stagger targets with combat components
        if (targetCombat != null)
        {
            targetCombat.ApplyStagger();
        }
    }


    // =========================================================
    // COMBO DAMAGE
    // =========================================================

    protected virtual int GetComboDamage()
    {
        switch (comboStep)
        {
            case 0: return damage;
            case 1: return damage + 1;
            case 2: return damage + 2;
        }

        return damage;
    }


    // =========================================================
    // HIT STOP DATA
    // =========================================================

    protected virtual HitStopData GetHitStopData()
    {
        // Combo finisher
        if (comboStep >= maxCombo - 1)
        {
            return finisherHit;
        }

        // Middle combo hit
        if (comboStep == 1)
        {
            return heavyHit;
        }

        // Basic hit
        return normalHit;
    }


    // =========================================================
    // KNOCKBACK AND STAGGER
    // =========================================================

    protected virtual float GetKnockbackForce()
    {
        if (comboStep >= maxCombo - 1)
        {
            return 2f;
        }

        if (comboStep == 1)
        {
            return 1f;
        }

        return 0.5f;
    }

    public virtual void ApplyStagger()
    {
        staggeredUntil = Mathf.Max(staggeredUntil, Time.time + staggerDuration);

        CancelAttack();
        SetBlocking(false);

        //Don't leave held mouse button ready to punch immediately after stagger
        leftMouseHeld = false;
        rightMouseHeld = false;
        leftMouseHoldTime = 0f;
        rightMouseHoldTime = 0f;
        blockChordUsed = false;

        if (animator != null)
        {
            animator.SetTrigger("Stagger");
        }
    }


    // =========================================================
    // LUNGE
    // =========================================================

    protected virtual void PerformLunge()
    {
        // StarCoroutine(LungeRoutine());
    }

    protected virtual IEnumerator LungeRoutine()
    {
        LockNavigation();

        float timer = 0f;

        Vector3 start = transform.position;

        Vector3 forward;

        // For a plap, use current fist's forward direction when possible
        FistHitbox fist = GetCurrentFist();

        if (fist != null)
        {
            forward = fist.transform.forward;
        }
        else
        {
            forward = attackPoint != null ? attackPoint.forward : transform.forward;
        }

        Vector3 target = start + forward * lungeForce;

        while (timer < lungeDuration)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(start, target, timer / lungeDuration);

            yield return null;
        }

        UnlockNavigation();
    }

    protected virtual float GetLungeDistance()
    {
        switch (comboStep)
        {
            case 0: return 0.5f;
            case 1: return 0.8f;
            case 2: return 1.2f;
        }    

        return 0.5f;
    }


    // =========================================================
    // INTERRUPTION
    // =========================================================

    public virtual void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        // Turn off both fist hitboxes
        if (leftFistHitbox != null)
        {
            leftFistHitbox.SetHitboxActive(false);
        }

        if (rightFistHitbox != null)
        {
            rightFistHitbox.SetHitboxActive(false);
        }

        queuedNextAttack = false;
        comboStep = 0;
        isAttacking = false;
        hitTargets.Clear();
    }


    // =========================================================
    // SIGNAL
    // =========================================================

    protected float GetSignalMultiplier()
    {
        return 1f; // temporary until hook real system

        //replace with actual signal system later
        //float signal = signalSystem.Instance != null ? signalSystem.Instance.GetSignalStrength() : 1f;

        //clamp so it doesn't break gameplay
        //return Mathf.Clamp(signal, 0.5f, 1.5f);
    }


    // =========================================================
    // CAN ATTACK
    // =========================================================

    public bool CanAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, damageMask);

        return hits.Length > 0;
    }


    // =========================================================
    // NAVIGATION
    // =========================================================

   public virtual void LockNavigation()
    {
        if (agent == null)
        {
            return;
        }

        navigationLocked = true;

        // Stop path movement
        agent.isStopped = true;

        // Prevent NavMesh overriding transform movement
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public virtual void UnlockNavigation()
    {
        if (agent == null)
        {
            return;
        }

        navigationLocked = false;

        // Re-sync NavMesh position
        agent.Warp(transform.position);

        agent.updatePosition = true;
        agent.updateRotation = true;

        agent.isStopped = false;
    }

    
    // =========================================================
    // CALLBACKS
    // =========================================================

    protected virtual void OnWindUp()
    {
        
    }

    protected virtual void OnHit(Transform hitTarget)
    {
        
    }


    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 hitPoint = transform.position + transform.forward * attackRadius;

        Gizmos.DrawWireSphere(hitPoint, attackRadius);

        Gizmos.color = Color.yellow;

        Vector3 rangePoint = transform.position + transform.forward * attackRange;

        Gizmos.DrawWireSphere(rangePoint, 0.2f);
    }

}
