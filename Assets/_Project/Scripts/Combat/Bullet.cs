using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Forward travel speed in world units per second.")]
    [Min(0f)]
    [SerializeField] private float speed = 20f;

    [Tooltip("Time in seconds before this bullet returns to its pool.")]
    [Min(0.01f)]
    [SerializeField] private float lifeTime = 3f;

    private float damage;
    private float spawnTime;
    private Action<Bullet> onReturn;
    private bool returned;

    public float Damage => damage;
    public float LifeTime => lifeTime;

    public void Init(float bulletDamage, Action<Bullet> returnCallback)
    {
        damage = bulletDamage;
        onReturn = returnCallback;
        returned = false;
        spawnTime = Time.time;
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        if (Time.time >= spawnTime + lifeTime)
            ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (returned) return;
        returned = true;
        onReturn?.Invoke(this);
    }
}
