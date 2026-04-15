using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float volume = 0.5f;
    
    private float nextFireTime = 0f;
    private PlayerStats stats;
    private AudioSource audioSource;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();

        // Ensure an AudioSource exists on the Player
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            // 1. Spawn the bullet
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 2. Set the damage
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = stats.bulletDamage;
            }

            // 3. Play sound
            if (shootSound != null)
            {
                audioSource.PlayOneShot(shootSound, volume);
            }

            // 4. Reset the cooldown timer
            nextFireTime = Time.time + 1f / stats.fireRate;
        }
    }
}
