using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Bullet prefab spawned by the player's weapon and stored in the bullet pool.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Transform used as the bullet spawn position and forward firing direction.")]
    [SerializeField] private Transform firePoint;

    [Space]
    [Header("Audio")]
    [Tooltip("Audio clip played once for each fired bullet.")]
    [SerializeField] private AudioClip shootSound;

    [Tooltip("Shot sound playback volume from 0 to 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private float nextFireTime;
    private float lastKnownFireRate;
    private float bulletLifetime;
    private int totalBulletsCreated;

    private PlayerStats stats;
    private AudioSource audioSource;
    private readonly Queue<Bullet> bulletPool = new Queue<Bullet>();

    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        Bullet prefabBullet = bulletPrefab.GetComponent<Bullet>();
        bulletLifetime = prefabBullet != null ? prefabBullet.LifeTime : 3f;

        lastKnownFireRate = stats.FireRate;
        EnsurePoolCapacity();
    }

    private void Update()
    {
        if (stats.FireRate != lastKnownFireRate)
        {
            lastKnownFireRate = stats.FireRate;
            EnsurePoolCapacity();
        }

        Shoot();
    }

    private void Shoot()
    {
        if (!Input.GetMouseButton(0) || Time.time < nextFireTime) return;

        Bullet bullet = GetBulletFromPool();
        bullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        bullet.Init(stats.BulletDamage, ReturnBulletToPool);
        bullet.gameObject.SetActive(true);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound, volume);

        nextFireTime = Time.time + 1f / stats.FireRate;
    }

    private void EnsurePoolCapacity()
    {
        // +2 safety margin so there is always a ready bullet even mid-burst
        int needed = Mathf.CeilToInt(stats.FireRate * bulletLifetime) + 2;
        int toCreate = needed - totalBulletsCreated;
        for (int i = 0; i < toCreate; i++)
            CreatePooledBullet();
    }

    private Bullet GetBulletFromPool()
    {
        if (bulletPool.Count == 0)
            CreatePooledBullet();

        return bulletPool.Dequeue();
    }

    private void ReturnBulletToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    private void CreatePooledBullet()
    {
        GameObject obj = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform);
        Bullet bullet = obj.GetComponent<Bullet>();
        obj.SetActive(false);
        bulletPool.Enqueue(bullet);
        totalBulletsCreated++;
    }
}
