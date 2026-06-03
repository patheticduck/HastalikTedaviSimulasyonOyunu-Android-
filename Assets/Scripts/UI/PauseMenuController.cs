using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance;

    [Header("Panels To Close On Main Menu")]
    public GameObject doctorBookPanel;
    public GameObject notebookPanel;

    [Header("UI")]
    public GameObject pauseMenuPanel;
    public GameObject crosshair;
    public TextMeshProUGUI resultText;

    [Header("Blocking Panels")]
    public GameObject mainMenuPanel;
    public GameObject doctorSelectionPanel;
    public GameObject settingsPanel;
    public GameObject continueSavePanel;
    public GameObject confirmClearAllContinuePanel;

    [Header("References")]
    public MouseLookLimited mouseLook;
    public GameFlowManager gameFlowManager;
    public PlayerInteractionModeManager interactionModeManager;
    public NotebookInteractable notebookInteractable;
    public DoctorBookInteractable doctorBookInteractable;
    public AIComputerCameraController aiComputerCameraController;

    private bool isPaused = false;
    private bool wasDialogueModeActiveBeforePause = false;
    private bool wasForcedUIModeBeforePause = false;

    // =====================================================
    // SINGLETON REFERANSI
    // =====================================================

    private void Awake()
    {
        Instance = this;
    }

    // =====================================================
    // BAŞLANGIÇ REFERANS VE UI AYARLARI
    // =====================================================

    private void Start()
    {
        // Pause paneli başlangıçta kapatılır.
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Crosshair başlangıçta görünür yapılır.
        if (crosshair != null)
            crosshair.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        isPaused = false;

        // Gerekli referanslar otomatik alınır.
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (interactionModeManager == null)
        {
            interactionModeManager =
                FindFirstObjectByType<PlayerInteractionModeManager>();
        }

        if (aiComputerCameraController == null)
        {
            aiComputerCameraController =
                FindFirstObjectByType<AIComputerCameraController>();
        }
    }

    // =====================================================
    // PAUSE INPUT KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool pausePressed = false;

#if UNITY_EDITOR
        pausePressed = Keyboard.current.pKey.wasPressedThisFrame;
#else
        pausePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
