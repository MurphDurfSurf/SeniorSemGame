using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StatusEffect
{
    public string name;
    public float duration;
    public float remainingTime;
    public bool isPermanent;

    // Effect values
    public float healthModifier;        // Per second
    public float speedModifier;         // Multiplier
    public float damageModifier;        // Multiplier for damage dealt
    public float defenseModifier;       // Multiplier for damage taken

    public StatusEffect(string name, float duration, float healthMod = 0f, float speedMod = 1f, float damageMod = 1f, float defenseMod = 1f)
    {
        this.name = name;
        this.duration = duration;
        this.remainingTime = duration;
        this.isPermanent = (duration <= 0);
        this.healthModifier = healthMod;
        this.speedModifier = speedMod;
        this.damageModifier = damageMod;
        this.defenseModifier = defenseMod;
    }
}

public class PlayerInfo : MonoBehaviour
{
    // Events for UI updates
    public delegate void HealthChangeHandler();
    public delegate void StatusEffectHandler(StatusEffect effect);
    public delegate void StatusEffectRemoveHandler(string effectName);
    
    public event HealthChangeHandler OnHealthChanged;
    public event StatusEffectHandler OnStatusEffectAdded;
    public event StatusEffectRemoveHandler OnStatusEffectRemoved;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvulnerable = false;

    [Header("Status Effects")]
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    
    // Cached modifiers
    private float cachedSpeedModifier = 1f;
    private float cachedDamageModifier = 1f;
    private float cachedDefenseModifier = 1f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        UpdateStatusEffects();
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        // Apply defense modifier
        float modifiedDamage = damage * cachedDefenseModifier;
        currentHealth = Mathf.Max(0f, currentHealth - modifiedDamage);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke();
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        // Check if effect of same type exists and remove it
        activeEffects.RemoveAll(e => e.name == effect.name);
        activeEffects.Add(effect);
        RecalculateModifiers();
    }

    public void RemoveStatusEffect(string effectName)
    {
        activeEffects.RemoveAll(e => e.name == effectName);
        RecalculateModifiers();
    }

    private void UpdateStatusEffects()
    {
        bool needsRecalculation = false;

        // Update all active effects
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            
            if (!effect.isPermanent)
            {
                // Update duration
                effect.remainingTime -= Time.deltaTime;
                
                // Remove expired effects
                if (effect.remainingTime <= 0)
                {
                    activeEffects.RemoveAt(i);
                    needsRecalculation = true;
                    continue;
                }
            }

            // Apply health modification over time
            if (effect.healthModifier != 0)
            {
                float healthChange = effect.healthModifier * Time.deltaTime;
                currentHealth = Mathf.Clamp(currentHealth + healthChange, 0f, maxHealth);
            }
        }

        if (needsRecalculation)
        {
            RecalculateModifiers();
        }
    }

    private void RecalculateModifiers()
    {
        // Reset modifiers
        cachedSpeedModifier = 1f;
        cachedDamageModifier = 1f;
        cachedDefenseModifier = 1f;

        // Apply all active effects
        foreach (var effect in activeEffects)
        {
            cachedSpeedModifier *= effect.speedModifier;
            cachedDamageModifier *= effect.damageModifier;
            cachedDefenseModifier *= effect.defenseModifier;
        }
    }

    // Public getters for current modifiers
    public float GetSpeedModifier() => cachedSpeedModifier;
    public float GetDamageModifier() => cachedDamageModifier;
    public float GetDefenseModifier() => cachedDefenseModifier;

    private void Die()
    {
        // Handle player death
        Debug.Log("Player died!");
        // You can add your death logic here
    }

    // Helper method to create common status effects
    public static StatusEffect CreatePoisonEffect(float duration, float damagePerSecond)
    {
        return new StatusEffect("Poison", duration, healthMod: -damagePerSecond);
    }

    public static StatusEffect CreateSpeedBoostEffect(float duration, float speedMultiplier)
    {
        return new StatusEffect("SpeedBoost", duration, speedMod: speedMultiplier);
    }

    public static StatusEffect CreateWeakenEffect(float duration)
    {
        return new StatusEffect("Weakened", duration, damageMod: 0.5f, defenseMod: 0.75f);
    }
}
