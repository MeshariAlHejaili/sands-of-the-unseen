/// <summary>
/// Final boss state contract. Mirrors IBossState but exposes punish-window
/// metadata so FinalBossArmor can decide whether to reduce damage based on
/// what the boss is currently doing (e.g., recovering from a heavy combo).
/// </summary>
public interface IFinalBossState
{
    void Enter(FinalBossController boss);
    void Tick(FinalBossController boss);
    void Exit(FinalBossController boss);

    /// <summary>True when the player should be able to land big hits during this state.</summary>
    bool IsPunishable { get; }

    /// <summary>Damage multiplier applied on top of armor logic (1.0 = normal, 1.5 = punish reward).</summary>
    float DamageMultiplier { get; }

    /// <summary>If true, FinalBossArmor ignores facing angle for this state.</summary>
    bool BypassFrontArmor { get; }
}
