using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject deathPanel; // Панель Death
    public GameObject pausePanel; // Панель Pause

    // Прямые ссылки на кнопки
    public Button tryAgainButton;
    public Button exitDeathButton;
    public Button continueButton;
    public Button exitPauseButton;
    public Button restartButton; // Новая кнопка Restart

    private bool isPaused = false;

    void Awake()
    {
        // Настройка Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager initialized and set as DontDestroyOnLoad on MenuCanvas");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Сброс состояния панелей при инициализации (например, после перезапуска)
        if (deathPanel != null && deathPanel.activeSelf)
        {
            deathPanel.SetActive(false);
            Debug.Log("Death Panel deactivated on Awake");
        }
        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            isPaused = false;
            Debug.Log("Pause Panel deactivated on Awake");
        }

        // Проверка, что панели назначены
        if (deathPanel == null)
        {
            Debug.LogError("Death Panel is not assigned! Please assign it in the Inspector.");
            return;
        }
        if (pausePanel == null)
        {
            Debug.LogError("Pause Panel is not assigned! Please assign it in the Inspector.");
            return;
        }

        // Автоматическое назначение кнопок, если не заданы вручную
        if (tryAgainButton == null)
        {
            tryAgainButton = deathPanel.transform.Find("TryAgainButton")?.GetComponent<Button>();
            if (tryAgainButton == null) Debug.LogError("TryAgainButton not found in Death Panel!");
        }
        
        if (exitDeathButton == null)
        {
            exitDeathButton = deathPanel.transform.Find("ExitButton")?.GetComponent<Button>();
            if (exitDeathButton == null) Debug.LogError("ExitButton (Death) not found!");
        }
        
        if (continueButton == null)
        {
            continueButton = pausePanel.transform.Find("ContinueButton")?.GetComponent<Button>();
            if (continueButton == null) Debug.LogError("ContinueButton not found in Pause Panel!");
        }
        
        if (exitPauseButton == null)
        {
            exitPauseButton = pausePanel.transform.Find("ExitButton")?.GetComponent<Button>();
            if (exitPauseButton == null) Debug.LogError("ExitButton (Pause) not found!");
        }

        // Автоматическое назначение новой кнопки Restart
        if (restartButton == null)
        {
            restartButton = deathPanel.transform.Find("RestartButton")?.GetComponent<Button>(); // Ищем кнопку в Death Panel
            if (restartButton == null)
            {
                restartButton = pausePanel.transform.Find("RestartButton")?.GetComponent<Button>(); // Или в Pause Panel
                if (restartButton == null) Debug.LogError("RestartButton not found in either Death or Pause Panel!");
            }
        }

        // Изначально отключаем панели (дополнительная защита)
        deathPanel.SetActive(false);
        pausePanel.SetActive(false);

        // Программное назначение обработчиков для кнопок
        Debug.Log("Assigning listeners to buttons...");
        AssignButtonListeners();
    }

    // Функция для назначения обработчиков кнопок
    private void AssignButtonListeners()
    {
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.RemoveAllListeners();
            tryAgainButton.onClick.AddListener(() => { Debug.Log("Try Again button clicked!"); RestartGame(); });
            Debug.Log("TryAgainButton listener assigned");
            if (!tryAgainButton.interactable) tryAgainButton.interactable = true;
        }
        else
        {
            Debug.LogError("TryAgainButton is null, cannot assign listener!");
        }

        if (exitDeathButton != null)
        {
            exitDeathButton.onClick.RemoveAllListeners();
            exitDeathButton.onClick.AddListener(() => { Debug.Log("Exit button clicked (Death Panel)!"); ExitGame(); });
            Debug.Log("ExitButton (Death) listener assigned");
            if (!exitDeathButton.interactable) exitDeathButton.interactable = true;
        }
        else
        {
            Debug.LogError("ExitDeathButton is null, cannot assign listener!");
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => { Debug.Log("Continue button clicked!"); ResumeGame(); });
            Debug.Log("ContinueButton listener assigned");
            if (!continueButton.interactable) continueButton.interactable = true;
        }
        else
        {
            Debug.LogError("ContinueButton is null, cannot assign listener!");
        }

        if (exitPauseButton != null)
        {
            exitPauseButton.onClick.RemoveAllListeners();
            exitPauseButton.onClick.AddListener(() => { Debug.Log("Exit button clicked (Pause Panel)!"); ExitGame(); });
            Debug.Log("ExitButton (Pause) listener assigned");
            if (!exitPauseButton.interactable) exitPauseButton.interactable = true;
        }
        else
        {
            Debug.LogError("ExitPauseButton is null, cannot assign listener!");
        }

        // Обработчик для новой кнопки Restart
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => { Debug.Log("Restart button clicked!"); RestartGame(); });
            Debug.Log("RestartButton listener assigned");
            if (!restartButton.interactable) restartButton.interactable = true;
        }
        else
        {
            Debug.LogError("RestartButton is null, cannot assign listener!");
        }
    }

    void OnEnable()
    {
        // Пересвязываем обработчики кнопок при активации объекта
        AssignButtonListeners();
    }

    public void ShowDeathPanel()
    {
        if (deathPanel == null)
        {
            Debug.LogError("Cannot show Death Panel because it is null!");
            return;
        }
        Time.timeScale = 0f; // Останавливаем игру
        deathPanel.SetActive(true);
        Debug.Log("Death Panel shown");
    }

    public void ShowPausePanel()
    {
        if (pausePanel == null || deathPanel == null)
        {
            Debug.LogError("Cannot show Pause Panel because one of the panels is null!");
            return;
        }
        if (!isPaused && !deathPanel.activeSelf)
        {
            Time.timeScale = 0f; // Останавливаем игру
            pausePanel.SetActive(true);
            isPaused = true;
            Debug.Log("Pause Panel shown");
        }
    }

    public void ResumeGame()
    {
        if (pausePanel == null)
        {
            Debug.LogError("Cannot resume game because Pause Panel is null!");
            return;
        }
        Time.timeScale = 1f; // Возобновляем игру
        pausePanel.SetActive(false);
        isPaused = false;
        Debug.Log("Game resumed");
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame called");
        if (deathPanel != null && deathPanel.activeSelf)
        {
            deathPanel.SetActive(false); // Деактивируем панель перед перезапуском
            Debug.Log("Death Panel deactivated before scene reload");
        }
        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false); // Деактивируем паузу перед перезапуском
            Debug.Log("Pause Panel deactivated before scene reload");
        }
        Time.timeScale = 1f; // Возобновляем время
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Перезапуск текущей сцены
        Debug.Log("Game restarted");
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame called");
        Application.Quit();
        Debug.Log("Game is exiting...");
        
        // Для работы в редакторе Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Публичные методы для вызова из редактора Unity
    public void TestRestartGame() { RestartGame(); }
    public void TestExitGame() { ExitGame(); }
    public void TestResumeGame() { ResumeGame(); }
    public void TestShowDeathPanel() { ShowDeathPanel(); }
    public void TestShowPausePanel() { ShowPausePanel(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (deathPanel != null && deathPanel.activeSelf)
            {
                return;
            }
            
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                ShowPausePanel();
            }
        }
    }
}