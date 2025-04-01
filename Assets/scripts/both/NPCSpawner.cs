using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField]
    [Tooltip("Префаб NPC для спавна")]
    private GameObject npcPrefab;

    [SerializeField]
    [Tooltip("Интервал спавна (в секундах)")]
    private float spawnInterval = 10f;

    [SerializeField]
    [Tooltip("Максимальное количество NPC, которые могут быть активны одновременно")]
    private int maxNPCs = 10;

    [Header("Player Detection")]
    [SerializeField]
    [Tooltip("Расстояние активации спавнера (в метрах)")]
    private float activationDistance = 30f;

    [SerializeField]
    [Tooltip("Тег игрока для обнаружения")]
    private string playerTag = "Player";

    private float timer;
    private int currentNPCCount = 0;
    private Transform playerTransform;
    private bool playerInRange = false;

    void Start()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab is not assigned in NPCSpawner!");
            enabled = false;
            return;
        }

        timer = spawnInterval;
        
        // Найти игрока по тегу
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning($"Player with tag '{playerTag}' not found! Spawner won't activate until player is found.");
        }
    }

    void Update()
    {
        // Если игрок еще не найден, пытаемся найти его
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("Player found, spawner can now activate based on distance.");
            }
            return;
        }

        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Обновляем статус нахождения игрока в радиусе активации
        playerInRange = distanceToPlayer <= activationDistance;
        
        if (!playerInRange || currentNPCCount >= maxNPCs) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnNPC();
            timer = spawnInterval;
            Debug.Log($"Next NPC spawn in {spawnInterval} seconds. Player is {distanceToPlayer.ToString("F2")}m away.");
        }
    }

    void SpawnNPC()
    {
        Vector3 spawnPosition = transform.position;

        GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        currentNPCCount++;
        Debug.Log($"Spawned NPC at {spawnPosition}. Current NPC count: {currentNPCCount}");

        HealthController healthController = npc.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.OnDeath += () => DecreaseNPCCount(npc);
        }
        else
        {
            Debug.LogWarning($"HealthController missing on NPC {npc.name}. Count won't decrease on death!");
        }
    }

    void DecreaseNPCCount(GameObject npc)
    {
        currentNPCCount--;
        Debug.Log($"NPC {npc.name} died. Current NPC count: {currentNPCCount}");
    }

    // Визуализация радиуса активации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}