#endif

        if (!pausePressed)
            return;

        // Belirli paneller açıkken pause engellenir.
        if (doctorSelectionPanel != null && doctorSelectionPanel.activeSelf)
            return;

        if (settingsPanel != null && settingsPanel.activeSelf)
            return;

        if (continueSavePanel != null && continueSavePanel.activeSelf)
            return;

        if (confirmClearAllContinuePanel != null &&
            confirmClearAllContinuePanel.activeSelf)
        {
            return;
        }

        // Pause açma / kapama işlemleri.
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            if (CanOpenPause())
                PauseGame();
        }
    }

    // =====================================================
    // PAUSE MENÜSÜNÜN AÇILABİLİRLİK KONTROLÜ
    // =====================================================

    private bool CanOpenPause()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            return false;

        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            return false;

        if (doctorSelectionPanel != null && doctorSelectionPanel.activeSelf)
            return false;

        if (settingsPanel != null && settingsPanel.activeSelf)
            return false;

        if (continueSavePanel != null && continueSavePanel.activeSelf)
            return false;

        if (confirmClearAllContinuePanel != null &&
            confirmClearAllContinuePanel.activeSelf)
        {
            return false;
        }

        // Gameplay geçiş panelleri açıkken pause engellenir.
        if (gameFlowManager != null)
        {
            if (gameFlowManager.patientResultPanel != null &&
                gameFlowManager.patientResultPanel.activeSelf)
            {
                return false;
            }

            if (gameFlowManager.endDayPanel != null &&
                gameFlowManager.endDayPanel.activeSelf)
            {
                return false;
            }

            if (gameFlowManager.gameOverPanel != null &&
                gameFlowManager.gameOverPanel.activeSelf)
            {
                return false;
            }

            if (gameFlowManager.dayTransitionPanel != null &&
                gameFlowManager.dayTransitionPanel.activeSelf)
            {
                return false;
            }
        }

        return true;
    }

    // =====================================================
    // OYUNU DURAKLATMA
    // =====================================================

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        RefreshPauseInfo();

        // Kamera geçişleri devam ediyorsa tamamlanır.
        if (aiComputerCameraController == null)
        {
            aiComputerCameraController =
                FindFirstObjectByType<AIComputerCameraController>();
        }

        if (aiComputerCameraController != null)
        {
            if (aiComputerCameraController.IsMovingToComputer())
            {
                aiComputerCameraController
                    .ForceCompleteComputerMoveForPause();
            }
            else if (aiComputerCameraController.IsMovingToGameplay())
            {
                aiComputerCameraController
                    .ForceCompleteGameplayMoveForPause();
            }
        }

        bool interruptedBookAnimation = false;

        // Notebook animasyonu çalışıyorsa zorla kapatılır.
        if (notebookInteractable != null &&
            notebookInteractable.IsNotebookAnimating())
        {
            notebookInteractable.ForceCloseNotebookImmediate();
            interruptedBookAnimation = true;
        }

        // DoctorBook animasyonu çalışıyorsa zorla kapatılır.
        if (doctorBookInteractable != null &&
            doctorBookInteractable.IsBookAnimating())
        {
            doctorBookInteractable.ForceCloseBookImmediate();
            interruptedBookAnimation = true;
        }

        // UI modu gerekiyorsa sıfırlanır.
        if (interruptedBookAnimation &&
            interactionModeManager != null)
        {
            interactionModeManager.ExitUIMode();
        }

        // Pause öncesi UI durumları kaydedilir.
        wasDialogueModeActiveBeforePause =
            interactionModeManager != null &&
            interactionModeManager.IsDialogueModeActive();

        wasForcedUIModeBeforePause =
            interactionModeManager != null &&
            interactionModeManager.IsInForcedUIMode();

        // Pause paneli açılır.
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            pauseMenuPanel.transform.SetAsLastSibling();
        }

        // Kamera ve cursor ayarları yapılır.
        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);
    }

    // =====================================================
    // OYUNU DEVAM ETTİRME
    // =====================================================

    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        // Pause öncesinde UI modunda olunuyorsa aynı durum korunur.
        if (wasDialogueModeActiveBeforePause ||
            wasForcedUIModeBeforePause)
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(false);

            if (crosshair != null)
                crosshair.SetActive(false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (gameFlowManager != null)
                gameFlowManager.SetNotebookBlocked(true);
        }

        // Normal gameplay moduna dönüş yapılır.
        else
        {
            if (mouseLook != null)
                mouseLook.SetLookEnabled(true);

            if (crosshair != null)
                crosshair.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (gameFlowManager != null)
                gameFlowManager.NotifyFreeLookStateEntered();
        }
    }

    // =====================================================
    // SAVE GAME İŞLEMİ
    // =====================================================

    public void SaveGame()
    {
        if (gameFlowManager == null)
        {
            Debug.LogWarning(
                "PauseMenuController -> GameFlowManager bağlı değil, kayıt alınamadı."
            );

            return;
        }

        if (RuntimeSaveManager.Instance == null)
        {
            Debug.LogWarning(
                "PauseMenuController -> RuntimeSaveManager bulunamadı, kayıt alınamadı."
            );

            return;
        }

        RuntimeSaveManager.Instance.SaveGame(gameFlowManager);
    }

    // =====================================================
    // ANA MENÜYE DÖNÜŞ İŞLEMLERİ
    // =====================================================

    public void ReturnToMainMenu()
    {
        // Gameplay sistemleri durdurulur.
        if (gameFlowManager != null)
            gameFlowManager.ForceStopAllGameplay();

        // Kamera gameplay moduna zorla döndürülür.
        if (aiComputerCameraController == null)
        {
            aiComputerCameraController =
                FindFirstObjectByType<AIComputerCameraController>();
        }

        if (aiComputerCameraController != null)
            aiComputerCameraController.ForceResetToGameplayCamera();

        // Açık UI sistemleri kapatılır.
        if (notebookInteractable != null)
            notebookInteractable.ForceCloseNotebookImmediate();

        if (doctorBookInteractable != null)
            doctorBookInteractable.ForceCloseBookImmediate();

        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        // Main menu ekranı açılır.
        if (gameFlowManager != null &&
            gameFlowManager.mainMenuManager != null)
        {
            gameFlowManager.mainMenuManager.ShowMainMenu();
        }
        else
        {
            Debug.LogWarning(
                "PauseMenuController -> MainMenuManager bulunamadı, menüye dönülemedi."
            );
        }
    }

    // =====================================================
    // PAUSE PANEL BİLGİLERİNİ GÜNCELLEME
    // =====================================================

    private void RefreshPauseInfo()
    {
        if (resultText == null || gameFlowManager == null)
            return;

        int patientInDay =
            ((gameFlowManager.currentPatientNumber - 1)
            % gameFlowManager.patientsPerDay) + 1;

        string selectedDoctorText = "Seçilmedi";

        if (DoctorProfileManager.Instance != null)
        {
            selectedDoctorText =
                DoctorProfileManager.Instance.GetSelectedDoctorDisplayName();
        }

        // Pause ekranındaki oyun bilgileri güncellenir.
        resultText.text =
            "Doktor: " + selectedDoctorText + "\n" +
            "Gün: " + gameFlowManager.currentDay + "\n" +
            "Hasta: " + patientInDay + " (" +
            gameFlowManager.currentPatientNumber + "/" +
            gameFlowManager.totalPatientsInRun + ")\n" +
            "Doğru: " + gameFlowManager.totalCorrect + "\n" +
            "Yanlış: " + gameFlowManager.totalWrong + "\n" +
            "Kalan Hak: " + gameFlowManager.remainingHearts;
    }

    // =====================================================
    // PAUSE DURUM KONTROLÜ
    // =====================================================

    public bool IsPaused()
    {
        return isPaused;
    }
    // =========================
    // MOBİL PAUSE BUTONU
    // =========================

    public void TogglePauseFromButton()
    {
        if (!CanOpenPause())
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
}