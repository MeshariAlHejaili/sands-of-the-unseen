using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1100)]
public class ArenaMapBounds : MonoBehaviour
{
    [Header("Map Size")]
    [Tooltip("Playable arena width and depth in world units. Snaps to a multiple of the tile size.")]
    [Range(25, 100)]
    [SerializeField] private int mapSize = 50;

    [Tooltip("Ground tile width and depth in world units. Ground_01 is 5 by 5 world units.")]
    [Range(1, 20)]
    [SerializeField] private int tileSize = 5;

    [Tooltip("Default horizontal size in world units of the Ground mesh before transform scaling. Unity's built-in Plane is 10 units wide.")]
    [Min(0.1f)]
    [SerializeField] private float unityPlaneMeshSize = 10f;

    [Space]
    [Header("Visual Tiles")]
    [Tooltip("Low-poly desert ground tile prefab used only for visual surface dressing.")]
    [AssetsOnly]
    [SerializeField] private GameObject groundTilePrefab;

    [Tooltip("Scene root name used for generated visual ground tiles.")]
    [SerializeField] private string visualRootName = "_ArenaGroundTiles";

    [Tooltip("Vertical offset in world units applied to visual tiles to prevent z-fighting with the gameplay ground plane.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float tileVerticalOffset = 0.01f;

    [Tooltip("Whether generated visual tile colliders are disabled so the single Ground collider remains authoritative.")]
    [SerializeField] private bool disableTileColliders = true;

    [Tooltip("Whether generated visual tile renderers should stop casting shadows.")]
    [SerializeField] private bool disableTileShadowCasting = true;

    [Tooltip("Whether generated visual tile renderers should receive shadows.")]
    [SerializeField] private bool receiveTileShadows = true;

    [Space]
    [Header("Gameplay Ground")]
    [Tooltip("Whether the original flat Ground renderer is hidden when visual tiles exist.")]
    [SerializeField] private bool hideGameplayRendererWhenTilesExist = true;

    public int MapSize => mapSize;

    public int TileSize => tileSize;

    public Vector2 MinBounds
    {
        get
        {
            Vector2 center = Center;
            float halfSize = mapSize * 0.5f;
            return new Vector2(center.x - halfSize, center.y - halfSize);
        }
    }

    public Vector2 MaxBounds
    {
        get
        {
            Vector2 center = Center;
            float halfSize = mapSize * 0.5f;
            return new Vector2(center.x + halfSize, center.y + halfSize);
        }
    }

    public Vector2 Center => new Vector2(transform.position.x, transform.position.z);

    private void Awake()
    {
        ApplyGameplayGroundScale();
        UpdateGameplayRendererVisibility();
    }

    private void OnValidate()
    {
        ClampSettings();
    }

    [Button("Apply Ground Bounds")]
    [ContextMenu("Apply Ground Bounds")]
    private void ApplyGroundBounds()
    {
        ClampSettings();
        ApplyGameplayGroundScale();
        UpdateGameplayRendererVisibility();
        MarkSceneDirty();
    }

    [Button("Use 50 x 50 Map")]
    private void UseFiftyByFiftyMap()
    {
        SetMapSizeAndRebuild(50);
    }

    [Button("Use 75 x 75 Map")]
    private void UseSeventyFiveBySeventyFiveMap()
    {
        SetMapSizeAndRebuild(75);
    }

    [Button("Use 100 x 100 Map")]
    private void UseOneHundredByOneHundredMap()
    {
        SetMapSizeAndRebuild(100);
    }

    [Button("Rebuild Visual Ground Tiles")]
    [ContextMenu("Rebuild Visual Ground Tiles")]
    private void RebuildVisualGroundTiles()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogWarning("Arena ground tiles should be rebuilt in edit mode, not during Play mode.", this);
            return;
        }

        if (groundTilePrefab == null)
        {
            Debug.LogWarning("Arena ground tiles could not be rebuilt because no ground tile prefab is assigned.", this);
            return;
        }

        ClampSettings();
        ApplyGameplayGroundScale();
        ClearVisualGroundTilesImmediate();

        GameObject root = new GameObject(visualRootName);
        Undo.RegisterCreatedObjectUndo(root, "Create arena ground tile root");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        int tileCountPerAxis = Mathf.RoundToInt(mapSize / tileSize);
        float start = (-mapSize * 0.5f) + (tileSize * 0.5f);

        for (int x = 0; x < tileCountPerAxis; x++)
        {
            for (int z = 0; z < tileCountPerAxis; z++)
            {
                GameObject tile = PrefabUtility.InstantiatePrefab(groundTilePrefab, root.transform) as GameObject;
                if (tile == null)
                {
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(tile, "Create arena ground tile");
                tile.name = $"GroundTile_{x:00}_{z:00}";
                tile.transform.position = new Vector3(
                    transform.position.x + start + (x * tileSize),
                    transform.position.y + tileVerticalOffset,
                    transform.position.z + start + (z * tileSize));
                tile.transform.rotation = Quaternion.identity;
                tile.transform.localScale = Vector3.one;
                ConfigureVisualTile(tile);
            }
        }

        UpdateGameplayRendererVisibility();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    [Button("Clear Visual Ground Tiles")]
    [ContextMenu("Clear Visual Ground Tiles")]
    private void ClearVisualGroundTiles()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            return;
        }

        ClearVisualGroundTilesImmediate();
        UpdateGameplayRendererVisibility();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private void ClampSettings()
    {
        tileSize = Mathf.Max(1, tileSize);
        mapSize = Mathf.Clamp(mapSize, tileSize, 100);
        mapSize = Mathf.RoundToInt(mapSize / (float)tileSize) * tileSize;
    }

    private void SetMapSizeAndRebuild(int size)
    {
        mapSize = size;
        RebuildVisualGroundTiles();
    }

    private void ApplyGameplayGroundScale()
    {
        float horizontalScale = mapSize / unityPlaneMeshSize;
        Vector3 localScale = transform.localScale;
        transform.localScale = new Vector3(horizontalScale, localScale.y, horizontalScale);
    }

    private void UpdateGameplayRendererVisibility()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            return;
        }

