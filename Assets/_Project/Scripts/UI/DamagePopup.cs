using System;
using System.Collections;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [Tooltip("World text mesh used to display the damage number.")]
    [SerializeField] private TextMesh textMesh;

    [Space]
    [Header("Animation")]
    [Tooltip("Upward movement speed in world units per second.")]
    [Min(0f)]
    [SerializeField] private float floatSpeed = 1.5f;

    [Tooltip("Time in seconds before the popup fades out and returns to its pool.")]
    [Min(0.01f)]
    [SerializeField] private float lifetime = 0.8f;

    private Camera mainCamera;
    private Color initialColor;
    private Coroutine animationRoutine;
    private Action<DamagePopup> onReturn;
    private bool returned;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();

        if (textMesh != null)
            initialColor = textMesh.color;
    }

    public void Show(float damage)
    {
        Show(damage, null);
    }

    public void Show(float damage, Action<DamagePopup> returnCallback)
    {
        if (textMesh == null)
        {
            ReturnToPool();
            return;
        }

        textMesh.text = Mathf.RoundToInt(damage).ToString();
        textMesh.color = initialColor;
        onReturn = returnCallback;
        returned = false;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(Animate());
    }

    public void ResetForPool()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        if (textMesh != null)
            textMesh.color = initialColor;

        onReturn = null;
        returned = false;
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Color color = initialColor;

        while (elapsed < lifetime)
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            textMesh.color = new Color(color.r, color.g, color.b, 1f - (elapsed / lifetime));

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
                transform.rotation = mainCamera.transform.rotation;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (returned) return;

        returned = true;
        animationRoutine = null;

        if (onReturn != null)
            onReturn.Invoke(this);
        else
            Destroy(gameObject);
    }
}
