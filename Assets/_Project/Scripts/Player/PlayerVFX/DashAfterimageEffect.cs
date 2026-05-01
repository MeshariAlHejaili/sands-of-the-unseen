using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DashAfterimageEffect : MonoBehaviour, IDashEffect
{
    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer[] playerRenderers;

    [Header("Afterimage Settings")]
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private float spawnInterval = 0.04f;
    [SerializeField] private float ghostLifetime = 0.25f;
    [SerializeField] private int maxGhosts = 5;

    private Transform playerTransform;
    private Coroutine afterimageRoutine;

    public void Initialize(Transform owner)
    {
        playerTransform = owner;
    }

    public void OnDashStarted()
    {
        if (afterimageRoutine != null)
            StopCoroutine(afterimageRoutine);

        afterimageRoutine = StartCoroutine(SpawnAfterimages());
    }

    public void OnDashEnded()
    {
        // Nothing needed here for now.
    }

    public void Cleanup()
    {
        if (afterimageRoutine != null)
            StopCoroutine(afterimageRoutine);
    }

    private IEnumerator SpawnAfterimages()
    {
        int spawned = 0;

        while (spawned < maxGhosts)
        {
            CreateGhost();
            spawned++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void CreateGhost()
    {
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogWarning("No player renderers assigned for dash afterimage.", this);
            return;
        }

        if (ghostMaterial == null)
        {
            Debug.LogWarning("No ghost material assigned for dash afterimage.", this);
            return;
        }

        foreach (SkinnedMeshRenderer sourceRenderer in playerRenderers)
        {
            if (sourceRenderer == null) continue;

            Mesh bakedMesh = new Mesh();
            sourceRenderer.BakeMesh(bakedMesh);

            GameObject ghost = new GameObject("Dash_Afterimage_Ghost");

            ghost.transform.SetPositionAndRotation(
                sourceRenderer.transform.position,
                sourceRenderer.transform.rotation
            );

            ghost.transform.localScale = Vector3.one;

            MeshFilter meshFilter = ghost.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = ghost.AddComponent<MeshRenderer>();

            meshFilter.mesh = bakedMesh;
            meshRenderer.material = new Material(ghostMaterial);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            StartCoroutine(FadeAndDestroy(meshRenderer, ghost, bakedMesh));
        }
    }

    private IEnumerator FadeAndDestroy(MeshRenderer meshRenderer, GameObject ghost, Mesh mesh)
    {
        float timer = 0f;
        Material material = meshRenderer.material;
        Color startColor = material.color;

        while (timer < ghostLifetime)
        {
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / ghostLifetime);
            material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(mesh);
        Destroy(material);
        Destroy(ghost);
    }
}