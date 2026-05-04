using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeRarity
{
    Common,
    Rare,
    Legendary
}

public enum UpgradeStatType
{
    MoveSpeed,
    StaminaRegainPerSecond,
    DashDistance,
    DashStaminaCost,
    BulletDamage,
    FireRate,
    HealthRestore,
    HealthCapacity
}

public enum UpgradeValueKind
{
    PercentageIncrease,
    PercentageReduction,
    FlatHealthRestore,
    FlatMaxHealthIncrease
}

[Serializable]
public class UpgradeStatDefinition
{
    [Header("Identity")]
    [Tooltip("Player stat category this generated upgrade modifies.")]
    [SerializeField] private UpgradeStatType statType;

    [Space]
    [Header("Rarity Values")]
    [Tooltip("Amount applied by the Common version of this upgrade. Percentage-based upgrades use whole-number percents.")]
    [Min(0f)]
    [SerializeField] private float commonValue;

    [Tooltip("Amount applied by the Rare version of this upgrade. Percentage-based upgrades use whole-number percents.")]
    [Min(0f)]
    [SerializeField] private float rareValue;

    [Tooltip("Amount applied by the Legendary version of this upgrade. Percentage-based upgrades use whole-number percents.")]
    [Min(0f)]
    [SerializeField] private float legendaryValue;

    public UpgradeStatType StatType => statType;

    public UpgradeValueKind ValueKind => statType switch
    {
        UpgradeStatType.DashStaminaCost => UpgradeValueKind.PercentageReduction,
        UpgradeStatType.HealthRestore => UpgradeValueKind.FlatHealthRestore,
        UpgradeStatType.HealthCapacity => UpgradeValueKind.FlatMaxHealthIncrease,
        _ => UpgradeValueKind.PercentageIncrease
    };

    public string DisplayName => statType switch
    {
        UpgradeStatType.MoveSpeed => "Move Speed",
        UpgradeStatType.StaminaRegainPerSecond => "Stamina Regain Per Second",
        UpgradeStatType.DashDistance => "Dash Distance",
        UpgradeStatType.DashStaminaCost => "Dash Stamina Cost",
        UpgradeStatType.BulletDamage => "Bullet Damage",
        UpgradeStatType.FireRate => "Fire Rate",
        UpgradeStatType.HealthRestore => "Health Restore",
        UpgradeStatType.HealthCapacity => "Health Capacity",
        _ => statType.ToString()
    };

    public float GetValue(UpgradeRarity rarity)
    {
        return rarity switch
        {
            UpgradeRarity.Common => commonValue,
            UpgradeRarity.Rare => rareValue,
            UpgradeRarity.Legendary => legendaryValue,
            _ => commonValue
        };
    }

    public string CreateDescription(UpgradeRarity rarity)
    {
        float value = GetValue(rarity);
        string formattedValue = FormatValue(value);

        return statType switch
        {
            UpgradeStatType.MoveSpeed => $"Increases move speed by {formattedValue}%",
            UpgradeStatType.StaminaRegainPerSecond => $"Increases stamina regain per second by {formattedValue}%",
            UpgradeStatType.DashDistance => $"Increases dash distance by {formattedValue}%",
            UpgradeStatType.DashStaminaCost => $"Reduces dash stamina cost by {formattedValue}%",
            UpgradeStatType.BulletDamage => $"Increases bullet damage by {formattedValue}%",
            UpgradeStatType.FireRate => $"Increases fire rate by {formattedValue}%",
            UpgradeStatType.HealthRestore => $"Restores {formattedValue} HP",
            UpgradeStatType.HealthCapacity => $"Increases max health by {formattedValue} HP",
            _ => string.Empty
        };
    }

