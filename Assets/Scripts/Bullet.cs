using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    public float damage; // Set by the Player script when spawned

    void Start()
    {
        // Destroy the bullet after X seconds to clean up memory
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move the bullet forward based on its own forward axis
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}