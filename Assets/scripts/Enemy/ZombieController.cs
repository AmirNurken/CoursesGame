using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField]
    [Tooltip("Ссылка на трансформ игрока, к которому движется зомби")]
    private Transform player;

    [Header("Behavior Settings")]
    [SerializeField]
    [Tooltip("Дистанция, на которой зомби начинает атаковать игрока")]
    private float attackRange = 7.0f;

    [SerializeField]
    [Tooltip("Задержка между атаками зомби (в секундах)")]
    private float attackCooldown = 0.5f;

    [SerializeField]
    [Tooltip("Время до уничтожения зомби после смерти (в секундах)")]
    private float destroyAfterDeathTime = 5f;

    [SerializeField]
    [Tooltip("Урон, наносимый зомби игроку при атаке")]
    private float attackDamage = 15f;

    [Header("Zigzag Movement Settings")]
    [SerializeField]
    [Tooltip("Базовая скорость движения зомби (единицы в секунду)")]
    private float moveSpeed = 10.0f;

    [SerializeField]
    [Tooltip("Частота колебаний зигзага (чем выше, тем чаще смена направления)")]
    private float zigzagFrequency = 2.0f;

    [SerializeField]
    [Tooltip("Амплитуда зигзага (насколько далеко зомби отклоняется в стороны)")]
    private float zigzagAmplitude = 3.0f;

    [SerializeField]
    [Tooltip("Вероятность использования зигзага вместо прямого движения (0-1)")]
    private float zigzagChance = 0.7f;

    [SerializeField]
    [Tooltip("Минимальное расстояние до игрока, при котором зигзаг активен")]
    private float zigzagMinDistance = 10.0f;

    [Header("Separation Settings")]
    [SerializeField]
    [Tooltip("Желаемая дистанция между зомби (в единицах)")]
    private float separationDistance = 2.0f;

    [SerializeField]
    [Tooltip("Сила отталкивания от других зомби (0-1)")]
    private float separationForce = 0.5f;

    [Header("Speed Boost Settings")]
    [SerializeField]
    [Tooltip("Вероятность случайного ускорения (0-1)")]
    private float speedBoostChance = 0.3f;

    [SerializeField]
    [Tooltip("Множитель скорости при ускорении (например, 1.5 = 150% от базовой скорости)")]
    private float speedBoostMultiplier = 1.5f;

    [SerializeField]
    [Tooltip("Длительность ускорения (в секундах)")]
    private float speedBoostDuration = 2.0f;

    [Header("Dodge Settings")]
    [SerializeField]
    [Tooltip("Дистанция обнаружения пуль (в единицах)")]
    private float bulletDetectionRange = 15.0f;

    [SerializeField]
    [Tooltip("Скорость уклонения от пули (единицы в секунду)")]
    private float dodgeSpeed = 15.0f;

    [SerializeField]
    [Tooltip("Длительность уклонения (в секундах)")]
    private float dodgeDuration = 0.5f;

    [SerializeField]
    [Tooltip("Вероятность попытки уклонения от пули (0-1)")]
    private float dodgeChance = 0.6f;

    [Header("Audio Settings")]
    [SerializeField]
    [Tooltip("Звук атаки зомби")]
    private AudioClip attackSound;

    [Header("Animation Settings")]
    [SerializeField]
    [Tooltip("Длительность начального крика зомби (в секундах)")]
    private float initialScreamDuration = 2.8f;

    [SerializeField]
    [Tooltip("Длительность анимации атаки по умолчанию (в секундах)")]
    private float defaultAttackDuration = 2.0f;

    [SerializeField]
    [Tooltip("Длительность анимации реакции на удар по умолчанию (в секундах)")]
    private float defaultHitDuration = 2.0f;

    private NavMeshAgent agent;
    private Animator animator;
    private HealthController healthController;
    private AudioSource audioSource;
    private Rigidbody rb;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool initialScreamPlayed = false;
    private float zigzagTimer = 0f;
    private bool isDodging = false;
    private float dodgeTimer = 0f;
    private Vector3 dodgeDirection;
    private bool isBoosting = false;
    private float boostTimer = 0f;

    private readonly int IsScreamingHash = Animator.StringToHash("isScreaming");
    private readonly int SpeedHash = Animator.StringToHash("Speed");
    private readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private readonly int IsHitHash = Animator.StringToHash("isHit");
    private readonly int IsDeadHash = Animator.StringToHash("isDead");
    private readonly int AttackTypeHash = Animator.StringToHash("attackType");
    private readonly int DeathTypeHash = Animator.StringToHash("deathType");

    void Awake()
    {
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            Debug.Log($"Collider check in Awake for {gameObject.name}: Height = {collider.height}, Radius = {collider.radius}, Center = {collider.center}, Is Trigger = {collider.isTrigger}, Layer = {LayerMask.LayerToName(gameObject.layer)}");
        }

        healthController = GetComponent<HealthController>();
        if (healthController == null)
        {
            Debug.LogError($"HealthController is missing on the zombie {gameObject.name}!");
            enabled = false;
            return;
        }

        healthController.OnDeath += Die;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log($"Player found for {gameObject.name} at {player.position}");
            }
            else
            {
                Debug.LogError($"Player not assigned in ZombieController for {gameObject.name} and no Player tag found!");
                enabled = false;
                return;
            }
        }

        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent is missing on the zombie {gameObject.name}!");
            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError($"Animator is missing on the zombie {gameObject.name}!");
            enabled = false;
            return;
        }

        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            Debug.LogError($"Capsule Collider is missing on the zombie {gameObject.name}!");
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        agent.stoppingDistance = 3.5f;
        agent.radius = 0.01f;
        agent.height = 0.5f;
        agent.angularSpeed = 360f;

        initialScreamPlayed = false;
        animator.SetBool(IsScreamingHash, true);
        Invoke(nameof(StopInitialScream), initialScreamDuration);
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            Debug.LogWarning($"Player reference is null for {gameObject.name}, zombie cannot move!");
            return;
        }

        if (!initialScreamPlayed)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float currentSpeed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat(SpeedHash, currentSpeed > 0.1f ? 1.0f : 0.0f);

        if (animator.GetBool(IsHitHash) || animator.GetBool(IsAttackingHash))
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }

        // Проверка уклонения от пуль
        if (!isDodging && TryDetectBullet(out Vector3 bulletDirection))
        {
            if (Random.value < dodgeChance)
            {
                StartDodge(bulletDirection);
            }
        }

        // Логика уклонения
        if (isDodging)
        {
            agent.isStopped = false;
            agent.speed = dodgeSpeed;
            agent.SetDestination(transform.position + dodgeDirection * 5f);
            dodgeTimer -= Time.deltaTime;

            if (dodgeTimer <= 0f)
            {
                isDodging = false;
                agent.speed = isBoosting ? moveSpeed * speedBoostMultiplier : moveSpeed;
                Debug.Log($"Dodge finished for {gameObject.name}");
            }
            return;
        }

        // Логика случайного ускорения
        if (!isBoosting && Random.value < speedBoostChance * Time.deltaTime)
        {
            StartSpeedBoost();
        }

        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                isBoosting = false;
                agent.speed = moveSpeed;
                Debug.Log($"Speed boost ended for {gameObject.name}");
            }
        }

        // Обычное движение
        if (distanceToPlayer > attackRange)
        {
            agent.isStopped = false;

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Vector3 targetDirection;

            if (distanceToPlayer > zigzagMinDistance && Random.value < zigzagChance)
            {
                zigzagTimer += Time.deltaTime * zigzagFrequency;
                float zigzagOffset = Mathf.Sin(zigzagTimer) * zigzagAmplitude;
                Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
                targetDirection = (directionToPlayer + perpendicular * zigzagOffset).normalized;
                Debug.Log($"Zigzag active for {gameObject.name}, Offset: {zigzagOffset}");
            }
            else
            {
                targetDirection = directionToPlayer;
                Debug.Log($"Straight movement for {gameObject.name}");
            }

            // Добавляем разделение от других зомби
            Vector3 separation = CalculateSeparation();
            targetDirection = (targetDirection + separation * separationForce).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            agent.SetDestination(transform.position + targetDirection * 10f);
        }
        else
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }

            Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            if (Time.time >= nextAttackTime)
            {
                Debug.Log($"Attempting attack at distance: {distanceToPlayer}, Time: {Time.time}");
                Attack();
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;
        Collider[] nearbyZombies = Physics.OverlapSphere(transform.position, separationDistance, LayerMask.GetMask("Enemy"));
        foreach (Collider zombie in nearbyZombies)
        {
            if (zombie.gameObject != gameObject)
            {
                Vector3 directionAway = (transform.position - zombie.transform.position).normalized;
                float distance = Vector3.Distance(transform.position, zombie.transform.position);
                if (distance > 0)
                {
                    separation += directionAway / distance; // Чем ближе, тем сильнее отталкивание
                }
            }
        }
        return separation.normalized;
    }

    void StartSpeedBoost()
    {
        isBoosting = true;
        boostTimer = speedBoostDuration;
        agent.speed = moveSpeed * speedBoostMultiplier;
        Debug.Log($"Speed boost started for {gameObject.name}, Speed: {agent.speed}");
    }

    bool TryDetectBullet(out Vector3 bulletDirection)
    {
        bulletDirection = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(transform.position, bulletDetectionRange);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Bullet"))
            {
                Bullet bullet = hit.GetComponent<Bullet>();
                if (bullet != null)
                {
                    Vector3 directionToBullet = (hit.transform.position - transform.position).normalized;
                    Vector3 bulletVelocity = hit.GetComponent<Rigidbody>().linearVelocity.normalized;

                    float dot = Vector3.Dot(bulletVelocity, -directionToBullet);
                    if (dot > 0.5f)
                    {
                        bulletDirection = bulletVelocity;
                        Debug.Log($"Bullet detected by {gameObject.name} at {hit.transform.position}, moving towards zombie!");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void StartDodge(Vector3 bulletDirection)
    {
        isDodging = true;
        dodgeTimer = dodgeDuration;
        Vector3 perpendicular = Vector3.Cross(bulletDirection, Vector3.up).normalized;
        dodgeDirection = (Random.value > 0.5f ? perpendicular : -perpendicular).normalized;
        Debug.Log($"Dodge started for {gameObject.name}, Direction: {dodgeDirection}");
    }

    void StopInitialScream()
    {
        initialScreamPlayed = true;
        animator.SetBool(IsScreamingHash, false);
        Debug.Log($"Initial scream finished for {gameObject.name}, zombie can now move.");
    }

    void Attack()
    {
        float attackType = Random.value;
        animator.SetFloat(AttackTypeHash, attackType);
        animator.SetBool(IsAttackingHash, true);

        nextAttackTime = Time.time + attackCooldown;

        float attackDuration = GetCurrentAnimationLength();
        if (attackDuration <= 0) attackDuration = defaultAttackDuration;

        Debug.Log($"Attack triggered for {gameObject.name}, Duration: {attackDuration} seconds, AttackType: {attackType}");
        Invoke(nameof(ResetAttack), attackDuration);
    }

    void ResetAttack()
    {
        animator.SetBool(IsAttackingHash, false);
        Debug.Log($"Attack reset for {gameObject.name}, returning to movement.");
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            Debug.Log($"TakeDamage called on dead zombie {gameObject.name}, ignoring.");
            return;
        }

        if (!initialScreamPlayed)
        {
            Debug.Log($"TakeDamage called before initial scream for {gameObject.name}, ignoring.");
            return;
        }

        float healthBefore = healthController.GetCurrentHealth();
        healthController.TakeDamage(damage);
        float healthAfter = healthController.GetCurrentHealth();

        Debug.Log($"Zombie {gameObject.name} took {damage} damage. Health before: {healthBefore}, Health after: {healthAfter}");

        if (healthAfter <= 0)
        {
            Die();
        }
        else
        {
            animator.SetBool(IsHitHash, true);
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }

            float hitDuration = GetCurrentAnimationLength();
            if (hitDuration <= 0) hitDuration = defaultHitDuration;

            Debug.Log($"TakeDamage triggered for {gameObject.name}, Hit Duration: {hitDuration} seconds");
            Invoke(nameof(ResetHit), hitDuration);
        }
    }

    void ResetHit()
    {
        animator.SetBool(IsHitHash, false);
        Debug.Log($"Hit reset for {gameObject.name}, resuming movement if not attacking.");

        if (!isDead && !animator.GetBool(IsAttackingHash))
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = false;
            }
        }
    }

    public void Die()
    {
        if (isDead)
        {
            Debug.Log($"Die already called for {gameObject.name}, skipping.");
            return;
        }

        isDead = true;
        enabled = false;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
            Debug.Log($"NavMeshAgent disabled for {gameObject.name}.");
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.freezeRotation = true;
            Debug.Log($"Rigidbody set to physics mode for {gameObject.name}.");
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
            Debug.Log($"Collider disabled on {col.gameObject.name} for {gameObject.name}.");
        }

        CancelInvoke();
        Debug.Log($"All pending Invokes cancelled for {gameObject.name}.");

        float deathType = Random.value;
        animator.SetFloat(DeathTypeHash, deathType);
        animator.SetBool(IsDeadHash, true);

        animator.Update(0f);
        Debug.Log($"Animator forced update for death animation, DeathType: {deathType} for {gameObject.name}.");

        Destroy(gameObject, destroyAfterDeathTime);
        Debug.Log($"Death triggered for {gameObject.name}, will be destroyed in {destroyAfterDeathTime} seconds.");
    }

    private float GetCurrentAnimationLength()
    {
        if (animator == null) return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.length > 0)
        {
            return stateInfo.length;
        }

        Debug.LogWarning($"Could not determine current animation length for {gameObject.name}, using default.");
        return 0f;
    }

    void OnTriggerStay(Collider other)
    {
        if (isDead) return;

        Debug.Log($"OnTriggerStay called for {gameObject.name} with: {other.gameObject.name}, Tag: {other.tag}, IsAttacking: {animator.GetBool(IsAttackingHash)}, Distance: {Vector3.Distance(transform.position, other.transform.position)}");

        if (other.gameObject.CompareTag("Bullet") || other.gameObject.name.Contains("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Debug.Log($"Zombie {gameObject.name} took {bullet.damage} damage from Bullet!");
                Destroy(other.gameObject);
            }
            else
            {
                Debug.LogWarning($"No Bullet component found on {other.gameObject.name} for {gameObject.name}");
            }
        }

        if (animator.GetBool(IsAttackingHash) && other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                if (Time.time >= nextAttackTime)
                {
                    player.TakeDamage(attackDamage);
                    Debug.Log($"Zombie {gameObject.name} dealt {attackDamage} damage to Player at {Time.time}!");

                    if (audioSource != null && attackSound != null)
                    {
                        audioSource.PlayOneShot(attackSound);
                    }

                    nextAttackTime = Time.time + attackCooldown;
                }
            }
            else
            {
                Debug.LogWarning($"PlayerMovement component not found on {other.gameObject.name} for {gameObject.name}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (this == null || agent == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bulletDetectionRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, separationDistance);
    }

    public bool IsDead()
    {
        return isDead;
    }
}