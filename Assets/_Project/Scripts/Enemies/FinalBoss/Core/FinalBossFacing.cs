using UnityEngine;

/// <summary>
/// Single-responsibility facing helper. States call FaceTarget(dt) to smoothly rotate
/// toward the player. FinalBossArmor reads IsAngleInsideFrontCone to decide if a hit
/// is reduced.
/// </summary>
[RequireComponent(typeof(FinalBossStats))]
public class FinalBossFacing : MonoBehaviour
{
    private FinalBossStats stats;
    private Transform target;

    private void Awake()
    {
        stats = GetComponent<FinalBossStats>();
    }

    public void SetTarget(Transform t) => target = t;

    /// <summary>Smoothly rotate toward the target on the Y axis only.</summary>
    public void FaceTarget(float deltaTime)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion want = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, want, deltaTime * stats.TurnSpeed);
    }

    /// <summary>Snaps facing instantly. Use sparingly — only at moments where a player can't punish (e.g., dash start).</summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    /// <summary>True if the given world-space hit direction lands inside the front-armor cone.</summary>
    public bool IsHitFromFront(Vector3 hitDirectionFromBoss)
    {
        Vector3 flatHit = hitDirectionFromBoss; flatHit.y = 0f;
        if (flatHit.sqrMagnitude < 0.0001f) return false;
        float angle = Vector3.Angle(transform.forward, flatHit.normalized);
        return angle <= stats.FrontArmorHalfAngle;
    }
}
