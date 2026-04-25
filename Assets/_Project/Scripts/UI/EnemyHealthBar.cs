using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image fillImage;
    [SerializeField] private DamagePopup damagePopupPrefab;
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private int initialDamagePopupPoolSize = 4;

    private EnemyHealth enemyHealth;
    private Camera mainCamera;
    private readonly Queue<DamagePopup> pooledDamagePopups = new Queue<DamagePopup>();
    private readonly HashSet<DamagePopup> activeDamagePopups = new HashSet<DamagePopup>();

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
        PrewarmDamagePopupPool();
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
            DamagePopup popup = GetDamagePopupFromPool();
            if (popup == null) return;

            activeDamagePopups.Add(popup);
            popup.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
            popup.gameObject.SetActive(true);
            popup.Show(amount, ReleaseDamagePopup);
        }
    }

    private void PrewarmDamagePopupPool()
    {
        if (damagePopupPrefab == null) return;

        EnsureDamagePopupPoolSize(initialDamagePopupPoolSize);
    }

    private void EnsureDamagePopupPoolSize(int requiredAvailable)
    {
        int missingCount = requiredAvailable - pooledDamagePopups.Count;
        for (int i = 0; i < missingCount; i++)
        {
            DamagePopup popup = Instantiate(damagePopupPrefab);
            popup.gameObject.SetActive(false);
            pooledDamagePopups.Enqueue(popup);
        }
    }

    private DamagePopup GetDamagePopupFromPool()
    {
        if (pooledDamagePopups.Count == 0)
            EnsureDamagePopupPoolSize(1);

        return pooledDamagePopups.Count > 0 ? pooledDamagePopups.Dequeue() : null;
    }

    private void ReleaseDamagePopup(DamagePopup popup)
    {
        if (popup == null || !activeDamagePopups.Remove(popup))
            return;

        popup.ResetForPool();
        popup.gameObject.SetActive(false);
        pooledDamagePopups.Enqueue(popup);
    }
}
