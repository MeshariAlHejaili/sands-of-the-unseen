using UnityEngine;

/// <summary>
/// Orchestrator for the final boss. Wires together component systems and drives the state machine.
/// Armor is removed for now.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(FinalBossStats))]
[RequireComponent(typeof(FinalBossStamina))]
[RequireComponent(typeof(FinalBossFacing))]
[RequireComponent(typeof(FinalBossPlayerSensor))]
public class FinalBossController : MonoBehaviour
{
    [Header("Debug — read-only")]
    [SerializeField] private string currentStateName;
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private int recentActionId = -1;

    public EnemyHealth Health { get; private set; }
    public FinalBossStats Stats { get; private set; }
    public FinalBossStamina Stamina { get; private set; }
    public FinalBossFacing Facing { get; private set; }
    public FinalBossPlayerSensor Sensor { get; private set; }
    public FinalBossTelegraph Telegraph { get; private set; }
    public FinalBossSwordAnimator SwordAnimator { get; private set; }
    public FinalBossAnimatorDriver AnimatorDriver { get; private set; }
    public FinalBossBrain Brain { get; private set; }

    public Transform Player { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }

    public IFinalBossState CurrentState { get; private set; }

    public int CurrentPhase => currentPhase;
    public static System.Action<FinalBossController> OnBossSpawned;

    public int RecentActionId
    {
        get => recentActionId;
        set => recentActionId = value;
    }

    public System.Action OnPhase2Started;

    private bool initialized;

    private void Awake()
    {
        Health = GetComponent<EnemyHealth>();
        Stats = GetComponent<FinalBossStats>();
        Stamina = GetComponent<FinalBossStamina>();
        Facing = GetComponent<FinalBossFacing>();
        Sensor = GetComponent<FinalBossPlayerSensor>();

        Telegraph = GetComponentInChildren<FinalBossTelegraph>();
        SwordAnimator = GetComponentInChildren<FinalBossSwordAnimator>();
        AnimatorDriver = GetComponentInChildren<FinalBossAnimatorDriver>();

        Health.Init(Stats.MaxHealth);

        Health.Died += HandleDied;
        Stamina.Depleted += HandleStaminaDepleted;

        Brain = new FinalBossBrain(this);
    }

    private void Start()
    {
        AcquirePlayer();
        
        OnBossSpawned?.Invoke(this);
        ChangeState(new FinalBossIdleState());
        initialized = true;
    }

    private void OnDestroy()
    {
        if (Health != null)
            Health.Died -= HandleDied;

        if (Stamina != null)
            Stamina.Depleted -= HandleStaminaDepleted;
    }

    private void Update()
    {
        if (!initialized) return;
        if (Health.IsDead) return;

        if (currentPhase == 1 &&
            Health.MaxHealth > 0f &&
            Health.CurrentHealth / Health.MaxHealth <= Stats.Phase2HealthThreshold)
        {
            currentPhase = 2;
            OnPhase2Started?.Invoke();

            Debug.Log("[FinalBoss] Phase 2 reached.");
        }

        CurrentState?.Tick(this);
    }

    private void AcquirePlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p == null)
        {
            Debug.LogWarning("[FinalBoss] No GameObject tagged 'Player' found.");
            return;
        }

        Player = p.transform;
        PlayerHealth = p.GetComponent<PlayerHealth>();

        Facing.SetTarget(Player);
        Sensor.SetPlayer(Player, PlayerHealth);
    }

    public void ChangeState(IFinalBossState next)
    {
        CurrentState?.Exit(this);

        CurrentState = next;
        currentStateName = next != null ? next.GetType().Name : "<null>";

        next?.Enter(this);
    }

    public void RequestNextAction()
    {
        if (Health.IsDead) return;

        IFinalBossState next = Brain.PickNextState();
        ChangeState(next);
    }

    public float DistanceToPlayer()
    {
        if (Player == null) return float.MaxValue;

        Vector3 a = transform.position;
        Vector3 b = Player.position;

        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
    }

    public Vector3 DirectionToPlayer()
    {
        if (Player == null)
            return transform.forward;

        Vector3 d = Player.position - transform.position;
        d.y = 0f;

        return d.sqrMagnitude > 0.0001f
            ? d.normalized
            : transform.forward;
    }

    private void HandleDied()
    {
        Debug.Log("[FinalBoss] Defeated.");

        ChangeState(null);

        if (AnimatorDriver != null)
            AnimatorDriver.SetDead();

        enabled = false;
    }

    private void HandleStaminaDepleted()
    {
        if (Health.IsDead) return;

        ChangeState(new FinalBossStaggerState());
    }

    private void OnDrawGizmosSelected()
    {
        FinalBossStats s = GetComponent<FinalBossStats>();
        if (s == null) return;

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, s.MeleeRange);

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, s.FarRange);
    }
}