using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Guid = System.Guid;
using Random = System.Random;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class ArenaLayoutGenerator : MonoBehaviour
{
    private const string RuntimeGeneratedParentName = "_RuntimeArenaDressing";
    private const string PreviewGeneratedParentName = "_PreviewArenaDressing";
    private const string LegacyGeneratedParentName = "_GeneratedArenaDressing";
    private const int MaxPlacementAttempts = 200;

    [Header("Generation")]
    [Tooltip("Whether a new arena layout is generated during Awake before wave spawning starts.")]
    [SerializeField] private bool generateOnAwake = true;

    [Tooltip("Whether generation uses the fixed seed for reproducible debugging instead of a fresh runtime seed.")]
    [SerializeField] private bool useFixedSeed;

    [Tooltip("Deterministic seed used when fixed seed mode is enabled.")]
    [Min(0)]
    [SerializeField] private int fixedSeed = 20260504;

    [Tooltip("Forces graveyards to generate when previewing or playtesting layout readability.")]
    [SerializeField] private bool forceGraveyardsForTesting;

    [Tooltip("Forces rock clusters and their chests to generate when previewing or playtesting layout readability.")]
    [SerializeField] private bool forceRockClustersForTesting;

    [Tooltip("Arena bounds source used for runtime spawn anchors and procedural prop placement. Falls back to the bounds below if empty.")]
    [SceneObjectsOnly]
    [SerializeField] private ArenaMapBounds mapBounds;

    [Tooltip("Minimum X/Z placement bounds in world units for generated props.")]
    [SerializeField] private Vector2 arenaMinBounds = new Vector2(-25f, -25f);

    [Tooltip("Maximum X/Z placement bounds in world units for generated props.")]
    [SerializeField] private Vector2 arenaMaxBounds = new Vector2(25f, 25f);

    [Tooltip("Minimum spacing in world units between major generated blocking props.")]
    [Min(0.5f)]
    [SerializeField] private float obstacleSpacing = 4f;

    [Space]
    [Header("Runtime Spawn Randomization")]
    [Tooltip("Minimum distance in world units between randomized player and boss spawn anchors.")]
    [Min(1f)]
    [SerializeField] private float minBossDistanceFromPlayer = 24f;

    [Tooltip("Distance in world units used to place the portal spawn near the boss spawn.")]
    [Min(0f)]
    [SerializeField] private float portalDistanceFromBoss = 4f;

    [Tooltip("How far enemy spawn points are inset from the arena edge in world units.")]
    [Min(0f)]
    [SerializeField] private float enemySpawnEdgeInset = 2f;

    [Tooltip("Maximum random angle offset in degrees applied to distributed enemy spawn points.")]
    [Range(0f, 45f)]
    [SerializeField] private float enemySpawnAngleJitterDegrees = 12f;

    [Tooltip("Radius in world units kept clear along enemy approach lanes.")]
    [Min(0f)]
    [SerializeField] private float enemyLaneClearRadius = 2.5f;

    [Tooltip("Number of clear samples placed between each enemy spawn point and the player area.")]
    [Range(0, 12)]
    [SerializeField] private int enemyLaneClearSteps = 5;

    [Space]
    [Header("Prefabs")]
    [Tooltip("Rock prefabs used for individual rocks and rock clusters.")]
    [AssetsOnly]
    [SerializeField] private GameObject[] rockPrefabs;

    [Tooltip("Cactus prefabs used for light desert dressing.")]
    [AssetsOnly]
    [SerializeField] private GameObject[] cactusPrefabs;

    [Tooltip("Grave prefabs used inside rare graveyards.")]
    [AssetsOnly]
    [SerializeField] private GameObject[] gravePrefabs;

    [Tooltip("Treasure chest prefab placed near each generated rock cluster.")]
    [AssetsOnly]
    [SerializeField] private GameObject chestPrefab;

    [Tooltip("Collectible currency orb prefab placed around generated graves.")]
    [AssetsOnly]
    [SerializeField] private CurrencyOrbPickup currencyOrbPrefab;

    [Space]
    [Header("Scene Anchors")]
    [Tooltip("Player spawn or player transform that must stay clear of generated props.")]
    [SceneObjectsOnly]
    [SerializeField] private Transform playerAnchor;

    [Tooltip("Boss spawn transform that must stay clear of generated props.")]
    [SceneObjectsOnly]
    [SerializeField] private Transform bossSpawnAnchor;

    [Tooltip("Portal spawn transform that must stay clear of generated props.")]
    [SceneObjectsOnly]
    [SerializeField] private Transform portalSpawnAnchor;

    [Tooltip("Enemy spawn transforms that must stay clear of generated props.")]
    [SceneObjectsOnly]
    [SerializeField] private Transform[] enemySpawnAnchors;

    [Tooltip("Clear radius in world units around the player spawn.")]
    [Min(0f)]
    [SerializeField] private float playerClearRadius = 8f;

    [Tooltip("Clear radius in world units around the boss spawn.")]
    [Min(0f)]
    [SerializeField] private float bossClearRadius = 9f;

    [Tooltip("Clear radius in world units around the portal spawn.")]
    [Min(0f)]
    [SerializeField] private float portalClearRadius = 6f;

    [Tooltip("Clear radius in world units around each enemy spawn point.")]
    [Min(0f)]
    [SerializeField] private float enemySpawnClearRadius = 7f;

    [Space]
    [Header("Graveyards")]
    [Tooltip("Chance from 0 to 1 that any graveyards are generated.")]
    [Range(0f, 1f)]
    [SerializeField] private float graveyardChance = 0.5f;

    [Tooltip("Maximum number of graveyards generated when the graveyard chance succeeds.")]
    [Range(0, 3)]
    [SerializeField] private int maxGraveyards = 3;

    [Tooltip("Minimum number of grave props inside each generated graveyard.")]
    [Range(1, 6)]
    [SerializeField] private int minGravesPerGraveyard = 2;

    [Tooltip("Maximum number of grave props inside each generated graveyard.")]
    [Range(1, 6)]
    [SerializeField] private int maxGravesPerGraveyard = 4;

    [Tooltip("Radius in world units used when placing graves around each graveyard center.")]
    [Min(1f)]
    [SerializeField] private float graveyardRadius = 4f;

    [Tooltip("Number of real collectible currency orbs placed around each generated grave.")]
    [Range(0, 6)]
    [SerializeField] private int coinsPerGrave = 3;

    [Tooltip("Currency amount assigned to each generated grave coin.")]
    [Min(1)]
    [SerializeField] private int graveCoinValue = 1;

    [Tooltip("Radius in world units used to place coins around each generated grave.")]
    [Min(0.5f)]
    [SerializeField] private float graveCoinRadius = 1.4f;

    [Space]
    [Header("Rock Clusters")]
    [Tooltip("Chance from 0 to 1 that any rock clusters are generated.")]
    [Range(0f, 1f)]
    [SerializeField] private float rockClusterChance = 0.25f;

    [Tooltip("Maximum number of rock clusters generated when the rock cluster chance succeeds.")]
    [Range(0, 3)]
    [SerializeField] private int maxRockClusters = 3;

    [Tooltip("Minimum number of rock props inside each generated rock cluster.")]
    [Range(1, 12)]
    [SerializeField] private int minRocksPerCluster = 4;

    [Tooltip("Maximum number of rock props inside each generated rock cluster.")]
    [Range(1, 12)]
    [SerializeField] private int maxRocksPerCluster = 7;

    [Tooltip("Radius in world units used when placing rocks around each rock cluster center.")]
    [Min(1f)]
    [SerializeField] private float rockClusterRadius = 4.5f;

    [Tooltip("Distance in world units from a rock cluster center to its nearby chest.")]
    [Min(1f)]
    [SerializeField] private float chestDistanceFromCluster = 5.5f;

    [Tooltip("Extra scale multiplier applied only to generated chests so they read from the top-down camera.")]
    [Range(0.5f, 4f)]
    [SerializeField] private float chestScaleMultiplier = 2f;

    [Tooltip("Vertical ground offset in world units applied to generated chests to prevent z-fighting or slight sinking.")]
    [Min(0f)]
    [SerializeField] private float chestGroundOffset = 0.05f;

    [Tooltip("Whether generated chest LOD groups are disabled so all chest renderers stay visible from the gameplay camera.")]
    [SerializeField] private bool disableChestLodGroups = true;

    [Tooltip("Point light intensity added to generated chests for readability in the night arena.")]
    [Range(0f, 4f)]
    [SerializeField] private float chestGlowIntensity = 1.25f;

    [Tooltip("Point light range in world units added to generated chests for readability in the night arena.")]
    [Range(0f, 8f)]
    [SerializeField] private float chestGlowRange = 3.5f;

    [Space]
    [Header("Loose Dressing")]
    [Tooltip("Number of individual rocks generated outside rock clusters.")]
    [Range(0, 80)]
    [SerializeField] private int individualRockCount = 18;

    [Tooltip("Number of cactus props generated outside rock clusters.")]
    [Range(0, 30)]
    [SerializeField] private int cactusCount = 6;

    [Tooltip("Minimum random scale multiplier applied to generated props.")]
    [Range(0.1f, 3f)]
    [SerializeField] private float minPropScale = 0.85f;

    [Tooltip("Maximum random scale multiplier applied to generated props.")]
    [Range(0.1f, 3f)]
    [SerializeField] private float maxPropScale = 1.25f;

    private int runtimeSeed;

    [ShowInInspector, ReadOnly]
    private int RuntimeSeed => runtimeSeed;

    private Vector2 ActiveArenaMinBounds => mapBounds != null ? mapBounds.MinBounds : arenaMinBounds;

    private Vector2 ActiveArenaMaxBounds => mapBounds != null ? mapBounds.MaxBounds : arenaMaxBounds;

    private readonly struct PlacementBlock
    {
        public readonly Vector3 Position;
        public readonly float Radius;

        public PlacementBlock(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }

    private void Awake()
    {
        if (generateOnAwake && Application.isPlaying)
        {
            GenerateRuntimeLayout();
        }
    }

    [Button("Generate Preview Layout")]
    [ContextMenu("Generate Preview Layout")]
    private void GeneratePreviewLayout()
    {
        GenerateLayout(PreviewGeneratedParentName, true);
    }

    private void GenerateRuntimeLayout()
    {
        GenerateLayout(RuntimeGeneratedParentName, false);
    }

    private void GenerateLayout(string generatedParentName, bool isEditorPreview)
    {
        ClearGeneratedRoot(generatedParentName);
        ClearGeneratedRoot(LegacyGeneratedParentName);

        runtimeSeed = ResolveSeed();
        Random random = new Random(runtimeSeed);
        int worldLayer = LayerMask.NameToLayer("World");
        int pickupLayer = LayerMask.NameToLayer("Pickup");

        RandomizeSceneAnchors(random);

        Transform root = CreateContainer(generatedParentName, transform);
        if (isEditorPreview)
        {
            SetHideFlagsRecursively(root.gameObject, HideFlags.DontSaveInEditor);
        }

        List<PlacementBlock> occupied = BuildInitialBlocks();

        GenerateGraveyards(random, root, occupied, worldLayer, pickupLayer);
        GenerateRockClusters(random, root, occupied, worldLayer);
        GenerateLooseDressing(random, root, occupied, worldLayer);
        if (isEditorPreview)
        {
            SetHideFlagsRecursively(root.gameObject, HideFlags.DontSaveInEditor);
        }

        Physics.SyncTransforms();
        MarkSceneDirty(isEditorPreview);
    }

    [Button("Clear Preview Layout")]
    [ContextMenu("Clear Preview Layout")]
    private void ClearPreviewLayout()
    {
        ClearGeneratedRoot(PreviewGeneratedParentName);
        ClearGeneratedRoot(LegacyGeneratedParentName);
        MarkSceneDirty(true);
    }

    private void ClearGeneratedRoot(string generatedParentName)
    {
        Transform existing = transform.Find(generatedParentName);
        if (existing == null)
        {
            return;
        }

        ClearEditorSelectionIfInside(existing);

        if (Application.isPlaying)
        {
            existing.gameObject.SetActive(false);
            Destroy(existing.gameObject);
        }
        else
        {
            DestroyEditorRootSafely(existing.gameObject);
        }
    }

    private int ResolveSeed()
    {
        if (useFixedSeed)
        {
            return Mathf.Max(0, fixedSeed);
        }

        return Guid.NewGuid().GetHashCode() & int.MaxValue;
    }

    private void RandomizeSceneAnchors(Random random)
    {
        List<PlacementBlock> reserved = new List<PlacementBlock>();

        if (playerAnchor != null && TryGetOpenPosition(random, playerClearRadius, reserved, out Vector3 playerPosition))
        {
            MoveAnchor(playerAnchor, playerPosition);
            reserved.Add(new PlacementBlock(playerPosition, playerClearRadius));
        }

        if (bossSpawnAnchor != null && TryGetBossSpawnPosition(random, reserved, out Vector3 bossPosition))
        {
            MoveAnchor(bossSpawnAnchor, bossPosition);
            reserved.Add(new PlacementBlock(bossPosition, bossClearRadius));
        }

        RandomizePortalAnchor(random);
        RandomizeEnemySpawnAnchors(random);
    }

    private bool TryGetBossSpawnPosition(Random random, List<PlacementBlock> reserved, out Vector3 position)
    {
        for (int i = 0; i < MaxPlacementAttempts; i++)
        {
            if (!TryGetOpenPosition(random, bossClearRadius, reserved, out position))
            {
                break;
            }

            if (playerAnchor == null || HorizontalDistance(position, playerAnchor.position) >= minBossDistanceFromPlayer)
            {
                return true;
            }
        }

        position = bossSpawnAnchor != null ? bossSpawnAnchor.position : GetArenaCenter();
        return false;
    }

    private void RandomizePortalAnchor(Random random)
    {
        if (portalSpawnAnchor == null)
        {
            return;
        }

        Vector3 bossPosition = bossSpawnAnchor != null ? bossSpawnAnchor.position : GetArenaCenter();
        float baseAngle = RandomAngle(random);

        for (int i = 0; i < 16; i++)
        {
            float angle = baseAngle + (i * Mathf.PI * 0.125f);
            Vector3 portalPosition = bossPosition + DirectionFromAngle(angle) * portalDistanceFromBoss;
            if (IsInsideBounds(portalPosition, portalClearRadius))
            {
                MoveAnchor(portalSpawnAnchor, portalPosition);
                return;
            }
        }

        MoveAnchor(portalSpawnAnchor, bossPosition);
    }

    private void RandomizeEnemySpawnAnchors(Random random)
    {
        if (enemySpawnAnchors == null || enemySpawnAnchors.Length == 0)
        {
            return;
        }

        int validCount = CountValidTransforms(enemySpawnAnchors);
        if (validCount == 0)
        {
            return;
        }

        float baseAngle = RandomAngle(random);
        int validIndex = 0;

        for (int i = 0; i < enemySpawnAnchors.Length; i++)
        {
            Transform enemySpawnAnchor = enemySpawnAnchors[i];
            if (enemySpawnAnchor == null)
            {
                continue;
            }

            float angleStep = Mathf.PI * 2f / validCount;
            float jitter = RandomRange(
                random,
                -enemySpawnAngleJitterDegrees * Mathf.Deg2Rad,
                enemySpawnAngleJitterDegrees * Mathf.Deg2Rad);
            float angle = baseAngle + (validIndex * angleStep) + jitter;
            MoveAnchor(enemySpawnAnchor, GetPerimeterPosition(angle, enemySpawnEdgeInset));
            validIndex++;
        }
    }

    private void GenerateGraveyards(Random random, Transform root, List<PlacementBlock> occupied, int worldLayer, int pickupLayer)
    {
        Transform group = CreateContainer("Graveyards", root);
        if ((!forceGraveyardsForTesting && !Roll(random, graveyardChance)) || maxGraveyards <= 0)
        {
            return;
        }

        int graveyardCount = random.Next(1, maxGraveyards + 1);
        int minGraves = Mathf.Min(minGravesPerGraveyard, maxGravesPerGraveyard);
        int maxGraves = Mathf.Max(minGravesPerGraveyard, maxGravesPerGraveyard);

        for (int i = 0; i < graveyardCount; i++)
        {
            if (!TryGetOpenPosition(random, graveyardRadius + obstacleSpacing, occupied, out Vector3 center))
            {
                continue;
            }

            Transform graveyard = CreateContainer($"Graveyard_{i + 1:00}", group);
            graveyard.position = center;
            int graveCount = random.Next(minGraves, maxGraves + 1);
            float baseAngle = RandomAngle(random);

            for (int j = 0; j < graveCount; j++)
            {
                GameObject prefab = GetRandomPrefab(gravePrefabs, random);
                if (prefab == null)
                {
                    continue;
                }

                float angle = baseAngle + (j * Mathf.PI * 2f / graveCount) + RandomRange(random, -0.35f, 0.35f);
                float distance = RandomRange(random, graveyardRadius * 0.25f, graveyardRadius);
                Vector3 gravePosition = center + DirectionFromAngle(angle) * distance;

                if (!IsInsideBounds(gravePosition, obstacleSpacing))
                {
                    continue;
                }

                GameObject grave = SpawnBlockingProp(prefab, gravePosition, graveyard, worldLayer, random, $"Grave_{j + 1:00}");
                occupied.Add(new PlacementBlock(gravePosition, obstacleSpacing));
                GenerateCoinsAroundGrave(grave.transform, graveyard, random, pickupLayer);
            }

            occupied.Add(new PlacementBlock(center, graveyardRadius + obstacleSpacing));
        }
    }

    private void GenerateCoinsAroundGrave(Transform grave, Transform parent, Random random, int pickupLayer)
    {
        if (currencyOrbPrefab == null || coinsPerGrave <= 0)
        {
            return;
        }

        float baseAngle = RandomAngle(random);
        for (int i = 0; i < coinsPerGrave; i++)
        {
            float angle = baseAngle + (i * Mathf.PI * 2f / coinsPerGrave);
            Vector3 position = grave.position + DirectionFromAngle(angle) * graveCoinRadius;
            CurrencyOrbPickup coin = Instantiate(currencyOrbPrefab, position, Quaternion.identity, parent);
            coin.gameObject.name = $"{grave.name}_Coin_{i + 1:00}";
            coin.SetValue(graveCoinValue);
            ConfigurePickup(coin.gameObject, pickupLayer);
        }
    }

    private void GenerateRockClusters(Random random, Transform root, List<PlacementBlock> occupied, int worldLayer)
    {
        Transform group = CreateContainer("RockClusters", root);
        if ((!forceRockClustersForTesting && !Roll(random, rockClusterChance)) || maxRockClusters <= 0)
        {
            return;
        }

        int clusterCount = random.Next(1, maxRockClusters + 1);
        int minRocks = Mathf.Min(minRocksPerCluster, maxRocksPerCluster);
        int maxRocks = Mathf.Max(minRocksPerCluster, maxRocksPerCluster);

        for (int i = 0; i < clusterCount; i++)
        {
            if (!TryGetOpenPosition(random, rockClusterRadius + obstacleSpacing, occupied, out Vector3 center))
            {
                continue;
            }

            Transform cluster = CreateContainer($"RockCluster_{i + 1:00}", group);
            cluster.position = center;
            int rockCount = random.Next(minRocks, maxRocks + 1);

            for (int j = 0; j < rockCount; j++)
            {
                GameObject prefab = GetRandomPrefab(rockPrefabs, random);
                if (prefab == null)
                {
                    continue;
                }

                Vector3 position = center + RandomInsideCircle(random, rockClusterRadius);
                if (!IsInsideBounds(position, obstacleSpacing))
                {
                    continue;
                }

                SpawnBlockingProp(prefab, position, cluster, worldLayer, random, $"Rock_{j + 1:00}");
            }

            TrySpawnClusterChest(random, cluster, worldLayer, center, occupied);
            occupied.Add(new PlacementBlock(center, rockClusterRadius + chestDistanceFromCluster));
        }
    }

    private void TrySpawnClusterChest(Random random, Transform cluster, int worldLayer, Vector3 clusterCenter, List<PlacementBlock> occupied)
    {
        if (chestPrefab == null)
        {
            return;
        }

        float baseAngle = RandomAngle(random);
        for (int i = 0; i < 12; i++)
        {
            Vector3 direction = DirectionFromAngle(baseAngle + (i * Mathf.PI / 6f));
            Vector3 position = clusterCenter + direction * chestDistanceFromCluster;
            if (!IsOpen(position, obstacleSpacing, occupied))
            {
                continue;
            }

            Quaternion rotation = Quaternion.LookRotation(-direction, Vector3.up);
            GameObject chest = SpawnBlockingProp(
                chestPrefab,
                position,
                cluster.parent,
                worldLayer,
                random,
                $"Chest_{cluster.name}",
                chestScaleMultiplier,
                chestGroundOffset,
                rotation);

            ConfigureChestVisuals(chest);
            occupied.Add(new PlacementBlock(position, obstacleSpacing));
            return;
        }

        Debug.LogWarning($"Arena layout could not place a chest near {cluster.name}. Try reducing cluster count or spacing.", this);
    }

    private void GenerateLooseDressing(Random random, Transform root, List<PlacementBlock> occupied, int worldLayer)
    {
        Transform rocks = CreateContainer("LooseRocks", root);
        for (int i = 0; i < individualRockCount; i++)
        {
            GameObject prefab = GetRandomPrefab(rockPrefabs, random);
            if (prefab == null || !TryGetOpenPosition(random, obstacleSpacing, occupied, out Vector3 position))
            {
                continue;
            }

            SpawnBlockingProp(prefab, position, rocks, worldLayer, random, $"LooseRock_{i + 1:00}");
            occupied.Add(new PlacementBlock(position, obstacleSpacing));
        }

        Transform cactus = CreateContainer("Cactus", root);
        for (int i = 0; i < cactusCount; i++)
        {
            GameObject prefab = GetRandomPrefab(cactusPrefabs, random);
            if (prefab == null || !TryGetOpenPosition(random, obstacleSpacing, occupied, out Vector3 position))
            {
                continue;
            }

            SpawnBlockingProp(prefab, position, cactus, worldLayer, random, $"Cactus_{i + 1:00}");
            occupied.Add(new PlacementBlock(position, obstacleSpacing));
        }
    }

    private GameObject SpawnBlockingProp(
        GameObject prefab,
        Vector3 position,
        Transform parent,
        int worldLayer,
        Random random,
        string objectName,
        float scaleMultiplier = 1f,
        float verticalOffset = 0f,
        Quaternion? forcedRotation = null)
    {
        Quaternion rotation = forcedRotation ?? Quaternion.Euler(0f, RandomRange(random, 0f, 360f), 0f);
        Vector3 spawnPosition = new Vector3(position.x, position.y + verticalOffset, position.z);
        GameObject instance = Instantiate(prefab, spawnPosition, rotation, parent);
        instance.name = objectName;
        float scale = RandomRange(random, minPropScale, maxPropScale) * Mathf.Max(0.01f, scaleMultiplier);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, Vector3.one * scale);
        SetLayerRecursively(instance, worldLayer);
        ConfigureBlockingColliders(instance);
        return instance;
    }

    private void ConfigureChestVisuals(GameObject chest)
    {
        SetRenderersVisible(chest);

        if (disableChestLodGroups)
        {
            LODGroup[] lodGroups = chest.GetComponentsInChildren<LODGroup>(true);
            for (int i = 0; i < lodGroups.Length; i++)
            {
                ForceFirstLodVisible(lodGroups[i]);
            }
        }

        if (chestGlowIntensity <= 0f || chestGlowRange <= 0f)
        {
            return;
        }

        GameObject glow = new GameObject("Chest_ReadabilityLight");
        glow.transform.SetParent(chest.transform);
        glow.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        glow.transform.localRotation = Quaternion.identity;
        glow.transform.localScale = Vector3.one;

        Light light = glow.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.72f, 0.32f);
        light.intensity = chestGlowIntensity;
        light.range = chestGlowRange;
        light.shadows = LightShadows.None;
    }

    private void SetRenderersVisible(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].gameObject.SetActive(true);
            renderers[i].enabled = true;
            renderers[i].forceRenderingOff = false;
        }
    }

    private void ForceFirstLodVisible(LODGroup lodGroup)
    {
        LOD[] lods = lodGroup.GetLODs();
        for (int i = 0; i < lods.Length; i++)
        {
            Renderer[] lodRenderers = lods[i].renderers;
            for (int j = 0; j < lodRenderers.Length; j++)
            {
                if (lodRenderers[j] == null)
                {
                    continue;
                }

                lodRenderers[j].gameObject.SetActive(i == 0);
                lodRenderers[j].enabled = i == 0;
                lodRenderers[j].forceRenderingOff = false;
            }
        }

        lodGroup.enabled = false;
    }

    private List<PlacementBlock> BuildInitialBlocks()
    {
        List<PlacementBlock> blocks = new List<PlacementBlock>();
        AddAnchorBlock(blocks, playerAnchor, playerClearRadius);
        AddAnchorBlock(blocks, bossSpawnAnchor, bossClearRadius);
        AddAnchorBlock(blocks, portalSpawnAnchor, portalClearRadius);

        if (enemySpawnAnchors == null)
        {
            return blocks;
        }

        for (int i = 0; i < enemySpawnAnchors.Length; i++)
        {
            AddAnchorBlock(blocks, enemySpawnAnchors[i], enemySpawnClearRadius);
        }

        AddEnemyLaneBlocks(blocks);
        return blocks;
    }

    private void AddAnchorBlock(List<PlacementBlock> blocks, Transform anchor, float radius)
    {
        if (anchor == null || radius <= 0f)
        {
            return;
        }

        blocks.Add(new PlacementBlock(anchor.position, radius));
    }

    private void AddEnemyLaneBlocks(List<PlacementBlock> blocks)
    {
        if (enemySpawnAnchors == null || enemyLaneClearRadius <= 0f || enemyLaneClearSteps <= 0)
        {
            return;
        }

        Vector3 target = playerAnchor != null ? playerAnchor.position : GetArenaCenter();
        for (int i = 0; i < enemySpawnAnchors.Length; i++)
        {
            Transform enemySpawnAnchor = enemySpawnAnchors[i];
            if (enemySpawnAnchor == null)
            {
                continue;
            }

            for (int step = 1; step <= enemyLaneClearSteps; step++)
            {
                float t = step / (enemyLaneClearSteps + 1f);
                Vector3 position = Vector3.Lerp(enemySpawnAnchor.position, target, t);
                blocks.Add(new PlacementBlock(position, enemyLaneClearRadius));
            }
        }
    }

    private bool TryGetOpenPosition(Random random, float clearanceRadius, List<PlacementBlock> occupied, out Vector3 position)
    {
        for (int i = 0; i < MaxPlacementAttempts; i++)
        {
            Vector2 minBounds = ActiveArenaMinBounds;
            Vector2 maxBounds = ActiveArenaMaxBounds;
            position = new Vector3(
                RandomRange(random, minBounds.x + clearanceRadius, maxBounds.x - clearanceRadius),
                0f,
                RandomRange(random, minBounds.y + clearanceRadius, maxBounds.y - clearanceRadius));

            if (IsOpen(position, clearanceRadius, occupied))
            {
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }

    private bool IsOpen(Vector3 position, float clearanceRadius, List<PlacementBlock> occupied)
    {
        if (!IsInsideBounds(position, clearanceRadius))
        {
            return false;
        }

        for (int i = 0; i < occupied.Count; i++)
        {
            PlacementBlock block = occupied[i];
            Vector3 offset = position - block.Position;
            offset.y = 0f;
            float minimumDistance = clearanceRadius + block.Radius;

            if (offset.sqrMagnitude < minimumDistance * minimumDistance)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInsideBounds(Vector3 position, float margin)
    {
        Vector2 minBounds = ActiveArenaMinBounds;
        Vector2 maxBounds = ActiveArenaMaxBounds;
        return position.x >= minBounds.x + margin
            && position.x <= maxBounds.x - margin
            && position.z >= minBounds.y + margin
            && position.z <= maxBounds.y - margin;
    }

    private Vector3 GetArenaCenter()
    {
        Vector2 minBounds = ActiveArenaMinBounds;
        Vector2 maxBounds = ActiveArenaMaxBounds;
        return new Vector3(
            (minBounds.x + maxBounds.x) * 0.5f,
            0f,
            (minBounds.y + maxBounds.y) * 0.5f);
    }

    private Vector3 GetPerimeterPosition(float angle, float inset)
    {
        Vector3 center = GetArenaCenter();
        Vector3 direction = DirectionFromAngle(angle);
        Vector2 minBounds = ActiveArenaMinBounds;
        Vector2 maxBounds = ActiveArenaMaxBounds;
        float halfWidth = Mathf.Max(0.1f, ((maxBounds.x - minBounds.x) * 0.5f) - inset);
        float halfDepth = Mathf.Max(0.1f, ((maxBounds.y - minBounds.y) * 0.5f) - inset);
        float distanceToX = Mathf.Abs(direction.x) > Mathf.Epsilon ? halfWidth / Mathf.Abs(direction.x) : float.PositiveInfinity;
        float distanceToZ = Mathf.Abs(direction.z) > Mathf.Epsilon ? halfDepth / Mathf.Abs(direction.z) : float.PositiveInfinity;
        float distance = Mathf.Min(distanceToX, distanceToZ);

        return center + direction * distance;
    }

    private void MoveAnchor(Transform anchor, Vector3 position)
    {
        if (anchor == null)
        {
            return;
        }

        anchor.position = new Vector3(position.x, anchor.position.y, position.z);
    }

    private static int CountValidTransforms(Transform[] transforms)
    {
        int count = 0;
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 offset = a - b;
        offset.y = 0f;
        return offset.magnitude;
    }

    private Transform CreateContainer(string containerName, Transform parent)
    {
        GameObject container = new GameObject(containerName);
        container.transform.SetParent(parent);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;
        container.transform.localScale = Vector3.one;
        return container.transform;
    }

    private void SetHideFlagsRecursively(GameObject root, HideFlags hideFlags)
    {
        root.hideFlags = hideFlags;
        Transform rootTransform = root.transform;
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            SetHideFlagsRecursively(rootTransform.GetChild(i).gameObject, hideFlags);
        }
    }

    private void ConfigureBlockingColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
        {
            AddRendererBoundsCollider(root);
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
            colliders[i].isTrigger = false;
        }
    }

    private void ConfigurePickup(GameObject root, int pickupLayer)
    {
        SetLayerRecursively(root, pickupLayer);
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
            colliders[i].isTrigger = true;
        }
    }

    private void AddRendererBoundsCollider(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 scale = Abs(root.transform.lossyScale);
        BoxCollider boxCollider = root.AddComponent<BoxCollider>();
        boxCollider.center = root.transform.InverseTransformPoint(bounds.center);
        boxCollider.size = new Vector3(
            SafeDivide(bounds.size.x, scale.x),
            SafeDivide(bounds.size.y, scale.y),
            SafeDivide(bounds.size.z, scale.z));
        boxCollider.isTrigger = false;
    }

    private void SetLayerRecursively(GameObject root, int layer)
    {
        if (layer < 0)
        {
            return;
        }

        root.layer = layer;
        Transform rootTransform = root.transform;
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            SetLayerRecursively(rootTransform.GetChild(i).gameObject, layer);
        }
    }

    private GameObject GetRandomPrefab(GameObject[] prefabs, Random random)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            return null;
        }

        int validCount = 0;
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return null;
        }

        int selected = random.Next(0, validCount);
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] == null)
            {
                continue;
            }

            if (selected == 0)
            {
                return prefabs[i];
            }

            selected--;
        }

        return null;
    }

    private bool Roll(Random random, float chance)
    {
        return random.NextDouble() <= Mathf.Clamp01(chance);
    }

    private Vector3 RandomInsideCircle(Random random, float radius)
    {
        float angle = RandomAngle(random);
        float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
        return DirectionFromAngle(angle) * distance;
    }

    private static Vector3 DirectionFromAngle(float angle)
    {
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private static float RandomAngle(Random random)
    {
        return RandomRange(random, 0f, Mathf.PI * 2f);
    }

    private static float RandomRange(Random random, float min, float max)
    {
        if (min > max)
        {
            return (min + max) * 0.5f;
        }

        return min + ((float)random.NextDouble() * (max - min));
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) <= Mathf.Epsilon ? value : value / divisor;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }

    private void ClearEditorSelectionIfInside(Transform root)
    {
#if UNITY_EDITOR
        UnityEngine.Object[] selectedObjects = Selection.objects;
        bool shouldClearSelection = false;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] == null)
            {
                shouldClearSelection = true;
                break;
            }

            GameObject selectedGameObject = GetSelectedGameObject(selectedObjects[i]);
            if (selectedGameObject != null && selectedGameObject.transform.IsChildOf(root))
            {
                shouldClearSelection = true;
                break;
            }
        }

        if (shouldClearSelection)
        {
            Selection.objects = new UnityEngine.Object[] { gameObject };
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
#endif
    }

    private void DestroyEditorRootSafely(GameObject root)
    {
#if UNITY_EDITOR
        ClearEditorSelectionIfInside(root.transform);
        root.SetActive(false);
        root.transform.SetParent(null);
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        EditorApplication.delayCall += () =>
        {
            if (root == null)
            {
                return;
            }

            ClearEditorSelectionIfInside(root.transform);
            ActiveEditorTracker.sharedTracker.ForceRebuild();

            EditorApplication.delayCall += () =>
            {
                if (root != null)
                {
                    DestroyImmediate(root);
                    ActiveEditorTracker.sharedTracker.ForceRebuild();
                }
            };
        };
#endif
    }

#if UNITY_EDITOR
    private static GameObject GetSelectedGameObject(UnityEngine.Object selectedObject)
    {
        if (selectedObject is GameObject selectedGameObject)
        {
            return selectedGameObject;
        }

        if (selectedObject is Component selectedComponent)
        {
            return selectedComponent.gameObject;
        }

        return null;
    }
#endif

    private void MarkSceneDirty(bool allowEditorPreviewDirty)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && allowEditorPreviewDirty)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
