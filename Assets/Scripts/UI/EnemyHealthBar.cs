using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image fillImage;
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 1.6f, 0f);

    private EnemyHealth enemyHealth;
    private Camera mainCamera;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogWarning("EnemyHealthBar requires EnemyHealth on the same GameObject.", this);
            enabled = false;
            return;
        }
        mainCamera = Camera.main;
        worldCanvas.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (enemyHealth == null) return;
        enemyHealth.Damaged += OnDamaged;

        if (enemyHealth.MaxHealth > 0f)
        {
            fillImage.fillAmount = enemyHealth.CurrentHealth / enemyHealth.MaxHealth;
            worldCanvas.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (enemyHealth == null) return;
        enemyHealth.Damaged -= OnDamaged;
        worldCanvas.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!worldCanvas.gameObject.activeSelf || mainCamera == null) return;

        worldCanvas.transform.position = transform.position + barOffset;
        worldCanvas.transform.rotation = mainCamera.transform.rotation;
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
