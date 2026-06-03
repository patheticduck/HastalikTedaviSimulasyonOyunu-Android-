using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIComputerCameraController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform aiComputerCameraPoint;
    public Transform gameplayCameraPoint;
    public AIAnalysisUIManager aiAnalysisUIManager;
    public PlayerInteractionModeManager interactionModeManager;
    public GameFlowManager gameFlowManager;
    public PauseMenuController pauseMenuController;

    [Header("Settings")]
    public Key toggleKey = Key.None;
    public float moveDuration = 0.45f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isComputerModeActive = false;
    private bool isMoving = false;

    private bool movingToComputer = false;
    private bool movingToGameplay = false;

    private Coroutine moveRoutine;

    // =====================================================
    // REFERANS VE BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Start()
    {
        // Kamera atanmadıysa ana kamera alınır.
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Gerekli sistem referansları otomatik alınır.
        if (aiAnalysisUIManager == null)
        {
            aiAnalysisUIManager =
                FindFirstObjectByType<AIAnalysisUIManager>();
        }

        if (interactionModeManager == null)
        {
            interactionModeManager =
                FindFirstObjectByType<PlayerInteractionModeManager>();
        }

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (pauseMenuController == null)
        {
            pauseMenuController =
                FindFirstObjectByType<PauseMenuController>();
        }

        // AI panelinin dışarıdan kapanma aksiyonu atanır.
        if (aiAnalysisUIManager != null)
        {
            aiAnalysisUIManager
                .SetExternalCloseAction(CloseComputerMode);
        }
    }

    // =====================================================
    // INPUT VE MOD KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Pause sırasında kamera geçişleri çalışmaz.
        if (pauseMenuController != null &&
            pauseMenuController.IsPaused())
        {
            return;
        }

        // ToggleKey None ise manuel input kontrolü yapılmaz.
        // Bilgisayar artık interactable sistemi üzerinden açılıyor.
        if (toggleKey == Key.None)
            return;

        // Toggle tuşu ile bilgisayar modu açılır / kapanır.
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (isMoving)
                return;

            if (isComputerModeActive)
                CloseComputerMode();
            else
                OpenComputerMode();
        }
    }

    // =====================================================
    // BİLGİSAYAR MODUNU AÇMA
    // =====================================================

    public void OpenComputerMode()
    {
        if (mainCamera == null ||
            aiComputerCameraPoint == null ||
            aiAnalysisUIManager == null)
        {
            return;
        }

        if (isMoving || isComputerModeActive)
            return;

        if (pauseMenuController != null &&
            pauseMenuController.IsPaused())
        {
            return;
        }

        // Diyalog modunda AI paneli açılamaz.
        if (interactionModeManager != null &&
            interactionModeManager.IsDialogueModeActive())
        {
            return;
        }

        // AI sistemi kilitliyse açılmaz.
        if (gameFlowManager != null)
        {
            if (!gameFlowManager.IsAIAnalysisUnlockedForCurrentPatient())
                return;

            if (!gameFlowManager.CanOpenAIAnalysisPanel())
                return;
        }

        // Kamera başlangıç pozisyonu kaydedilir.
        startPosition = mainCamera.transform.position;
        startRotation = mainCamera.transform.rotation;

        isComputerModeActive = true;

        movingToComputer = true;
        movingToGameplay = false;

        // UI modu aktif edilir.
        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        // Eski coroutine durdurulur.
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        // Kamera AI bilgisayar noktasına hareket ettirilir.
        moveRoutine = StartCoroutine(
            MoveCameraRoutine(
                aiComputerCameraPoint.position,
                aiComputerCameraPoint.rotation,
                true
            )
        );
    }

    // =====================================================
    // BİLGİSAYAR MODUNU KAPATMA
    // =====================================================

    public void CloseComputerMode()
    {
        if (!isComputerModeActive && !isMoving)
            return;

        // AI paneli kapatılır.
        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        movingToComputer = false;
        movingToGameplay = true;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        // Kamera gameplay pozisyonuna geri döndürülür.
        moveRoutine = StartCoroutine(
            MoveCameraRoutine(
                startPosition,
                startRotation,
                false
            )
        );
    }

    // =====================================================
    // DURUM KONTROLLERİ
    // =====================================================

    public bool IsComputerModeActive()
    {
        return isComputerModeActive;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsMovingToComputer()
    {
        return movingToComputer;
    }

    public bool IsMovingToGameplay()
    {
        return movingToGameplay;
    }

    // =====================================================
    // PAUSE İÇİN BİLGİSAYAR MODUNU TAMAMLAMA
    // =====================================================

    public void ForceCompleteComputerMoveForPause()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;

        movingToComputer = false;
        movingToGameplay = false;

        isComputerModeActive = true;

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Kamera doğrudan bilgisayar pozisyonuna yerleştirilir.
        if (mainCamera != null &&
            aiComputerCameraPoint != null)
        {
            mainCamera.transform.position =
                aiComputerCameraPoint.position;

            mainCamera.transform.rotation =
                aiComputerCameraPoint.rotation;
        }

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.OpenPanel();
    }

    // =====================================================
    // PAUSE İÇİN GAMEPLAY MODUNA DÖNÜŞ
    // =====================================================

    public void ForceCompleteGameplayMoveForPause()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;

        movingToComputer = false;
        movingToGameplay = false;

        isComputerModeActive = false;

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Kamera gameplay pozisyonuna yerleştirilir.
        if (mainCamera != null &&
            gameplayCameraPoint != null)
        {
            mainCamera.transform.position =
                gameplayCameraPoint.position;

            mainCamera.transform.rotation =
                gameplayCameraPoint.rotation;
        }

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();
    }

    // =====================================================
    // GAMEPLAY KAMERASINA ZORLA RESETLEME
    // =====================================================

    public void ForceResetToGameplayCamera(bool exitUIMode = true)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        isMoving = false;

        movingToComputer = false;
        movingToGameplay = false;

        isComputerModeActive = false;

        if (aiAnalysisUIManager != null)
            aiAnalysisUIManager.ClosePanelOnly();

        if (mainCamera == null)
            mainCamera = Camera.main;

        // Kamera gameplay noktasına sıfırlanır.
        if (mainCamera != null &&
            gameplayCameraPoint != null)
        {
            mainCamera.transform.position =
                gameplayCameraPoint.position;

            mainCamera.transform.rotation =
                gameplayCameraPoint.rotation;
        }

        if (exitUIMode &&
            interactionModeManager != null)
        {
            interactionModeManager.ExitUIMode();
        }
    }

    // =====================================================
    // KAMERA GEÇİŞ ANİMASYONU
    // =====================================================

    private IEnumerator MoveCameraRoutine(
        Vector3 targetPosition,
        Quaternion targetRotation,
        bool openPanelAfterMove)
    {
        isMoving = true;

        Vector3 fromPosition = mainCamera.transform.position;
        Quaternion fromRotation = mainCamera.transform.rotation;

        float elapsed = 0f;

        // Kamera geçiş animasyonu oynatılır.
        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);

            // Smoothstep benzeri geçiş efekti uygulanır.
            t = t * t * (3f - 2f * t);

            mainCamera.transform.position =
                Vector3.Lerp(fromPosition, targetPosition, t);

            mainCamera.transform.rotation =
                Quaternion.Slerp(fromRotation, targetRotation, t);

            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;

        isMoving = false;

        movingToComputer = false;
        movingToGameplay = false;

        // AI paneli gerekiyorsa açılır.
        if (openPanelAfterMove)
        {
            if (aiAnalysisUIManager != null)
                aiAnalysisUIManager.OpenPanel();
        }

        // Gameplay moduna geri dönülür.
        else
        {
            isComputerModeActive = false;

            if (interactionModeManager != null)
                interactionModeManager.ExitUIMode();

            if (gameFlowManager != null)
                gameFlowManager.NotifyFreeLookStateEntered();
        }

        moveRoutine = null;
    }
}