using UnityEngine;

public class BossGroundSlamState : IBossState
{
    private enum Phase { Windup, Telegraph, Impact, Recovery }
    private Phase phase;
    private float phaseTimer;
    private Vector3 slamCenter;
    private Vector3 originalChainScale;
    private Transform indicatorOriginalParent;

    private const float WINDUP    = 0.6f;
    private const float TELEGRAPH = 1.8f;
    private const float IMPACT    = 0.15f;
    private const float RECOVERY  = 0.8f;

    private const float SLAM_RADIUS = 5.5f;
    private const float SLAM_DAMAGE = 35f;

    // Unity's default plane is 10m wide at scale 1, so we multiply by 0.2 instead of 2
    // (radius 5.5m → diameter 11m → plane scale 1.1)
    private const float PLANE_SCALE_FACTOR = 0.2f;

    public void Enter(BossController boss)
    {
        phase = Phase.Windup;
        phaseTimer = 0f;
        if (boss.chainPivot != null)
            originalChainScale = boss.chainPivot.localScale;
        Debug.Log("[Boss] Ground slam — WINDUP");
    }

    public void Tick(BossController boss)
    {
        phaseTimer += Time.deltaTime / boss.PhaseAttackSpeed;

        switch (phase)
        {
            case Phase.Windup:
                boss.FacePlayer(Time.deltaTime);
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / WINDUP;
                    float xRot = Mathf.Lerp(20f, -180f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(xRot, 0f, 0f);
                    boss.chainPivot.localScale = originalChainScale * Mathf.Lerp(1f, 1.4f, t);
                }

                if (phaseTimer >= WINDUP)
                {
                    slamCenter = boss.player != null ? boss.player.position : boss.transform.position;
                    slamCenter.y = 0;
                    phase = Phase.Telegraph;
                    phaseTimer = 0f;

                    if (boss.groundIndicator != null)
                    {
                        boss.groundIndicator.gameObject.SetActive(true);
                        indicatorOriginalParent = boss.groundIndicator.parent;
                        boss.groundIndicator.SetParent(null, true);
                        boss.groundIndicator.position = slamCenter + Vector3.up * 0.05f;
                        boss.groundIndicator.rotation = Quaternion.identity;
                        boss.groundIndicator.localScale = new Vector3(
                            SLAM_RADIUS * PLANE_SCALE_FACTOR, 1f, SLAM_RADIUS * PLANE_SCALE_FACTOR);
                    }
                    Debug.Log($"[Boss] SLAM ZONE at {slamCenter} — DASH OUT! 1.8s");
                }
                break;

            case Phase.Telegraph:
                if (boss.groundIndicator != null)
                {
                    float urgency = phaseTimer / TELEGRAPH;
                    float pulseSpeed = Mathf.Lerp(8f, 25f, urgency);
                    float pulseAmount = 0.1f + urgency * 0.15f;
                    float pulse = 1f + Mathf.Sin(phaseTimer * pulseSpeed) * pulseAmount;
                    boss.groundIndicator.localScale = new Vector3(
                        SLAM_RADIUS * PLANE_SCALE_FACTOR * pulse, 1f, SLAM_RADIUS * PLANE_SCALE_FACTOR * pulse);
                }
                if (phaseTimer >= TELEGRAPH)
                {
                    phase = Phase.Impact;
                    phaseTimer = 0f;
                    DoSlamDamage(boss);
                }
                break;

            case Phase.Impact:
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / IMPACT;
                    float xRot = Mathf.Lerp(-180f, 90f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(xRot, 0f, 0f);
                }
                if (phaseTimer >= IMPACT)
                {
                    phase = Phase.Recovery;
                    phaseTimer = 0f;
                    if (boss.groundIndicator != null)
                    {
                        boss.groundIndicator.gameObject.SetActive(false);
                        if (indicatorOriginalParent != null)
                            boss.groundIndicator.SetParent(indicatorOriginalParent, true);
                    }
                }
                break;

            case Phase.Recovery:
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / RECOVERY;
                    boss.chainPivot.localScale = originalChainScale * Mathf.Lerp(1.4f, 1f, t);
                }
                if (phaseTimer >= RECOVERY)
                    boss.RequestNextAction();
                break;
        }
    }

    private void DoSlamDamage(BossController boss)
    {
        if (boss.player == null || boss.playerHealth == null || boss.playerHealth.IsDead) return;
        Vector3 playerPos = boss.player.position; playerPos.y = 0;
        if (Vector3.Distance(playerPos, slamCenter) <= SLAM_RADIUS)
        {
            boss.playerHealth.TakeDamage(SLAM_DAMAGE);
            Debug.Log("[Boss] Slam HIT player");
        }
    }

    public void Exit(BossController boss)
    {
        if (boss.groundIndicator != null)
        {
            boss.groundIndicator.gameObject.SetActive(false);
            if (indicatorOriginalParent != null)
                boss.groundIndicator.SetParent(indicatorOriginalParent, true);
        }
        if (boss.chainPivot != null)
            boss.chainPivot.localScale = originalChainScale;
    }
}