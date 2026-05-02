using UnityEngine;

public class BossChainSweepState : IBossState
{
    private enum Phase { Windup, Spinning, Recovery }
    private Phase phase;
    private float phaseTimer;
    private float damageTickTimer;

    private const float WINDUP    = 0.9f;
    private const float SPINNING  = 3.0f;
    private const float RECOVERY  = 1.0f;
    private const float SWEEP_RADIUS = 12f;
    private const float SWEEP_DAMAGE_PER_TICK = 12f;
    private const float DAMAGE_TICK_RATE = 0.3f;

    public void Enter(BossController boss)
    {
        phase = Phase.Windup;
        phaseTimer = 0f;
        damageTickTimer = 0f;
        Debug.Log("[Boss] Chain sweep — WINDUP (RUN!)");
    }

    public void Tick(BossController boss)
    {
        phaseTimer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Windup:
                if (phaseTimer >= WINDUP)
                {
                    phase = Phase.Spinning;
                    phaseTimer = 0f;
                    Debug.Log("[Boss] Chain sweep — ACTIVE");
                }
                break;

            case Phase.Spinning:
                // Spin in place visually; damage anyone in radius
                damageTickTimer += Time.deltaTime / boss.PhaseAttackSpeed;
                boss.transform.Rotate(Vector3.up, 720f * Time.deltaTime / boss.PhaseAttackSpeed);
                if (damageTickTimer >= DAMAGE_TICK_RATE)
                {
                    damageTickTimer = 0f;
                    if (boss.DistanceToPlayer() <= SWEEP_RADIUS)
                    {
                        var ph = boss.player.GetComponent<PlayerHealth>();
                        if (ph != null) ph.TakeDamage(SWEEP_DAMAGE_PER_TICK);
                    }
                }
                if (phaseTimer >= SPINNING)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Recovery:
                if (phaseTimer >= RECOVERY)
                    boss.RequestNextAction();
                break;
        }
    }

    public void Exit(BossController boss) { }
}