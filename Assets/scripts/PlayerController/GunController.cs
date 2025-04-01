using UnityEngine;

public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f; // Уменьшенная скорость для лучшей видимости
    public float spreadAngle = 10f; // Базовый угол рассеяния для одиночного выстрела
    public float burstSpreadAngle = 25f; // Угол рассеяния для залпового выстрела (дробовик)

    void Start()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned in GunController!");
            enabled = false;
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError("Fire point is not assigned in GunController!");
            enabled = false;
            return;
        }

        // Отладка направления firePoint
        Debug.Log($"FirePoint direction: {firePoint.forward}");
    }

    public void Shoot()
    {
        // Увеличиваем случайное смещение для спавна
        Vector3 spawnOffset = firePoint.forward * 0.3f + Random.insideUnitSphere * 0.2f;
        Vector3 spawnPosition = firePoint.position + spawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        // Определяем направление с базовым рассеянием
        Vector3 shootDirection = ApplySpread(firePoint.forward);

        if (rb != null)
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
            Debug.Log($"Bullet {bullet.GetInstanceID()} linear velocity set to: {rb.linearVelocity}, Position: {bullet.transform.position}, Direction: {shootDirection}");
        }
        else
        {
            Debug.LogError("Rigidbody missing on bullet prefab!");
        }

        MeshRenderer renderer = bullet.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Debug.Log($"Bullet {bullet.GetInstanceID()} MeshRenderer is enabled");
        }
        else
        {
            Debug.LogError("MeshRenderer missing on bullet prefab!");
        }

        // Устанавливаем ротацию пули, чтобы она смотрела в направлении движения
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);
    }

    public void ShootBurst(int bulletCount)
    {
        Debug.Log($"ShootBurst called with bulletCount: {bulletCount}");
        
        // Создаём несколько пуль с равномерным разбросом (эффект дробовика)
        float angleStep = 360f / bulletCount; // Равномерное распределение по кругу
        for (int i = 0; i < bulletCount; i++)
        {
            Debug.Log($"Creating bullet {i + 1} of {bulletCount}");
            
            // Случайное смещение позиции для спавна
            Vector3 spawnOffset = firePoint.forward * 0.3f + Random.insideUnitSphere * 0.3f;
            Vector3 spawnPosition = firePoint.position + spawnOffset;
            
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            
            // Вычисляем направление с равномерным рассеянием
            Vector3 shootDirection = ApplyBurstSpread(firePoint.forward, i, angleStep);
            
            if (rb != null)
            {
                rb.linearVelocity = shootDirection * bulletSpeed;
                Debug.Log($"Burst bullet {i + 1} linear velocity set to: {rb.linearVelocity}, Direction: {shootDirection}");
            }
            else
            {
                Debug.LogError("Rigidbody missing on bullet prefab!");
            }
            
            // Устанавливаем ротацию пули, чтобы она смотрела в направлении движения
            bullet.transform.rotation = Quaternion.LookRotation(shootDirection);
        }
        
        Debug.Log($"ShootBurst completed, created {bulletCount} bullets with spread");
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        float spread = spreadAngle * Mathf.Deg2Rad;
        Vector3 spreadDirection = Vector3.RotateTowards(
            direction,
            Random.insideUnitSphere,
            spread,
            0.0f
        );
        return spreadDirection.normalized;
    }
    
    private Vector3 ApplyBurstSpread(Vector3 direction, int index, float angleStep)
    {
        // Преобразуем угол в радианы для равномерного распределения
        float spread = burstSpreadAngle * Mathf.Deg2Rad;
        float angle = index * angleStep * Mathf.Deg2Rad;
        
        // Создаём вектор в плоскости XY с заданным углом
        Vector3 randomOffset = new Vector3(
            Mathf.Cos(angle) * Random.Range(0f, spread),
            Mathf.Sin(angle) * Random.Range(0f, spread),
            0f
        );
        
        // Добавляем случайное вертикальное отклонение для 3D-эффекта
        randomOffset += Vector3.up * Random.Range(-spread * 0.5f, spread * 0.5f);
        
        // Преобразуем в мировые координаты и нормализуем
        Quaternion rotation = Quaternion.LookRotation(direction);
        Vector3 spreadDirection = rotation * randomOffset + direction;
        
        return spreadDirection.normalized;
    }

    // Визуализация направления firePoint для отладки
    void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f); // Рисуем синий луч направления
        }
    }
}