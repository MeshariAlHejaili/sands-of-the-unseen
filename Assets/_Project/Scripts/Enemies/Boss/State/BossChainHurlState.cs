using UnityEngine;

public class BossChainHurlState : IBossState
{
    private enum Phase { Telegraph, Launch, Recovery }
    private Phase phase;
    private float phaseTimer;
    private Vector3 lockedDirection;

    private const float TELEGRAPH = 1.0f;  // longer windup
    private const float LAUNCH    = 0.3f;  // brief moment when projectile spawns
    private const float RECOVERY  = 0.8f;

    private const float HURL_RANGE = 25f;
    private const float HURL_PROJECTILE_SPEED = 18f;
    private const float HURL_WIDTH = 1.8f;
    private const float HURL_DAMAGE = 30f;

    private Vector3 originalChainScale;
    private bool projectileSpawned;

    public void Enter(BossController boss)
    {
        phase = Phase.Telegraph;
        phaseTimer = 0f;
        projectileSpawned = false;
        if (boss.chainPivot != null)
            originalChainScale = boss.chainPivot.localScale;
        Debug.Log("[Boss] Chain hurl — TELEGRAPH (1s to dodge)");
    }

    public void Tick(BossController boss)
    {
        phaseTimer += Time.deltaTime / boss.PhaseAttackSpeed;

        switch (phase)
        {
            case Phase.Telegraph:
                if (phaseTimer < TELEGRAPH * 0.7f)
                    boss.FacePlayer(Time.deltaTime);

                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / TELEGRAPH;
                    float yRot = Mathf.Lerp(0f, -150f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(45f, yRot, 0f);
                }

                if (phaseTimer >= TELEGRAPH)
                {
                    lockedDirection = boss.DirectionToPlayer();
                    phase = Phase.Launch;
                    phaseTimer = 0f;
                    Debug.Log("[Boss] Chain hurl — LAUNCHED!");

                    // Spawn the visible projectile
                    SpawnProjectile(boss);
                    projectileSpawned = true;
                }
                break;

            case Phase.Launch:
                // Chain visually swings forward
                if (boss.chainPivot != null)
                {
                    float t = phaseTimer / LAUNCH;
                    float yRot = Mathf.Lerp(-150f, 30f, t);
                    boss.chainPivot.localRotation = Quaternion.Euler(45f, yRot, 0f);
                }
                if (phaseTimer >= LAUNCH)
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

    private void SpawnProjectile(BossController boss)
    {
        // Create the visible mace projectile
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "BossChainProjectile";
        projectile.transform.position = boss.transform.position + Vector3.up * 1.5f + lockedDirection * 2f;
        projectile.transform.localScale = Vector3.one * 1.5f;

        // Make it bright orange so it's super visible
        Renderer rend = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.4f, 0.1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(2f, 0.8f, 0.2f));
        rend.material = mat;

        // Remove default collider — we'll handle our own hit detection
        Object.Destroy(projectile.GetComponent<Collider>());

        // Add behaviour to move it and damage on contact
        var movement = projectile.AddComponent<BossChainProjectile>();
        movement.Init(lockedDirection, HURL_PROJECTILE_SPEED, HURL_RANGE, HURL_WIDTH, HURL_DAMAGE, boss.playerHealth);
    }

    public void Exit(BossController boss)
    {
        if (boss.chainPivot != null)
            boss.chainPivot.localScale = originalChainScale;
    }
}