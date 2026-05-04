using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerInputReader))]
[RequireComponent(typeof(PlayerAim))]
[DefaultExecutionOrder(100)]
public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("Bullet prefab spawned by the player's weapon and stored in the bullet pool.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Fallback transform used as the shot spawn position when no PlayerWeaponMount is present.")]
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
    private PlayerInputReader inputReader;
    private PlayerAim playerAim;
    private PlayerWeaponMount weaponMount;
    private AudioSource audioSource;
    private readonly Queue<Bullet> bulletPool = new Queue<Bullet>();

    public event Action<Vector3, Quaternion> ShotFired;

    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        inputReader = PlayerInputReader.GetOrAdd(gameObject);
        playerAim = GetComponent<PlayerAim>();
        weaponMount = GetComponent<PlayerWeaponMount>();
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
    }

    private void LateUpdate()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (!inputReader.IsAttackHeld || Time.time < nextFireTime) return;
        if (!TryGetShotPose(out Vector3 shotPosition, out Quaternion shotRotation)) return;

        int bulletsToFire = Mathf.Max(1, stats.BulletsPerShot);

        for (int i = 0; i < bulletsToFire; i++)
        {
            SpawnBullet(shotPosition, GetShotRotation(i, bulletsToFire, shotRotation));
        }

        ShotFired?.Invoke(shotPosition, shotRotation);

        if (shootSound != null)
            audioSource.PlayOneShot(shootSound, volume);

        nextFireTime = Time.time + 1f / stats.FireRate;
    }

    private bool TryGetShotPose(out Vector3 position, out Quaternion rotation)
    {
        if (weaponMount != null && weaponMount.GetShotPose(out position, out rotation))
        {
            return true;
        }

        position = firePoint != null ? firePoint.position : transform.position;

        if (playerAim != null)
        {
            if (!playerAim.TryGetAimRotationFrom(position, out rotation))
            {
                rotation = playerAim.AimRotation;
            }

            return true;
        }

        Vector3 forward = firePoint != null ? firePoint.forward : transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude <= Mathf.Epsilon)
        {
            rotation = Quaternion.identity;
            return false;
        }

        rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        return true;
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation)
    {
        Bullet bullet = GetBulletFromPool();
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.Init(stats.BulletDamage, ReturnBulletToPool);
        bullet.gameObject.SetActive(true);
    }

    private Quaternion GetShotRotation(int bulletIndex, int bulletsToFire, Quaternion baseRotation)
    {
        if (bulletsToFire <= 1 || stats.BulletSpreadAngle <= 0f)
        {
            return baseRotation;
        }

        float spreadStep = stats.BulletSpreadAngle / (bulletsToFire - 1);
        float yawOffset = -stats.BulletSpreadAngle * 0.5f + spreadStep * bulletIndex;
        return Quaternion.AngleAxis(yawOffset, Vector3.up) * baseRotation;
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
        GameObject obj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = obj.GetComponent<Bullet>();
        obj.SetActive(false);
        bulletPool.Enqueue(bullet);
        totalBulletsCreated++;
    }
}
