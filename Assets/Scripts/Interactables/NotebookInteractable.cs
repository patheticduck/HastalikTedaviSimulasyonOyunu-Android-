using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NotebookInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public NotebookUIManager notebookUIManager;
    public GameFlowManager gameFlowManager;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    [Header("Mobile Button")]
    public GameObject notebookButton;
    public float interactDistance = 3f;

    [Header("Interaction Text")]
    public string notebookInteractText = "[E] Kayıt Defterini Aç";

    [Header("Animation")]
    public float moveSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, -0.05f, 0.6f);

    [Header("Open Rotation Offset")]
    public Vector3 openRotationOffset = new Vector3(75f, 0f, 0f);

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isOpen = false;
    private bool isAnimating = false;
    private bool isLookingAt = false;

    // =====================================================
    // REFERANS VE BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Start()
    {
        // Kamera atanmadıysa ana kamera alınır.
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Gerekli sistem referansları otomatik alınır.
        if (interactionManager == null)
        {
            interactionManager =
                FindFirstObjectByType<PlayerInteractionModeManager>();
        }

        if (notebookUIManager == null)
        {
            notebookUIManager =
                FindFirstObjectByType<NotebookUIManager>();
        }

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        // Başlangıç pozisyonu kaydedilir.
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Notebook paneli başlangıçta kapatılır.
        if (notebookUIManager != null &&
            notebookUIManager.notebookPanel != null)
        {
            notebookUIManager.notebookPanel.SetActive(false);
        }

        // Interaction görselleri başlangıçta gizlenir.
        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);

        if (notebookButton != null)
            notebookButton.SetActive(false);
    }

    // =====================================================
    // INPUT VE INTERACTION KONTROLLERİ
    // =====================================================

    private void Update()
    {
        // Pause sırasında interaction çalışmaz.
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        if (Keyboard.current == null)
            return;

        CheckLookAt();

        // E tuşu ile notebook açılır / kapanır.
        if (isLookingAt &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryToggleNotebook();
        }

        // ESC ile notebook kapatılır.
        if (isOpen &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StartCoroutine(CloseNotebook());
        }
    }

    // =====================================================
    // NOTEBOOK'A BAKIŞ KONTROLÜ
    // =====================================================

    private void CheckLookAt()
    {
        if (playerCamera == null || isAnimating)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        RaycastHit hit;

        bool hitNotebook = false;

        // Raycast ile notebook hedef kontrolü yapılır.
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider != null &&
                hit.collider.GetComponentInParent<NotebookInteractable>() == this)
            {
                hitNotebook = true;
            }
        }

        // Bakış durumu değişirse interaction UI güncellenir.
        if (hitNotebook != isLookingAt)
        {
            isLookingAt = hitNotebook;
            if (notebookButton != null)
            {
                notebookButton.SetActive(isLookingAt);
            }
            RefreshInteractionVisuals();
        }
    }

    // =====================================================
    // INTERACTION GÖRSELLERİNİ GÜNCELLEME
    // =====================================================

    private void RefreshInteractionVisuals()
    {
        bool canShowPrompt =
            isLookingAt &&
            !isOpen &&
            !isAnimating &&
            CanShowInteractionPrompt();

        // Highlight objesi güncellenir.
        if (highlightObject != null)
            highlightObject.SetActive(canShowPrompt);

        // Interaction yazısı güncellenir.
        if (interactText != null)
        {
            interactText.SetActive(canShowPrompt);

            if (canShowPrompt)
            {
                TextMeshProUGUI tmp =
                    interactText.GetComponent<TextMeshProUGUI>();

                if (tmp != null)
                    tmp.text = notebookInteractText;
            }
        }
    }

    // =====================================================
    // INTERACTION PROMPT GÖSTERİM KONTROLÜ
    // =====================================================

    private bool CanShowInteractionPrompt()
    {
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return false;
        }

        if (interactionManager == null)
            return false;

        // Diyalog modunda notebook açılamaz.
        if (interactionManager.IsDialogueModeActive())
            return false;

        // Hasta masaya ulaşmadan notebook açılamaz.
        if (gameFlowManager != null &&
            !gameFlowManager.CanOpenNotebook())
        {
            return false;
        }

        return true;
    }

    // =====================================================
    // NOTEBOOK AÇMA / KAPAMA KONTROLÜ
    // =====================================================

    public void TryToggleNotebook()
    {
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        if (isAnimating)
            return;

        // Gerekli referanslar eksikse hata verilir.
        if (playerCamera == null)
        {
            Debug.LogError(
                $"NotebookInteractable -> playerCamera atanmadı: {gameObject.name}",
                this
            );

            return;
        }

        if (interactionManager == null)
        {
            Debug.LogError(
                $"NotebookInteractable -> interactionManager atanmadı: {gameObject.name}",
                this
            );

            return;
        }

        // Notebook açıksa kapatılır.
        if (isOpen)
        {
            StartCoroutine(CloseNotebook());
            return;
        }

        if (!CanOpenNotebookNow())
            return;

        StartCoroutine(OpenNotebook());
    }

    // =====================================================
    // NOTEBOOK AÇILABİLİRLİK KONTROLÜ
    // =====================================================

    private bool CanOpenNotebookNow()
    {
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return false;
        }

        if (interactionManager == null)
            return false;

        if (interactionManager.IsDialogueModeActive())
            return false;

        if (gameFlowManager != null &&
            !gameFlowManager.CanOpenNotebook())
        {
            return false;
        }

        return true;
    }

    // =====================================================
    // NOTEBOOK AÇILIŞ ANİMASYONU
    // =====================================================

    private IEnumerator OpenNotebook()
    {
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            yield break;
        }

        if (!CanOpenNotebookNow())
            yield break;

        isAnimating = true;
        isOpen = true;

        // Açılış sesi oynatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        RefreshInteractionVisuals();

        // UI modu aktif edilir.
        interactionManager.EnterUIMode();

        if (notebookButton != null)
            notebookButton.SetActive(false);

        // Kameraya göre hedef pozisyon hesaplanır.
        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * cameraOffset.z +
            playerCamera.transform.up * cameraOffset.y +
            playerCamera.transform.right * cameraOffset.x;

        Quaternion baseLookRot =
            Quaternion.LookRotation(
                playerCamera.transform.forward,
                playerCamera.transform.up
            );

        Quaternion targetRot =
            baseLookRot * Quaternion.Euler(openRotationOffset);

        float t = 0f;

        // Notebook açılış animasyonu oynatılır.
        while (t < 1f)
        {
            if (PauseMenuController.Instance != null &&
                PauseMenuController.Instance.IsPaused())
            {
                yield break;
            }

            if (playerCamera == null)
                break;

            t += Time.deltaTime * moveSpeed;

            transform.position =
                Vector3.Lerp(originalPosition, targetPos, t);

            transform.rotation =
                Quaternion.Slerp(originalRotation, targetRot, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        // Notebook UI açılır.
        if (notebookUIManager != null)
            notebookUIManager.OpenNotebookUI();

        isAnimating = false;

        RefreshInteractionVisuals();
    }

    // =====================================================
    // NOTEBOOK KAPANIŞ ANİMASYONU
    // =====================================================

    private IEnumerator CloseNotebook()
    {
        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            yield break;
        }

        isAnimating = true;
        isOpen = false;

        // Kapanış sesi oynatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        // Notebook UI kapatılır.
        if (notebookUIManager != null)
            notebookUIManager.CloseNotebookUI();

        float t = 0f;

        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        // Notebook eski konumuna döndürülür.
        while (t < 1f)
        {
            if (PauseMenuController.Instance != null &&
                PauseMenuController.Instance.IsPaused())
            {
                yield break;
            }

            t += Time.deltaTime * moveSpeed;

            transform.position =
                Vector3.Lerp(currentPos, originalPosition, t);

            transform.rotation =
                Quaternion.Slerp(currentRot, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // UI modu kapatılır.
        interactionManager.ExitUIMode();

        if (notebookButton != null && isLookingAt)
            notebookButton.SetActive(true);

        if (gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();

        isAnimating = false;

        RefreshInteractionVisuals();
    }

    // =====================================================
    // NOTEBOOK DURUM KONTROLLERİ
    // =====================================================

    public bool IsNotebookOpen()
    {
        return isOpen;
    }

    public bool IsNotebookAnimating()
    {
        return isAnimating;
    }

    // =====================================================
    // UI BUTONU ÜZERİNDEN NOTEBOOK KAPATMA
    // =====================================================

    public void CloseNotebookFromButton()
    {
        if (!isOpen || isAnimating)
            return;

        if (PauseMenuController.Instance != null &&
            PauseMenuController.Instance.IsPaused())
        {
            return;
        }

        StartCoroutine(CloseNotebook());
    }

    // =====================================================
    // NOTEBOOK'U ANINDA KAPATMA
    // =====================================================

    public void ForceCloseNotebookImmediate()
    {
        StopAllCoroutines();

        isOpen = false;
        isAnimating = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (notebookUIManager != null)
            notebookUIManager.CloseNotebookUI();

        RefreshInteractionVisuals();
    }
}