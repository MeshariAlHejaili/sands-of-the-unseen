using System;
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
    private int lastKnownBulletsPerShot;
    private float bulletLifetime;
    private int totalBulletsCreated;

    private PlayerStats stats;
    private AudioSource audioSource;
    private readonly Queue<Bullet> bulletPool = new Queue<Bullet>();

    public event Action<Vector3, Quaternion> ShotFired;

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
        lastKnownBulletsPerShot = stats.BulletsPerShot;
        EnsurePoolCapacity();
    }

    private void Update()
    {
        if (stats.FireRate != lastKnownFireRate || stats.BulletsPerShot != lastKnownBulletsPerShot)
        {
            lastKnownFireRate = stats.FireRate;
            lastKnownBulletsPerShot = stats.BulletsPerShot;
            EnsurePoolCapacity();
        }

        Shoot();
    }

    private void Shoot()
    {
        if (!Input.GetMouseButton(0) || Time.time < nextFireTime) return;

        int bulletsToFire = Mathf.Max(1, stats.BulletsPerShot);

        for (int i = 0; i < bulletsToFire; i++)
        {
            SpawnBullet(GetShotRotation(i, bulletsToFire));
        }

        ShotFired?.Invoke(firePoint.position, firePoint.rotation);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound, volume);

        nextFireTime = Time.time + 1f / stats.FireRate;
    }

    private void SpawnBullet(Quaternion rotation)
    {
        Bullet bullet = GetBulletFromPool();
        bullet.transform.SetPositionAndRotation(firePoint.position, rotation);
        bullet.Init(stats.BulletDamage, ReturnBulletToPool);
        bullet.gameObject.SetActive(true);
    }

    private Quaternion GetShotRotation(int bulletIndex, int bulletsToFire)
    {
        if (bulletsToFire <= 1 || stats.BulletSpreadAngle <= 0f)
        {
            return firePoint.rotation;
        }

        float spreadStep = stats.BulletSpreadAngle / (bulletsToFire - 1);
        float yawOffset = -stats.BulletSpreadAngle * 0.5f + spreadStep * bulletIndex;
        return Quaternion.AngleAxis(yawOffset, Vector3.up) * firePoint.rotation;
    }

    private void EnsurePoolCapacity()
    {
        // +2 safety margin so there is always a ready bullet even mid-burst
        int needed = Mathf.CeilToInt(stats.FireRate * bulletLifetime * Mathf.Max(1, stats.BulletsPerShot)) + 2;
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