    public void Apply(GameObject player, UpgradeRarity rarity)
    {
        if (player == null)
        {
            return;
        }

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        float percentValue = GetValue(rarity) / 100f;

        switch (statType)
        {
            case UpgradeStatType.MoveSpeed:
                stats?.IncreaseMoveSpeedByPercent(percentValue);
                break;
            case UpgradeStatType.StaminaRegainPerSecond:
                stats?.IncreaseStaminaRegenByPercent(percentValue);
                break;
            case UpgradeStatType.DashDistance:
                stats?.IncreaseDashDistanceByPercent(percentValue);
                break;
            case UpgradeStatType.DashStaminaCost:
                stats?.ReduceDashStaminaCostByPercent(percentValue);
                break;
            case UpgradeStatType.BulletDamage:
                stats?.IncreaseBulletDamageByPercent(percentValue);
                break;
            case UpgradeStatType.FireRate:
                stats?.IncreaseFireRateByPercent(percentValue);
                break;
            case UpgradeStatType.HealthRestore:
                health?.Heal(GetValue(rarity));
                break;
            case UpgradeStatType.HealthCapacity:
                health?.IncreaseMaxHealth(GetValue(rarity));
                break;
        }
    }

    private static string FormatValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value)) ? value.ToString("0") : value.ToString("0.##");
    }
}

public sealed class UpgradeOffer
{
    private readonly UpgradeStatDefinition definition;

    public UpgradeOffer(UpgradeStatDefinition definition, UpgradeRarity rarity)
    {
        this.definition = definition;
        Rarity = rarity;
    }

    public UpgradeStatType StatType => definition.StatType;
    public UpgradeRarity Rarity { get; }
    public string DisplayName => definition.DisplayName;
    public string Description => definition.CreateDescription(Rarity);
    public float Value => definition.GetValue(Rarity);
    public UpgradeValueKind ValueKind => definition.ValueKind;

    public void Apply(GameObject player)
    {
        definition.Apply(player, Rarity);
    }
}

[CreateAssetMenu(menuName = "Upgrades/Upgrade Library")]
public class UpgradeLibrary : ScriptableObject
{
    [Header("Upgrade Pool")]
    [Tooltip("List of player stat definitions that can appear in random upgrade offers.")]
    [SerializeField] private List<UpgradeStatDefinition> availableStats = new List<UpgradeStatDefinition>();

    [Space]
    [Header("Rarity Weights")]
    [Tooltip("Relative roll weight for Common upgrade offers.")]
    [Range(0f, 1f)]
    [SerializeField] private float commonChance = 0.6f;

    [Tooltip("Relative roll weight for Rare upgrade offers.")]
    [Range(0f, 1f)]
    [SerializeField] private float rareChance = 0.3f;

    [Tooltip("Relative roll weight for Legendary upgrade offers.")]
    [Range(0f, 1f)]
    [SerializeField] private float legendaryChance = 0.1f;

    public IReadOnlyList<UpgradeOffer> CreateRandomOffers(int count)
    {
        if (availableStats == null || availableStats.Count == 0 || count <= 0)
        {
            return Array.Empty<UpgradeOffer>();
        }

        List<UpgradeStatDefinition> pool = new List<UpgradeStatDefinition>(availableStats);
        List<UpgradeOffer> offers = new List<UpgradeOffer>(Mathf.Min(count, pool.Count));
        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            UpgradeStatDefinition selectedDefinition = pool[index];
            pool.RemoveAt(index);
            offers.Add(new UpgradeOffer(selectedDefinition, RollRarity()));
        }

        return offers;
    }

    private UpgradeRarity RollRarity()
    {
        float totalChance = commonChance + rareChance + legendaryChance;

        if (totalChance <= Mathf.Epsilon)
        {
            return UpgradeRarity.Common;
        }

        float roll = UnityEngine.Random.value * totalChance;

        if (roll < commonChance)
        {
            return UpgradeRarity.Common;
        }

        roll -= commonChance;

        if (roll < rareChance)
        {
            return UpgradeRarity.Rare;
        }

        return UpgradeRarity.Legendary;
    }
}
