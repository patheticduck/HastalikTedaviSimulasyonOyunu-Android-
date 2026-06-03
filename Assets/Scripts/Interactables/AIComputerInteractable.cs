using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class AIComputerInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public GameFlowManager gameFlowManager;
    public AIComputerCameraController aiComputerController;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    [Header("Mobile Button")]
    public GameObject computerButton;
    public float interactDistance = 3f;

    [Header("Interaction Text")]
    public string lockedText = "AI Asistan Kilitli";
    public string unlockedText = "[E] Yapay Zeka Asistanını Aç";

    private bool isLookingAt = false;

    // =====================================================
    // REFERANS VE BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Start()
    {
        // Kamera referansı atanmadıysa ana kamera alınır.
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Gerekli sistem referansları otomatik alınır.
        if (interactionManager == null)
        {
            interactionManager =
                FindFirstObjectByType<PlayerInteractionModeManager>();
        }

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (aiComputerController == null)
        {
            aiComputerController =
                FindFirstObjectByType<AIComputerCameraController>();
        }

        HideVisuals();
    }

    // =====================================================
    // INTERACTION VE INPUT KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Pause durumunda interaction çalışmaz.
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        // Kamera geçiş animasyonu sırasında interaction çalışmaz.
        if (aiComputerController != null &&
            aiComputerController.IsMoving())
        {
            return;
        }

        // Bilgisayar modu açıksa E ile kapatılır.
        if (aiComputerController != null &&
            aiComputerController.IsComputerModeActive() &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            aiComputerController.CloseComputerMode();
            return;
        }
        // Bilgisayar modu açıksa interaction kontrolü durdurulur.
        if (aiComputerController != null &&
            aiComputerController.IsComputerModeActive())
        {
            HideVisuals();
            return;
        }
        CheckLookAt();

        // Interaction UI sürekli güncellenir.
        if (isLookingAt)
            RefreshInteractionVisuals();

        // E tuşuna basıldığında bilgisayar açılır.
        if (isLookingAt &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryOpenComputer();
        }
    }

    // =====================================================
    // BİLGİSAYARA BAKIŞ KONTROLÜ
    // =====================================================

    private void CheckLookAt()
    {
        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitComputer = false;

        // Raycast ile oyuncunun bilgisayara bakıp bakmadığı kontrol edilir.
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            AIComputerInteractable computer =
                hit.collider.GetComponentInParent<AIComputerInteractable>();

            if (computer == this)
                hitComputer = true;
        }

        // Bakış durumu değiştiğinde interaction görselleri güncellenir.
        if (hitComputer != isLookingAt)
        {
            isLookingAt = hitComputer;

            if (computerButton != null)
            {
                computerButton.SetActive(isLookingAt);
            }
            RefreshInteractionVisuals();
        }
    }

    // =====================================================
    // INTERACTION GÖRSELLERİNİ GÜNCELLEME
    // =====================================================

    private void RefreshInteractionVisuals()
    {
        bool canShow =
            isLookingAt &&
            CanShowInteractionPrompt();

        // Highlight objesi güncellenir.
        if (highlightObject != null)
            highlightObject.SetActive(canShow);

        // Interaction yazısı güncellenir.
        if (interactText != null)
        {
            interactText.SetActive(canShow);

            if (canShow)
            {
                TextMeshProUGUI tmp =
                    interactText.GetComponent<TextMeshProUGUI>();

                if (tmp != null)
                {
                    tmp.text = IsAIUnlocked()
                        ? unlockedText
                        : lockedText;
                }
            }
        }
    }

    // =====================================================
    // INTERACTION PROMPT GÖSTERİM KONTROLÜ
    // =====================================================

    private bool CanShowInteractionPrompt()
    {
        if (interactionManager == null)
            return false;

        // Diyalog modunda interaction engellenir.
        if (interactionManager.IsDialogueModeActive())
            return false;

        // Zorunlu UI modunda interaction engellenir.
        if (interactionManager.IsInForcedUIMode())
            return false;

        // Hasta masaya gelmeden interaction gösterilmez.
        if (gameFlowManager != null &&
            !gameFlowManager.CanOpenNotebook())
        {
            return false;
        }

        return true;
    }

    // =====================================================
    // BİLGİSAYAR MODUNU AÇMA
    // =====================================================

    public void TryOpenComputer()
    {
        if (!CanShowInteractionPrompt())
            return;

        // AI sistemi kilitliyse açılmaz.
        if (!IsAIUnlocked())
            return;

        HideVisuals();
        isLookingAt = false;

        // Mobil computer butonu gizlenir.
        if (computerButton != null)
            computerButton.SetActive(false);

        // Bilgisayar kamera modu açılır.
        if (aiComputerController != null)
            aiComputerController.OpenComputerMode();
    }

    // =====================================================
    // AI ANALİZ SİSTEMİNİN KİLİT DURUMU
    // =====================================================

    private bool IsAIUnlocked()
    {
        if (gameFlowManager == null)
            return false;

        return gameFlowManager
            .IsAIAnalysisUnlockedForCurrentPatient();
    }

    // =====================================================
    // INTERACTION GÖRSELLERİNİ GİZLEME
    // =====================================================

    private void HideVisuals()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);

        if (computerButton != null)
            computerButton.SetActive(false);
    }
}