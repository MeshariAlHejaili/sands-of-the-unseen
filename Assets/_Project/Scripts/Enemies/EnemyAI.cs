using UnityEngine;

public class EnemyAI : MonoBehaviour, IEnemyBehaviour
{
    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Animation")]
    [Tooltip("Optional Animator used to drive enemy FSM presentation. If empty, one is resolved from child objects.")]
    [SerializeField] private Animator animator;

    private static readonly int IsChasingHash = Animator.StringToHash("IsChasing");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private EnemyHealth enemyHealth;
    private Animator cachedAnimator;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private EnemyState currentState;
    private float moveSpeed;
    private float contactDamage;
    private float contactRange;
    private float contactRangeSqr;
    private float contactCooldown;
    private float nextDamageTime;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        cachedAnimator = animator != null ? animator : GetComponentInChildren<Animator>(true);
        UpdateAnimatorState();
    }

    public void Init(EnemyStatsContext statsContext)
    {
        playerTransform = statsContext.Target;
        playerHealth = statsContext.TargetHealth;
        moveSpeed = statsContext.MoveSpeed;
        contactDamage = statsContext.ContactDamage;
        contactRange = statsContext.ContactRange;
        contactRangeSqr = contactRange * contactRange;
        contactCooldown = statsContext.ContactDamageCooldown;
        nextDamageTime = 0f;

        ChangeState(HasLivingTarget() ? EnemyState.Chase : EnemyState.Idle, true);
    }

    public void ResetState()
    {
        playerTransform = null;
        playerHealth = null;
        moveSpeed = 0f;
        contactDamage = 0f;
        contactRange = 0f;
        contactRangeSqr = 0f;
        contactCooldown = 0f;
        nextDamageTime = 0f;

        ChangeState(EnemyState.Idle, true);
    }

    public void OnDeathAnimationComplete()
    {
        ChangeState(EnemyState.Dead, true);
    }

    private void Update()
    {
        if (enemyHealth == null)
            return;

        if (enemyHealth.IsDead)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        if (!HasLivingTarget())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Chase:
                HandleChaseState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
            case EnemyState.Dead:
                HandleDeadState();
                break;
        }
    }

    private void HandleIdleState()
    {
        Vector3 toPlayer = GetFlatOffsetToPlayer();

        if (IsWithinContactRange(toPlayer))
        {
            ChangeState(EnemyState.Attack);
            HandleAttackState();
            return;
        }

        ChangeState(EnemyState.Chase);
        HandleChaseState();
    }

    private void HandleChaseState()
    {
        Vector3 toPlayer = GetFlatOffsetToPlayer();

        if (IsWithinContactRange(toPlayer))
        {
            ChangeState(EnemyState.Attack);
            HandleAttackState();
            return;
        }

        MoveTowardsPlayer(toPlayer);
    }

    private void HandleAttackState()
    {
        Vector3 toPlayer = GetFlatOffsetToPlayer();

        if (!IsWithinContactRange(toPlayer))
        {
            ChangeState(EnemyState.Chase);
            HandleChaseState();
            return;
        }

        TryDamagePlayer();
    }

    private void HandleDeadState()
    {
    }

    private void MoveTowardsPlayer(Vector3 toPlayer)
    {
        Vector3 direction = toPlayer.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void TryDamagePlayer()
    {
        if (Time.time < nextDamageTime) return;

        playerHealth.TakeDamage(contactDamage);
        nextDamageTime = Time.time + contactCooldown;
    }

    private Vector3 GetFlatOffsetToPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;
        return toPlayer;
    }

    private bool IsWithinContactRange(Vector3 toPlayer)
    {
        return toPlayer.sqrMagnitude <= contactRangeSqr;
    }

    private bool HasLivingTarget()
    {
        return playerTransform != null && playerHealth != null && !playerHealth.IsDead;
    }

    private void ChangeState(EnemyState nextState, bool forceAnimatorUpdate = false)
    {
        if (currentState == nextState)
        {
            if (forceAnimatorUpdate)
                UpdateAnimatorState();

            return;
        }

        currentState = nextState;
        UpdateAnimatorState();
    }

    private void UpdateAnimatorState()
    {
        if (cachedAnimator == null)
            return;

        cachedAnimator.SetBool(IsChasingHash, currentState == EnemyState.Chase);
        cachedAnimator.SetBool(IsAttackingHash, currentState == EnemyState.Attack);
        cachedAnimator.SetBool(IsDeadHash, currentState == EnemyState.Dead);
    }
}