        bool visualTilesExist = HasVisualGroundTiles();
        meshRenderer.enabled = !hideGameplayRendererWhenTilesExist || !visualTilesExist;
    }

    private bool HasVisualGroundTiles()
    {
        GameObject root = GameObject.Find(visualRootName);
        return root != null && root.transform.childCount > 0;
    }

#if UNITY_EDITOR
    private void ClearVisualGroundTilesImmediate()
    {
        GameObject root = GameObject.Find(visualRootName);
        if (root == null)
        {
            return;
        }

        ClearSelectionIfInside(root.transform);
        Undo.DestroyObjectImmediate(root);
    }

    private void ConfigureVisualTile(GameObject tile)
    {
        SetStaticRecursively(tile);
        ConfigureRenderers(tile);

        if (!disableTileColliders)
        {
            return;
        }

        Collider[] colliders = tile.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void ConfigureRenderers(GameObject tile)
    {
        Renderer[] renderers = tile.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (disableTileShadowCasting)
            {
                renderers[i].shadowCastingMode = ShadowCastingMode.Off;
            }

            renderers[i].receiveShadows = receiveTileShadows;
        }
    }

    private void SetStaticRecursively(GameObject root)
    {
        root.isStatic = true;
        Transform rootTransform = root.transform;
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            SetStaticRecursively(rootTransform.GetChild(i).gameObject);
        }
    }

    private void ClearSelectionIfInside(Transform root)
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || !selected.transform.IsChildOf(root))
        {
            return;
        }

        Selection.activeGameObject = gameObject;
    }

    private void MarkSceneDirty()
    {
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
#else
    private void MarkSceneDirty()
    {
    }
#endif
}
