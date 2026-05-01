using UnityEngine;

public interface IShootEffect
{
    void Initialize(Transform owner);
    void OnShotFired(Vector3 position, Quaternion rotation);
    void Cleanup();
}