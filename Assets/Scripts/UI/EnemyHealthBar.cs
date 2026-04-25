using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image fillImage;
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 1.6f, 0f);

    private EnemyBoxAgent enemy;
    private Camera mainCamera;

    private void Awake()
    {
        enemy = GetComponent<EnemyBoxAgent>();
        mainCamera = Camera.main;
        worldCanvas.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        enemy.Spawned += OnSpawned;
        enemy.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        enemy.Spawned -= OnSpawned;
        enemy.Damaged -= OnDamaged;
    }

    private void LateUpdate()
    {
        if (!worldCanvas.gameObject.activeSelf || mainCamera == null) return;

        worldCanvas.transform.position = transform.position + barOffset;
        worldCanvas.transform.rotation = mainCamera.transform.rotation;
    }

    private void OnSpawned(float current, float max)
    {
        fillImage.fillAmount = 1f;
        worldCanvas.gameObject.SetActive(true);
    }

    private void OnDamaged(float amount, float current, float max)
    {
        fillImage.fillAmount = max > 0f ? current / max : 0f;

        if (damagePopupPrefab != null)
        {
            Vector3 spawnPos = transform.position + barOffset + Vector3.up * 0.3f;
            DamagePopup popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            popup.Show(amount);
        }
    }
}
