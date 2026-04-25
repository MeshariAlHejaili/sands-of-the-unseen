using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
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
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

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
