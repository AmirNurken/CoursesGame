using UnityEngine;

public class SwordController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float[] attackRanges = new float[10] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f }; // Радиусы для 10 точек
    public float attackRate = 0.3f;
    public float damage = 20f;
    private float lastAttackTime = 0f;

    [Header("Attack Points")]
    public Transform[] attackPoints = new Transform[10]; // Массив из 10 точек атаки

    private int enemyLayer;

    void Start()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (attackPoints[i] == null)
            {
                Debug.LogError($"AttackPoint {i} is not assigned in SwordController!");
            }
        }

        enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1)
        {
            Debug.LogError("Layer 'Enemy' not found! Please ensure it exists in the Layer settings.");
        }
    }

    public void Attack(int attackType)
    {
        if (Time.time >= lastAttackTime + attackRate)
        {
            lastAttackTime = Time.time;
            Debug.Log($"Sword attack (Type {attackType}) triggered at Time: {Time.time}");

            int layerMask = 1 << enemyLayer;
            bool anyHit = false;

            for (int i = 0; i < attackPoints.Length; i++)
            {
                if (attackPoints[i] == null)
                {
                    Debug.LogWarning($"Skipping attackPoint {i} because it is null!");
                    continue;
                }

                Debug.Log($"Checking attackPoint {i} at position: {attackPoints[i].position}, Range: {attackRanges[i]}");
                Collider[] hits = Physics.OverlapSphere(attackPoints[i].position, attackRanges[i], layerMask);

                if (hits.Length > 0)
                {
                    anyHit = true;
                    Debug.Log($"AttackPoint {i} detected {hits.Length} hits");

                    foreach (Collider hit in hits)
                    {
                        ZombieController zombie = hit.GetComponentInParent<ZombieController>();
                        if (zombie != null && !zombie.IsDead())
                        {
                            HealthController zombieHealth = hit.GetComponentInParent<HealthController>();
                            float healthBefore = zombieHealth != null ? zombieHealth.GetCurrentHealth() : -1f;
                            zombie.TakeDamage(damage);
                            float healthAfter = zombieHealth != null ? zombieHealth.GetCurrentHealth() : -1f;

                            Debug.Log($"Sword (Type {attackType}, Point {i}) dealt {damage} damage to {hit.gameObject.name}. " +
                                      $"Health before: {healthBefore}, Health after: {healthAfter}");
                        }
                        else if (zombie == null)
                        {
                            Debug.LogWarning($"Hit {hit.gameObject.name} on Enemy layer but no ZombieController found!");
                        }
                    }
                }
            }

            if (!anyHit)
            {
                Debug.Log($"No enemies hit by any attackPoint (Type {attackType}). Check positions and ranges!");
            }
        }
        else
        {
            Debug.Log($"Sword attack (Type {attackType}) on cooldown. Time remaining: {lastAttackTime + attackRate - Time.time:F2} seconds");
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < attackPoints.Length; i++)
        {
            if (attackPoints[i] != null)
            {
                Gizmos.color = (i % 2 == 0) ? Color.red : Color.blue;
                Gizmos.DrawWireSphere(attackPoints[i].position, attackRanges[i]);
            }
        }
    }
}