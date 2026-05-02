using UnityEngine;

public class FinalBossFakeOpeningState : IFinalBossState
{
    public bool IsPunishable => true;
    public float DamageMultiplier => 1.25f;
    public bool BypassFrontArmor => false;

    private float timer;

    private const float FakeOpeningDuration = 0.55f;

    public void Enter(FinalBossController boss)
    {
        timer = FakeOpeningDuration;
    }

    public void Tick(FinalBossController boss)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
            boss.RequestNextAction();
    }

    public void Exit(FinalBossController boss)
    {
    }
}