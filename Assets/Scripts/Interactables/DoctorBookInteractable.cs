using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DoctorBookInteractable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public PlayerInteractionModeManager interactionManager;
    public DoctorBookUIManager doctorBookUIManager;
    public GameFlowManager gameFlowManager;

    [Header("Interaction")]
    public GameObject highlightObject;
    public GameObject interactText;
    public float interactDistance = 3f;

    [Header("Interaction Text")]
    public string unlockedInteractText = "Doktor Kitabını Aç";
    public string lockedInteractText = "Doktor Kitabı kilitli - Enerji kullan";

    [Header("Mobile")]
    public GameObject mobileDoctorBookButton;

    [Header("Animation")]
    public float moveSpeed = 5f;
    public Vector3 cameraOffset = new Vector3(0f, -0.05f, 0.6f);

    [Header("Open Rotation Offset")]
    public Vector3 openRotationOffset = new Vector3(-75f, -180f, 180f);

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

        // Sistem referansları otomatik alınır.
        if (interactionManager == null)
        {
            interactionManager =
                FindFirstObjectByType<PlayerInteractionModeManager>();
        }

        if (doctorBookUIManager == null)
        {
            doctorBookUIManager =
                FindFirstObjectByType<DoctorBookUIManager>();
        }

        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        // Kitabın başlangıç pozisyonu kaydedilir.
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Interaction görselleri başlangıçta gizlenir.
        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (interactText != null)
            interactText.SetActive(false);

        if (mobileDoctorBookButton != null)
            mobileDoctorBookButton.SetActive(false);
    }

    // =====================================================
    // INPUT VE INTERACTION KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (IsPauseActive())
            return;

        if (Keyboard.current == null)
            return;

        CheckLookAt();

        // E tuşu ile kitabı açma / kapama.
        if (isLookingAt &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryToggleBook();
        }

        // ESC ile kitabı kapatma.
        if (isOpen &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseBook();
        }
    }

    // =====================================================
    // PAUSE DURUM KONTROLÜ
    // =====================================================

    private bool IsPauseActive()
    {
        if (PauseMenuController.Instance == null)
            return false;

        if (PauseMenuController.Instance.IsPaused())
            return true;

        if (PauseMenuController.Instance.pauseMenuPanel != null &&
            PauseMenuController.Instance.pauseMenuPanel.activeInHierarchy)
        {
            return true;
        }

        return false;
    }

    // =====================================================
    // KİTABA BAKIŞ KONTROLÜ
    // =====================================================

    private void CheckLookAt()
    {
        if (playerCamera == null || isAnimating || isOpen)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        bool hitBook = false;

        // Oyuncunun kitaba bakıp bakmadığı raycast ile kontrol edilir.
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider != null &&
                hit.collider.GetComponentInParent<DoctorBookInteractable>() == this)
            {
                hitBook = true;
            }
        }

        // Bakış durumu güncellenir.
        isLookingAt = hitBook;

        // Interaction UI sürekli güncellenir.
        RefreshInteractionVisuals();
    }

    // =====================================================
    // INTERACTION GÖRSELLERİNİ GÜNCELLEME
    // =====================================================

    private void RefreshInteractionVisuals()
    {
        bool canShow =
            isLookingAt &&
            !isOpen &&
            !isAnimating;

        // Highlight objesi güncellenir.
        if (highlightObject != null)
            highlightObject.SetActive(canShow);

        if (mobileDoctorBookButton != null)
        {
            mobileDoctorBookButton.SetActive(canShow);
            Debug.Log(canShow);
        }
        // Interaction yazısı güncellenir.
        if (interactText != null)
        {
            interactText.SetActive(canShow && !Application.isMobilePlatform);

            if (canShow)
            {
                TextMeshProUGUI tmp =
                    interactText.GetComponent<TextMeshProUGUI>();

                if (tmp != null)
                {
                    if (IsDoctorBookUnlocked())
                        tmp.text = unlockedInteractText;
                    else
                        tmp.text = lockedInteractText;
                }
            }
        }
    }

    // =====================================================
    // KİTABIN AÇILABİLİRLİK KONTROLÜ
    // =====================================================

    private bool CanOpenBookNow()
    {
        if (IsPauseActive())
            return false;

        if (interactionManager == null)
            return false;

        // Diyalog modunda kitap açılamaz.
        if (interactionManager.IsDialogueModeActive())
            return false;

        // Hasta masaya ulaşmadan kitap açılamaz.
        if (gameFlowManager != null &&
            !gameFlowManager.CanOpenNotebook())
        {
            return false;
        }

        return true;
    }

    // =====================================================
    // KİTABIN KİLİT DURUMU
    // =====================================================

    private bool IsDoctorBookUnlocked()
    {
        if (gameFlowManager == null)
            return false;

        return gameFlowManager
            .IsDoctorBookUnlockedForCurrentPatient();
    }

    // =====================================================
    // KİTABI AÇMA / KAPAMA İŞLEMİ
    // =====================================================

    public void TryToggleBook()
    {
        if (IsPauseActive())
            return;

        if (isAnimating)
            return;

        // Kitap açıksa kapatılır.
        if (isOpen)
        {
            CloseBook();
            return;
        }

        // Açılabilir durumda değilse işlem yapılmaz.
        if (!CanOpenBookNow())
            return;

        // Kilitliyse sadece interaction UI güncellenir.
        if (!IsDoctorBookUnlocked())
        {
            RefreshInteractionVisuals();
            return;
        }

        StartCoroutine(OpenBookRoutine());
    }

    // =====================================================
    // KİTABI KAMERAYA DOĞRU AÇMA ANİMASYONU
    // =====================================================

    private IEnumerator OpenBookRoutine()
    {
        isAnimating = true;
        isOpen = true;

        // Açılış sesi oynatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        RefreshInteractionVisuals();

        // UI modu aktif edilir.
        if (interactionManager != null)
            interactionManager.EnterUIMode();

        // Mobil doctor book butonu gizlenir.
        if (mobileDoctorBookButton != null)
            mobileDoctorBookButton.SetActive(false);

        yield return null;
        yield return new WaitForEndOfFrame();

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

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        // Kitap açılış animasyonu oynatılır.
        while (t < 1f)
        {
            if (IsPauseActive())
            {
                isAnimating = false;
                yield break;
            }

            t += Time.deltaTime * moveSpeed;

            transform.position =
                Vector3.Lerp(startPos, targetPos, t);

            transform.rotation =
                Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        // Book panel güvenli şekilde açılır.
        yield return StartCoroutine(SafeOpenBookPanelRoutine());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isAnimating = false;

        RefreshInteractionVisuals();
    }

    // =====================================================
    // BOOK PANELİNİ GÜVENLİ ŞEKİLDE AÇMA
    // =====================================================

    private IEnumerator SafeOpenBookPanelRoutine()
    {
        if (doctorBookUIManager == null)
            yield break;

        doctorBookUIManager.OpenBook();

        yield return null;
        yield return new WaitForEndOfFrame();

        // Panel açılmadıysa tekrar denenir.
        if (!doctorBookUIManager.IsBookPanelOpen())
            doctorBookUIManager.OpenBook();

        yield return null;

        if (!doctorBookUIManager.IsBookPanelOpen())
            doctorBookUIManager.OpenBook();
    }

    // =====================================================
    // KİTABI KAPATMA İŞLEMİ
    // =====================================================

    private void CloseBook()
    {
        if (IsPauseActive())
            return;

        if (!isOpen || isAnimating)
            return;

        StartCoroutine(CloseBookRoutine());
    }

    public void CloseBookFromButton()
    {
        if (IsPauseActive())
            return;

        CloseBook();
    }

    // =====================================================
    // KİTABI ANINDA KAPATMA
    // =====================================================

    public void ForceCloseBookImmediate()
    {
        StopAllCoroutines();

        isOpen = false;
        isAnimating = false;

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (doctorBookUIManager != null)
            doctorBookUIManager.CloseBook();

        RefreshInteractionVisuals();
    }

    // =====================================================
    // ANİMASYON DURUM KONTROLÜ
    // =====================================================

    public bool IsBookAnimating()
    {
        return isAnimating;
    }

    // =====================================================
    // KİTABI ESKİ KONUMUNA GERİ GÖNDERME
    // =====================================================

    private IEnumerator CloseBookRoutine()
    {
        isAnimating = true;

        // Kapanış sesi oynatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBookOpenClose();

        if (doctorBookUIManager != null)
            doctorBookUIManager.CloseBook();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;

        // Kitap kapanış animasyonu oynatılır.
        while (t < 1f)
        {
            if (IsPauseActive())
            {
                isAnimating = false;
                yield break;
            }

            t += Time.deltaTime * moveSpeed;

            transform.position =
                Vector3.Lerp(startPos, originalPosition, t);

            transform.rotation =
                Quaternion.Slerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        isOpen = false;
        isAnimating = false;

        // UI modu kapatılır.
        if (interactionManager != null)
            interactionManager.ExitUIMode();

        // Oyuncu hala doctor book'a bakıyorsa
        // mobil buton tekrar gösterilir.
        if (mobileDoctorBookButton != null && isLookingAt)
            mobileDoctorBookButton.SetActive(true);

        // Gameplay free look modu geri açılır.
        if (gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();

        RefreshInteractionVisuals();
    }
}