using UnityEngine;

public class BossPhaseTransitionState : IBossState
{
    private const float Duration = 2.0f;

    private float timer;

    public void Enter(BossController boss)
    {
        timer = 0f;

        Debug.Log("[Boss] Phase transition started — boss stops attacking.");

        if (boss.rb != null)
        {
            boss.rb.linearVelocity = Vector3.zero;
            boss.rb.angularVelocity = Vector3.zero;
        }

        if (boss.bossRenderer != null)
        {
            boss.bossRenderer.material.color = boss.phase2Color;
        }

        if (boss.chainPivot != null)
        {
            boss.chainPivot.localRotation = Quaternion.identity;
        }

        if (boss.groundIndicator != null)
        {
            boss.groundIndicator.gameObject.SetActive(false);
        }
    }

    public void Tick(BossController boss)
    {
        timer += Time.deltaTime;

        boss.FacePlayer(Time.deltaTime);

        if (timer >= Duration)
        {
            Debug.Log("[Boss] Phase transition finished — Phase 2 active.");
            boss.RequestNextAction();
        }
    }

    public void Exit(BossController boss)
    {
    }
}