using UnityEngine;

public class FinalBossEvadeDashState : IFinalBossState
{
    public bool IsPunishable => false;
    public float DamageMultiplier => 1f;
    public bool BypassFrontArmor => false;

    private Vector3 dashDirection;
    private float timer;

    private const float DashDuration = 0.22f;
    private const float DashSpeed = 14f;

    public void Enter(FinalBossController boss)
    {
        timer = DashDuration;

        Vector3 toPlayer = boss.DirectionToPlayer();
        Vector3 side = Vector3.Cross(Vector3.up, toPlayer).normalized;

        if (Random.value < 0.5f)
            side = -side;

        Vector3 backward = -toPlayer;
        dashDirection = (side * 0.85f + backward * 0.15f).normalized;

        // Dash VFX can be called here later.
        // Example:
        // boss.DashVFX.Play();
    }

    public void Tick(FinalBossController boss)
    {
        timer -= Time.deltaTime;

        boss.transform.position += dashDirection * DashSpeed * Time.deltaTime;

        if (timer <= 0f)
            boss.RequestNextAction();
    }

    public void Exit(FinalBossController boss)
    {
    }
}