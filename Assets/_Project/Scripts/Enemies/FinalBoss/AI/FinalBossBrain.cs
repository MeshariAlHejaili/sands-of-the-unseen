using UnityEngine;

public class FinalBossBrain
{
    private readonly FinalBossController boss;

    private float nextEvadeTime;
    private float nextFrostSlashTime;
    private float nextFakeOpeningTime;

    private readonly float evadeCooldown = 1.15f;
    private readonly float frostSlashCooldown = 1.4f;
    private readonly float fakeOpeningCooldown = 3.5f;

    public FinalBossBrain(FinalBossController boss)
    {
        this.boss = boss;
    }

    public IFinalBossState PickNextState()
    {
        bool canEvade = Time.time >= nextEvadeTime;
        bool canFrostSlash = Time.time >= nextFrostSlashTime;
        bool canFakeOpening = Time.time >= nextFakeOpeningTime;

        // Small bait pause, like an opening.
        if (canFakeOpening && Random.value < 0.18f)
        {
            nextFakeOpeningTime = Time.time + fakeOpeningCooldown;
            return new FinalBossFakeOpeningState();
        }

        // Main attack: projectile slash only.
        if (canFrostSlash)
        {
            nextFrostSlashTime = Time.time + frostSlashCooldown;
            return new FinalBossFrostSlashState();
        }

        // Defensive dodge/reposition.
        if (canEvade)
        {
            nextEvadeTime = Time.time + evadeCooldown;
            return new FinalBossEvadeDashState();
        }

        return new FinalBossIdleState();
    }
}