using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerDashVFX : MonoBehaviour
{
    [Header("Dash Effects")]
    [Tooltip("Assign components that implement IDashEffect, like EnergyTrailDashEffect or DashAfterimageEffect.")]
    [SerializeField] private MonoBehaviour[] dashEffectBehaviours;

    private PlayerMovement movement;
    private readonly List<IDashEffect> dashEffects = new List<IDashEffect>();

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        ResolveDashEffects();
    }

    private void OnEnable()
    {
        foreach (IDashEffect effect in dashEffects)
        {
            movement.DashStarted += effect.OnDashStarted;
            movement.DashEnded += effect.OnDashEnded;
        }
    }

    private void OnDisable()
    {
        foreach (IDashEffect effect in dashEffects)
        {
            movement.DashStarted -= effect.OnDashStarted;
            movement.DashEnded -= effect.OnDashEnded;
        }
    }

    private void OnDestroy()
    {
        foreach (IDashEffect effect in dashEffects)
        {
            effect.Cleanup();
        }
    }

    private void ResolveDashEffects()
    {
        dashEffects.Clear();

        foreach (MonoBehaviour behaviour in dashEffectBehaviours)
        {
            if (behaviour is IDashEffect effect)
            {
                effect.Initialize(transform);
                dashEffects.Add(effect);
            }
            else if (behaviour != null)
            {
                Debug.LogWarning($"{behaviour.name} does not implement IDashEffect.", behaviour);
            }
        }

        if (dashEffects.Count == 0)
        {
            Debug.LogWarning($"No IDashEffect assigned on {name}.", this);
        }
    }
}