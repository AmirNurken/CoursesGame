using UnityEngine;
using System;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Blood Effect Settings")]
    [SerializeField]
    [Tooltip("Префаб эффекта крови")]
    private GameObject bloodEffectPrefab;

    [SerializeField]
    [Tooltip("Масштаб эффекта крови")]
    private float bloodEffectScale = 1f;

    [SerializeField]
    [Tooltip("Время жизни эффекта крови (в секундах)")]
    private float bloodEffectLifetime = 1f;

    [SerializeField]
    [Tooltip("Смещение эффекта крови по высоте от центра NPC")]
    private float bloodOffsetY = 0.5f;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Vector3? hitPosition = null)
    {
        if (currentHealth <= 0) return;

        Debug.Log($"HealthController: {gameObject.name} taking {damage} damage. Current health before: {currentHealth}");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (bloodEffectPrefab != null && currentHealth > 0)
        {
            Vector3 spawnPosition = hitPosition ?? (transform.position + Vector3.up * bloodOffsetY);
            GameObject blood = Instantiate(bloodEffectPrefab, spawnPosition, Quaternion.identity);

            // Используем UnityEngine.Random для избежания конфликта
            blood.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            blood.transform.localScale = Vector3.one * bloodEffectScale;
            Destroy(blood, bloodEffectLifetime);
            Debug.Log($"Spawned blood effect at {spawnPosition} for {gameObject.name}");
        }

        Debug.Log($"HealthController: {gameObject.name} health after damage: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} health reached 0, invoking OnDeath.");
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        Debug.Log($"{gameObject.name} healed for {amount}. Current health: {currentHealth}");
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died.");
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}