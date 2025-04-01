using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float lifetime = 10f; // Увеличено время жизни до 10 секунд
    public GameObject vfxPrefab; // Префаб VFX-огня
    public float vfxScale = 1f; // Масштаб VFX-огня (1 = стандартный размер)
    private float damage;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // Отключаем гравитацию
        }

        // Добавляем VFX-огонь, если префаб указан
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            vfx.transform.SetParent(transform); // Делаем VFX дочерним объектом Fireball
            vfx.transform.localPosition = Vector3.zero; // Центрируем VFX на Fireball
            vfx.transform.localScale = Vector3.one * vfxScale; // Устанавливаем масштаб
            Debug.Log($"Added VFX to Fireball with scale {vfxScale}");
        }
        else
        {
            Debug.LogWarning("VFX Prefab is not assigned in Fireball!");
        }

        Destroy(gameObject, lifetime); // Уничтожить объект через lifetime секунд
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Fireball dealt {damage} damage to Player!");
            }
            Destroy(gameObject); // Уничтожить огненный шар после попадания
        }
    }
}