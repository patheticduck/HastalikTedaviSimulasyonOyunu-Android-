using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUIManager : MonoBehaviour
{
    [Header("References")]
    public GameFlowManager gameFlowManager;
    public GameObject confirmPanel;

    [Header("Buttons")]
    public Button useEnergyButton;

    public Button doctorBookButton;
    public Button aiAnalysisButton;
    public Button noButton;

    [Header("Texts")]
    public TextMeshProUGUI confirmText;

    [Header("Energy Icons")]
    public Image[] energyIcons;

    [Header("Optional Text")]
    public TextMeshProUGUI energyInfoText;

    // =====================================================
    // REFERANS VE BUTTON EVENT AYARLARI
    // =====================================================

    private void Awake()
    {
        // GameFlowManager referansı otomatik alınır.
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        // Confirm paneli başlangıçta kapatılır.
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Enerji kullanım butonu eventleri atanır.
        if (useEnergyButton != null)
        {
            useEnergyButton.onClick.RemoveListener(OnClickUseEnergy);
            useEnergyButton.onClick.AddListener(OnClickUseEnergy);
        }

        // DoctorBook enerji butonu eventleri atanır.
        if (doctorBookButton != null)
        {
            doctorBookButton.onClick.RemoveListener(OnDoctorBookButton);
            doctorBookButton.onClick.AddListener(OnDoctorBookButton);
        }

        // AI analiz enerji butonu eventleri atanır.
        if (aiAnalysisButton != null)
        {
            aiAnalysisButton.onClick.RemoveListener(OnAIAnalysisButton);
            aiAnalysisButton.onClick.AddListener(OnAIAnalysisButton);
        }

        // Vazgeç butonu eventleri atanır.
        if (noButton != null)
        {
            noButton.onClick.RemoveListener(OnConfirmNo);
            noButton.onClick.AddListener(OnConfirmNo);
        }
    }

    // =====================================================
    // BAŞLANGIÇ UI GÜNCELLEME
    // =====================================================

    private void Start()
    {
        UpdateEnergyUI();
    }

    private void OnEnable()
    {
        UpdateEnergyUI();
    }

    // =====================================================
    // ENERJİ KULLANIM PANELİNİ AÇMA
    // =====================================================

    public void OnClickUseEnergy()
    {
        if (gameFlowManager == null)
            return;

        UpdateEnergyUI();

        // İpucu paneli açılamıyorsa işlem iptal edilir.
        if (!gameFlowManager.CanOpenHintPanel())
            return;

        // Confirm paneli açılır.
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
            confirmPanel.transform.SetAsLastSibling();
        }

        UpdateHintButtons();

        // Confirm mesajı güncellenir.
        if (confirmText != null)
        {
            confirmText.text =
                "1 enerji harcayarak ipucu kullanmak istediğinize emin misiniz?";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =====================================================
    // DOCTOR BOOK ENERJİ KULLANIMI
    // =====================================================

    public void OnDoctorBookButton()
    {
        if (gameFlowManager != null)
            gameFlowManager.TryUseEnergyForDoctorBook();

        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    // =====================================================
    // AI ANALİZ ENERJİ KULLANIMI
    // =====================================================

    public void OnAIAnalysisButton()
    {
        if (gameFlowManager != null)
            gameFlowManager.TryUseEnergyForAIAnalysis();

        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    // =====================================================
    // CONFIRM PANELİNİ KAPATMA
    // =====================================================

    public void OnConfirmNo()
    {
        CloseConfirmPanel();
        UpdateEnergyUI();
    }

    private void CloseConfirmPanel()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // =====================================================
    // CONFIRM PANELİ DURUM KONTROLÜ
    // =====================================================

    public bool IsConfirmPanelOpen()
    {
        return confirmPanel != null && confirmPanel.activeInHierarchy;
    }

    // =====================================================
    // ENERJİ UI GÜNCELLEME
    // =====================================================

    public void UpdateEnergyUI()
    {
        // Referans kaybolduysa tekrar alınır.
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        int currentEnergy = gameFlowManager != null
            ? gameFlowManager.currentEnergy
            : 0;

        // Enerji ikonları güncellenir.
        if (energyIcons != null)
        {
            for (int i = 0; i < energyIcons.Length; i++)
            {
                if (energyIcons[i] != null)
                    energyIcons[i].enabled = i < currentEnergy;
            }
        }

        // Enerji kullanım butonu güncellenir.
        if (useEnergyButton != null && gameFlowManager != null)
        {
            useEnergyButton.interactable =
                gameFlowManager.CanOpenHintPanel();
        }

        // Enerji bilgi yazısı güncellenir.
        if (energyInfoText != null && gameFlowManager != null)
        {
            energyInfoText.text =
                currentEnergy + " / " + gameFlowManager.maxEnergy;
        }

        UpdateHintButtons();
    }

    // =====================================================
    // İPUCU BUTON DURUMLARINI GÜNCELLEME
    // =====================================================

    private void UpdateHintButtons()
    {
        if (gameFlowManager == null)
            return;

        bool hasEnergy = gameFlowManager.currentEnergy > 0;

        // DoctorBook butonu kontrol edilir.
        if (doctorBookButton != null)
        {
            doctorBookButton.interactable =
                hasEnergy &&
                !gameFlowManager.IsDoctorBookUnlockedForCurrentPatient();
        }

        // AI analiz butonu kontrol edilir.
        if (aiAnalysisButton != null)
        {
            aiAnalysisButton.interactable =
                hasEnergy &&
                !gameFlowManager.IsAIAnalysisUnlockedForCurrentPatient();
        }
    }
}