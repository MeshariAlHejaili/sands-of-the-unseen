using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody rb;
    public EnemyHealth health;

    [Header("Visuals")]
    public Transform chainPivot;
    public Transform groundIndicator;
    public Renderer bossRenderer;

    [Header("Stats")]
    public float bossMaxHealth = 1500f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 6f;

    [Header("Combat Ranges")]
    public float whipRange = 8f;
    public float hurlMinRange = 14f;
    public float slamCooldown = 5f;
    public float sweepCooldown = 12f;

    [Header("Phase Thresholds")]
    [Range(0f, 1f)] public float phase2Threshold = 0.75f;

    [Header("Phase 2")]
    [Tooltip("0.9 means attacks are 10% faster.")]
    [Range(0.1f, 1f)] public float phase2AttackSpeedMultiplier = 0.9f;

    public Color phase1Color = Color.white;
    public Color phase2Color = new Color(0.55f, 0f, 1f);

    public float PhaseAttackSpeed => currentPhase >= 2 ? phase2AttackSpeedMultiplier : 1f;

    [Header("Debug")]
    public string currentStateName;
    public int currentPhase = 1;
    public float lastSlamTime = -999f;
    public float lastSweepTime = -999f;
    public int recentAttackId = -1;

    private IBossState currentState;

    public BossBrain brain { get; private set; }
    public PlayerHealth playerHealth { get; private set; }

    public System.Action OnPhase2Started;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (bossRenderer == null) bossRenderer = GetComponentInChildren<Renderer>();

        if (health != null)
        {
            health.Init(bossMaxHealth);
            health.Died += OnBossDied;
        }

        if (bossRenderer != null)
            bossRenderer.material.color = phase1Color;

        brain = new BossBrain(this);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.Died -= OnBossDied;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        ChangeState(new BossIdleState());
    }

    private void Update()
    {
        if (health != null && health.IsDead)
            return;

        if (currentPhase == 1 && HealthPercent <= phase2Threshold)
        {
            currentPhase = 2;
            OnPhase2Started?.Invoke();
            ChangeState(new BossPhaseTransitionState());
            return;
        }

        currentState?.Tick(this);
    }

    public void ChangeState(IBossState newState)
    {
        currentState?.Exit(this);

        currentState = newState;
        currentStateName = newState.GetType().Name;

        currentState.Enter(this);
    }

    public void RequestNextAction()
    {
        IBossState next = brain.DecideNextState();
        ChangeState(next);
    }

    public float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        Vector3 a = transform.position;
        a.y = 0f;

        Vector3 b = player.position;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    public Vector3 DirectionToPlayer()
    {
        if (player == null)
            return transform.forward;

        Vector3 d = player.position - transform.position;
        d.y = 0f;

        return d.sqrMagnitude > 0.001f ? d.normalized : transform.forward;
    }

    public float HealthPercent
    {
        get
        {
            if (health == null || health.MaxHealth <= 0f)
                return 1f;

            return health.CurrentHealth / health.MaxHealth;
        }
    }

    public void FacePlayer(float deltaTime)
    {
        Vector3 dir = DirectionToPlayer();

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, deltaTime * turnSpeed);
    }

    private void OnBossDied()
    {
        Debug.Log("[Boss] Defeated — player wins!");
        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, whipRange);

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, hurlMinRange);
    }
}