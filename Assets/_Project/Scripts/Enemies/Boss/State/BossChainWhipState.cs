using UnityEngine;

public class BossChainWhipState : IBossState
{
    private enum Phase { Telegraph, Active, Recovery }
    private Phase phase;
    private float phaseTimer;

    // SLOWER telegraph — gives player time to react and dash
    private const float TELEGRAPH = 1.0f;
    private const float ACTIVE    = 0.25f;
    private const float RECOVERY  = 0.7f;

    private const float WHIP_ARC = 180f;
    private const float WHIP_RADIUS = 9f;
    private const float WHIP_DAMAGE = 25f;

    private bool damageDealt;
    private Vector3 originalChainScale;

    public void Enter(BossController boss)
    {
        phase = Phase.Telegraph;
        phaseTimer = 0f;
        damageDealt = false;
        if (boss.chainPivot != null)
            originalChainScale = boss.chainPivot.localScale;
        Debug.Log("[Boss] Chain whip — TELEGRAPH (1 second to dodge)");
    }

    public void Tick(BossController boss)
    {
        phaseTimer += Time.deltaTime / boss.PhaseAttackSpeed;

        switch (phase)
        {
            case Phase.Telegraph:
                boss.FacePlayer(Time.deltaTime);
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / TELEGRAPH;
                    float yRot = Mathf.Lerp(0f, -120f, t);
                    float xRot = Mathf.Lerp(20f, -60f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(xRot, yRot, 0f);
                    boss.chainPivot.localScale = originalChainScale * Mathf.Lerp(1f, 1.3f, t);
                }
                if (phaseTimer >= TELEGRAPH)
                {
                    phase = Phase.Active;
                    phaseTimer = 0f;
                    Debug.Log("[Boss] Chain whip — SWING!");
                }
                break;

            case Phase.Active:
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / ACTIVE;
                    float yRot = Mathf.Lerp(-120f, 120f, t);
                    float xRot = Mathf.Lerp(-60f, 30f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(xRot, yRot, 0f);
                    boss.chainPivot.localScale = originalChainScale * 1.5f;
                }
                if (!damageDealt && phaseTimer >= ACTIVE * 0.5f)
                {
                    DoArcDamage(boss);
                    damageDealt = true;
                }
                if (phaseTimer >= ACTIVE)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                }
                break;

            case Phase.Recovery:
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / RECOVERY;
                    boss.chainPivot.localScale = originalChainScale * Mathf.Lerp(1.5f, 1f, t);
                }
                if (phaseTimer >= RECOVERY)
                    boss.RequestNextAction();
                break;
        }
    }

    private void DoArcDamage(BossController boss)
    {
        if (boss.player == null || boss.playerHealth == null || boss.playerHealth.IsDead) return;

        Vector3 toPlayer = boss.player.position - boss.transform.position;
        toPlayer.y = 0;
        float dist = toPlayer.magnitude;
        if (dist > WHIP_RADIUS) return;

        float angle = Vector3.Angle(boss.transform.forward, toPlayer);
        if (angle <= WHIP_ARC * 0.5f)
        {
            boss.playerHealth.TakeDamage(WHIP_DAMAGE);
            Debug.Log("[Boss] Whip HIT player");
        }
    }

    public void Exit(BossController boss)
    {
        if (boss.chainPivot != null)
            boss.chainPivot.localScale = originalChainScale;
    }
}