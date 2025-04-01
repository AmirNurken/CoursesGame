using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float rotationSpeed = 5f;
    public GameObject pistol;
    public GameObject sword;
    public GameObject playerModel;

    [Header("Audio Settings")]
    public AudioClip hitSound;

    [Header("UI Settings")]
    public Image playerHealthFill; // Ссылка на Image полоски здоровья ГГ

    private int mode = 1;
    private Vector3 movementDirection;
    private WeaponUIController weaponUIController;
    private GunController gunController;
    private SwordController swordController;
    private float idleTimer = 0f;
    private float idleThreshold = 5f;
    private float attackDuration = 0.2f;
    private bool isShooting = false;
    private float rightButtonHoldTime = 0f;
    private float holdThreshold = 2f;
    private bool isRightButtonHeld = false;
    private Rigidbody rb;
    private AnimationController animationController;
    private HealthController healthController;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animationController = GetComponent<AnimationController>();
        healthController = GetComponent<HealthController>();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            Debug.LogError("Rigidbody is missing on the player!");
        }

        if (healthController == null)
        {
            Debug.LogError("HealthController is missing on the player!");
        }
        else
        {
            healthController.OnHealthChanged += UpdateHealthBar; // Подписка на изменение здоровья
            healthController.OnDeath += Die; // Подписка на смерть
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        InitializeComponents();
        SetInitialWeaponMode();
        UpdateHealthBar(healthController.GetHealthPercentage()); // Инициализация полоски
    }

    void InitializeComponents()
    {
        gunController = GetComponent<GunController>();
        weaponUIController = FindFirstObjectByType<WeaponUIController>();

        if (sword != null)
        {
            swordController = sword.GetComponent<SwordController>();
        }

        if (animationController == null) Debug.LogError("AnimationController missing!");
        if (gunController == null) Debug.LogError("GunController missing!");
        if (weaponUIController == null) Debug.LogError("WeaponUIController missing!");
        if (swordController == null) Debug.LogError("SwordController missing!");
        if (pistol == null) Debug.LogError("Pistol not assigned!");
        if (sword == null) Debug.LogError("Sword not assigned!");
        if (playerHealthFill == null) Debug.LogError("PlayerHealthFill Image is not assigned!");
    }

    void SetInitialWeaponMode()
    {
        mode = 1;
        pistol.SetActive(true);
        sword.SetActive(false);

        if (gunController) gunController.enabled = true;
        if (swordController) swordController.enabled = false;

        if (animationController)
        {
            animationController.SetMode(mode);
            weaponUIController.UpdateWeaponUI(mode);
        }
    }

    void Update()
    {
        // Проверяем, активна ли какая-либо панель UI, чтобы отключить ввод
        if (UIManager.Instance != null && (UIManager.Instance.deathPanel.activeSelf || UIManager.Instance.pausePanel.activeSelf))
        {
            return; // Прерываем обработку ввода, если панель активна
        }

        HandleMovementInput();
        HandleRotation();
        UpdateAnimations();
        HandleWeaponSwitching();
        HandleAttacks();
    }

    void HandleMovementInput()
    {
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0)
        {
            movementDirection = transform.forward;
        }
        else if (vertical < 0)
        {
            movementDirection = -transform.forward;
        }
        else
        {
            movementDirection = Vector3.zero;
        }
    }

    void HandleRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDirection = (hitPoint - transform.position).normalized;
            lookDirection.y = 0;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateAnimations()
    {
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (mode == 1 || mode == 2)
        {
            animationController.SetMovement(verticalInput);

            if (verticalInput == 0 && !isShooting && !animationController.IsAttacking() && !animationController.IsDamaged())
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleThreshold)
                {
                    animationController.SetIdleActive(true);
                }
            }
            else
            {
                idleTimer = 0f;
                animationController.SetIdleActive(false);
            }
        }
    }

    void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToGunMode();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToSwordMode();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animationController.SetRolling(true);
        }
    }

    void HandleAttacks()
    {
        if (mode == 1) // Gun Mode
        {
            HandleGunAttacks();
        }
        else if (mode == 2) // Sword Mode
        {
            HandleSwordAttacks();
        }
    }

    void HandleGunAttacks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gunController.Shoot();
            isShooting = true;
        }

        if (Input.GetMouseButton(1))
        {
            TrackRightMouseButton(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            ResetRightMouseButton();
        }

        if (!Input.GetMouseButton(0))
        {
            isShooting = false;
        }
    }

    void HandleSwordAttacks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (swordController != null)
            {
                swordController.Attack(1);
                animationController.SetAttacking2(true);
                Invoke("ResetAttack2", attackDuration);
                Debug.Log("Left mouse button pressed, sword attack (Type 1) triggered.");
            }
            else
            {
                Debug.LogError("SwordController is null, cannot perform attack!");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (swordController != null)
            {
                swordController.Attack(2);
                animationController.SetIdleActive(false);
                animationController.SetStableSword(true);
                Invoke("ResetStableSword", attackDuration);
                Debug.Log("Right mouse button pressed, sword attack (Type 2) triggered.");
            }
            else
            {
                Debug.LogError("SwordController is null, cannot perform attack!");
            }
        }
    }

    void TrackRightMouseButton(bool isGunMode)
    {
        if (!isRightButtonHeld)
        {
            isRightButtonHeld = true;
            rightButtonHoldTime = 0f;
        }
        rightButtonHoldTime += Time.deltaTime;

        if (rightButtonHoldTime >= holdThreshold && isGunMode)
        {
            if (gunController != null)
            {
                Debug.Log("Right mouse button held for 2 seconds, calling ShootBurst(5)");
                gunController.ShootBurst(5);
                animationController.SetDisarmed(true);
                Invoke("ResetDisarmed", attackDuration);
            }
            else
            {
                Debug.LogError("GunController is null in TrackRightMouseButton!");
            }
            isRightButtonHeld = false;
            rightButtonHoldTime = 0f;
        }
    }

    void ResetRightMouseButton()
    {
        isRightButtonHeld = false;
        rightButtonHoldTime = 0f;
    }

    void FixedUpdate()
    {
        if (movementDirection != Vector3.zero)
        {
            Vector3 moveVelocity = movementDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + moveVelocity);
            Debug.Log($"Player position: {rb.position}, Velocity: {rb.linearVelocity}");
        }
    }

    void SwitchToGunMode()
    {
        mode = 1;
        weaponUIController.UpdateWeaponUI(mode);
        pistol.SetActive(true);
        sword.SetActive(false);
        gunController.enabled = true;
        swordController.enabled = false;
        animationController.SetMode(mode);
        ResetAttackStates();
    }

    void SwitchToSwordMode()
    {
        mode = 2;
        weaponUIController.UpdateWeaponUI(mode);
        pistol.SetActive(false);
        sword.SetActive(true);
        gunController.enabled = false;
        swordController.enabled = true;
        animationController.SetMode(mode);
        ResetAttackStates();
    }

    void ResetAttackStates()
    {
        animationController.SetAttacking2(false);
        animationController.SetStableSword(false);
        animationController.SetDisarmed(false);
    }

    void ResetAttack2() => animationController.SetAttacking2(false);
    void ResetStableSword() => animationController.SetStableSword(false);
    void ResetDisarmed() => animationController.SetDisarmed(false);

    public void TakeDamage(float damage)
    {
        if (healthController == null)
        {
            Debug.LogError("HealthController is missing on the player!");
            return;
        }

        Debug.Log($"Player taking {damage} damage. Current health before: {healthController.GetCurrentHealth()}");

        healthController.TakeDamage(damage);

        Debug.Log($"Player health after damage: {healthController.GetCurrentHealth()}");

        if (animationController != null)
        {
            animationController.SetDamaged(true);
            Debug.Log("SetDamaged called with true");
        }
        else
        {
            Debug.LogError("AnimationController is missing on the player!");
        }

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private void UpdateHealthBar(float healthPercentage)
    {
        if (playerHealthFill != null)
        {
            playerHealthFill.fillAmount = healthPercentage;

            // Градиент от зеленого (100%) к красному (0%)
            Color healthColor = Color.Lerp(Color.red, Color.green, healthPercentage);
            playerHealthFill.color = healthColor;
            Debug.Log($"Player health updated: {healthPercentage * 100}%, Color: {healthColor}");
        }
    }

    private void Die()
    {
        Debug.Log("PlayerMovement: Die method called! Player is dead.");

        if (animationController != null)
        {
            animationController.SetDead(true);
            Debug.Log("SetDead called with true, playing death animation");
        }
        else
        {
            Debug.LogError("AnimationController is missing on the player!");
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        enabled = false;

        // Показываем панель смерти через UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathPanel();
        }
        else
        {
            Debug.LogError("UIManager is not initialized!");
        }

        Debug.Log("Player has died!");
    }
}