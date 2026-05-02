using UnityEngine;

public class BossIdleState : IBossState
{
    private float duration;
    private float timer;

    public void Enter(BossController boss)
    {
        // Phase 2 has shorter idles → more pressure
        if (boss.currentPhase >= 2)
            duration = Random.Range(0.3f, 0.7f);
        else
            duration = Random.Range(0.6f, 1.2f);
        timer = 0f;
    }

    public void Tick(BossController boss)
    {
        timer += Time.deltaTime / boss.PhaseAttackSpeed;
        boss.FacePlayer(Time.deltaTime);

        if (timer >= duration)
            boss.RequestNextAction();
    }

    public void Exit(BossController boss) { }
}