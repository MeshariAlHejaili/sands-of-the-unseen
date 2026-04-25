using System.Collections;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMesh textMesh;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.8f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();
    }

    public void Show(float damage)
    {
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Color color = textMesh.color;

        while (elapsed < lifetime)
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            textMesh.color = new Color(color.r, color.g, color.b, 1f - (elapsed / lifetime));

            if (mainCamera != null)
                transform.rotation = mainCamera.transform.rotation;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
