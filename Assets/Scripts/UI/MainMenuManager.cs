using UnityEngine.UI;
using UnityEngine;
using Transform = UnityEngine.Transform;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // =====================================================
    // ANA MENÜ UI REFERANSLARI
    // =====================================================

    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject historySection;

    // =====================================================
    // ANA MENÜ BUTONLARI
    // =====================================================

    [Header("Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;

    // =====================================================
    // DOKTOR SEÇİM PANELİ
    // =====================================================

    [Header("Doctor Selection")]
    public GameObject doctorSelectionPanel;
    public Button respiratoryDoctorButton;
    public Button gastroDoctorButton;
    public Button generalistDoctorButton;
    public Button doctorSelectionBackButton;

    // =====================================================
    // CONTINUE SAVE PANELİ
    // =====================================================

    [Header("Continue Save Panel")]
    public GameObject continueSavePanel;
    public Transform continueSaveContent;
    public ContinueSaveItemUI continueSaveItemPrefab;
    public TextMeshProUGUI continueSaveDetailText;
    public Button continueSelectedSaveButton;
    public Button continueSaveBackButton;

    // =====================================================
    // CONTINUE SAVE SİLME SİSTEMİ
    // =====================================================

    [Header("Continue Save Delete")]
    public Button continueSaveDeleteModeButton;
    public TextMeshProUGUI continueSaveDeleteModeButtonText;
    public Button continueSaveClearAllButton;
    public GameObject confirmClearAllContinuePanel;
    public Button confirmClearAllContinueYesButton;
    public Button confirmClearAllContinueNoButton;

    // =====================================================
    // GEÇMİŞ OYUN OTURUMLARI UI
    // =====================================================

    [Header("History UI")]
    public Transform historyContent;
    public HistoryItemUI historyItemPrefab;

    // =====================================================
    // HISTORY DELETE MODE
    // =====================================================

    [Header("History Delete Mode")]
    public Button deleteModeButton;
    public TextMeshProUGUI deleteModeButtonText;

    // =====================================================
    // TÜM HISTORY VERİLERİNİ SİLME
    // =====================================================

    [Header("Clear All History")]
    public Button clearAllButton;
    public GameObject confirmDeleteAllPanel;
    public Button confirmDeleteYesButton;
    public Button confirmDeleteNoButton;

    // =====================================================
    // OYUN PANEL REFERANSLARI
    // =====================================================

    [Header("Game Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;
    public GameObject notebookPanel;
    public GameObject doctorBookPanel;
    public GameObject energyConfirmPanel;

    // =====================================================
    // MANAGER REFERANSLARI
    // =====================================================

    [Header("Manager")]
    public GameFlowManager gameFlowManager;
    public RunHistoryManager runHistoryManager;

    // =====================================================
    // KAMERA VE PLAYER LOOK AYARLARI
    // =====================================================

    [Header("Player Look")]
    public MouseLookLimited mouseLook;
    public Transform doctorLookRoot;
    public Transform menuCameraPoint;
    public Transform gameplayCameraPoint;

    private ScrollRect historyScrollRect;
    private ScrollRect continueSaveScrollRect;

    private bool isHistoryDeleteMode = false;
    private bool isContinueSaveDeleteMode = false;

    private int selectedContinueSaveIndex = -1;
    private RuntimeSaveData selectedContinueSave = null;

    private string activeContinueSaveRunId = "";

    // =====================================================
    // BAŞLANGIÇ REFERANS BAĞLANTILARI
    // =====================================================

    private void Awake()
    {
        if (historyContent != null)
            historyScrollRect = historyContent.GetComponentInParent<ScrollRect>();

        if (continueSaveContent != null)
            continueSaveScrollRect = continueSaveContent.GetComponentInParent<ScrollRect>();
    }

    // =====================================================
    // MENÜ BAŞLANGIÇ AYARLARI VE BUTTON EVENTLERİ
    // =====================================================

    private void Start()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OpenDoctorSelectionPanel);
        }

        if (respiratoryDoctorButton != null)
        {
            respiratoryDoctorButton.onClick.RemoveAllListeners();
            respiratoryDoctorButton.onClick.AddListener(SelectRespiratoryDoctor);
        }

        if (gastroDoctorButton != null)
        {
            gastroDoctorButton.onClick.RemoveAllListeners();
            gastroDoctorButton.onClick.AddListener(SelectGastroDoctor);
        }

        if (generalistDoctorButton != null)
        {
            generalistDoctorButton.onClick.RemoveAllListeners();
            generalistDoctorButton.onClick.AddListener(SelectGeneralistDoctor);
        }

        if (doctorSelectionBackButton != null)
        {
            doctorSelectionBackButton.onClick.RemoveAllListeners();
            doctorSelectionBackButton.onClick.AddListener(CloseDoctorSelectionPanel);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OpenContinueSavePanel);
        }

        if (continueSelectedSaveButton != null)
        {
            continueSelectedSaveButton.onClick.RemoveAllListeners();
            continueSelectedSaveButton.onClick.AddListener(ContinueSelectedSave);
            continueSelectedSaveButton.interactable = false;
        }

        if (continueSaveBackButton != null)
        {
            continueSaveBackButton.onClick.RemoveAllListeners();
            continueSaveBackButton.onClick.AddListener(CloseContinueSavePanel);
        }

        if (continueSaveDeleteModeButton != null)
        {
            continueSaveDeleteModeButton.onClick.RemoveAllListeners();
            continueSaveDeleteModeButton.onClick.AddListener(ToggleContinueSaveDeleteMode);
        }

        if (continueSaveClearAllButton != null)
        {
            continueSaveClearAllButton.onClick.RemoveAllListeners();
            continueSaveClearAllButton.onClick.AddListener(OpenConfirmClearAllContinuePanel);
        }

        if (confirmClearAllContinueYesButton != null)
        {
            confirmClearAllContinueYesButton.onClick.RemoveAllListeners();
            confirmClearAllContinueYesButton.onClick.AddListener(ConfirmClearAllContinueSaves);
        }

        if (confirmClearAllContinueNoButton != null)
        {
            confirmClearAllContinueNoButton.onClick.RemoveAllListeners();
            confirmClearAllContinueNoButton.onClick.AddListener(CloseConfirmClearAllContinuePanel);
        }

        if (deleteModeButton != null)
        {
            deleteModeButton.onClick.RemoveAllListeners();
            deleteModeButton.onClick.AddListener(ToggleHistoryDeleteMode);
        }

        if (clearAllButton != null)
        {
            clearAllButton.onClick.RemoveAllListeners();
            clearAllButton.onClick.AddListener(OpenConfirmDeleteAllPanel);
        }

        if (confirmDeleteYesButton != null)
        {
            confirmDeleteYesButton.onClick.RemoveAllListeners();
            confirmDeleteYesButton.onClick.AddListener(ConfirmClearAllHistory);
        }

        if (confirmDeleteNoButton != null)
        {
            confirmDeleteNoButton.onClick.RemoveAllListeners();
            confirmDeleteNoButton.onClick.AddListener(CloseConfirmDeleteAllPanel);
        }

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        ClearContinueSaveSelection();

        UpdateDeleteModeButtonText();
        UpdateContinueSaveDeleteModeButtonText();

        ShowMainMenu();
    }

    // =====================================================
    // ANA MENÜYE DÖNÜŞ VE MENÜ RESETLEME
    // =====================================================

    public void ShowMainMenu()
    {
        Time.timeScale = 0f;

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (heartsPanel != null)
            heartsPanel.SetActive(false);

        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);

        if (energyConfirmPanel != null)
            energyConfirmPanel.SetActive(false);

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMainMenuMusic();

        MoveCameraRootTo(menuCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        bool canContinue =
            RuntimeSaveManager.Instance != null &&
            RuntimeSaveManager.Instance.HasAnyContinueSave();

        UpdateContinueButtonState(canContinue);
        UpdateHistorySection();

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    // =====================================================
    // DOKTOR SEÇİM PANELİ İŞLEMLERİ
    // =====================================================

    public void OpenDoctorSelectionPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(true);
    }

    public void CloseDoctorSelectionPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    // =====================================================
    // DOKTOR SEÇİM BUTONLARI
    // =====================================================

    public void SelectRespiratoryDoctor()
    {
        SelectDoctorAndStartGame(DoctorSpecialization.Respiratory);
    }

    public void SelectGastroDoctor()
    {
        SelectDoctorAndStartGame(DoctorSpecialization.Gastro);
    }

    public void SelectGeneralistDoctor()
    {
        SelectDoctorAndStartGame(DoctorSpecialization.Generalist);
    }

    // =====================================================
    // DOKTOR SEÇİMİ VE OYUN BAŞLATMA
    // =====================================================

    private void SelectDoctorAndStartGame(DoctorSpecialization specialization)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (DoctorProfileManager.Instance != null)
            DoctorProfileManager.Instance.SelectSpecialization(specialization);
        else
            Debug.LogWarning("DoctorProfileManager bulunamadı!");

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        StartGameFromMenu();
    }

    // =====================================================
    // YENİ OYUN BAŞLATMA
    // =====================================================

    public void StartGameFromMenu()
    {
        Time.timeScale = 1f;

        activeContinueSaveRunId = "";

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        if (heartsPanel != null)
            heartsPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();

        MoveCameraRootTo(gameplayCameraPoint);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(true);

        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearSave();

        if (gameFlowManager != null)
        {
            gameFlowManager.ResetGame();
            gameFlowManager.StartGameplayFlow();
        }
        else
        {
            Debug.LogWarning("GameFlowManager bağlı değil!");
        }
    }
    // =====================================================
    // DEVAM KAYIT PANELİ AÇMA / KAPAMA
    // =====================================================

    public void OpenContinueSavePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Kayıt bulunup bulunmadığı kontrol edilir.
        bool hasAnySave =
            RuntimeSaveManager.Instance != null &&
            RuntimeSaveManager.Instance.HasAnyContinueSave();

        if (!hasAnySave)
        {
            Debug.LogWarning("Devam edilecek kayıt bulunamadı.");
            return;
        }

        // Menü panelleri arasında geçiş yapılır.
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (continueSavePanel != null)
            continueSavePanel.SetActive(true);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        // Kayıt listesi yeniden oluşturulur.
        ClearContinueSaveSelection();
        RebuildContinueSaveUI();

        StartCoroutine(ForceContinueSaveLayoutRefreshNextFrame());
    }

    public void CloseContinueSavePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Continue paneli kapatılır.
        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // Seçim ve silme modu sıfırlanır.
        isContinueSaveDeleteMode = false;
        UpdateContinueSaveDeleteModeButtonText();
        ClearContinueSaveSelection();

        ShowMainMenu();
    }

    // =====================================================
    // DEVAM KAYIT LİSTESİNİ OLUŞTURMA
    // =====================================================

    private void RebuildContinueSaveUI()
    {
        if (continueSaveContent == null || continueSaveItemPrefab == null)
        {
            Debug.LogWarning("RebuildContinueSaveUI -> content veya prefab eksik.");
            return;
        }

        // Eski kayıt itemları temizlenir.
        for (int i = continueSaveContent.childCount - 1; i >= 0; i--)
        {
            Transform child = continueSaveContent.GetChild(i);
            if (child.GetComponent<ContinueSaveItemUI>() != null)
                Destroy(child.gameObject);
        }

        if (RuntimeSaveManager.Instance == null ||
            RuntimeSaveManager.Instance.ContinueSaveList == null)
            return;

        // Runtime kayıt listesindeki kayıtlar UI'a eklenir.
        for (int i = 0; i < RuntimeSaveManager.Instance.ContinueSaveList.Count; i++)
        {
            RuntimeSaveData save = RuntimeSaveManager.Instance.ContinueSaveList[i];
            if (save == null)
                continue;

            ContinueSaveItemUI item = Instantiate(continueSaveItemPrefab, continueSaveContent, false);
            item.gameObject.SetActive(true);
            item.Setup(save, i, SelectContinueSave, DeleteContinueSaveItemAt);
            item.SetDeleteMode(isContinueSaveDeleteMode);

            // Oluşturulan item transform değerleri sıfırlanır.
            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localScale = Vector3.one;
                itemRect.localRotation = Quaternion.identity;
            }
        }

        StartCoroutine(ForceContinueSaveLayoutRefreshNextFrame());
    }

    // =====================================================
    // DEVAM KAYDI SEÇİM İŞLEMLERİ
    // =====================================================

    private void SelectContinueSave(int index, RuntimeSaveData save)
    {
        selectedContinueSaveIndex = index;
        selectedContinueSave = save;

        // Continue butonu aktif edilir.
        if (continueSelectedSaveButton != null)
            continueSelectedSaveButton.interactable = selectedContinueSave != null;

        // Detay paneli güncellenir.
        if (continueSaveDetailText != null)
            continueSaveDetailText.text = BuildContinueSaveDetailText(save);
    }

    private void ClearContinueSaveSelection()
    {
        selectedContinueSaveIndex = -1;
        selectedContinueSave = null;

        // Continue butonu pasif hale getirilir.
        if (continueSelectedSaveButton != null)
            continueSelectedSaveButton.interactable = false;

        // Varsayılan bilgi metni gösterilir.
        if (continueSaveDetailText != null)
            continueSaveDetailText.text = "Devam etmek istediğiniz kaydı seçin.";
    }

    // =====================================================
    // DEVAM KAYDI DETAY METNİ OLUŞTURMA
    // =====================================================

    private string BuildContinueSaveDetailText(RuntimeSaveData save)
    {
        if (save == null)
            return "Kayıt bilgisi bulunamadı.";

        int patientsPerDay = 3;
        int totalPatients = 15;

        // Oyun akışındaki hasta bilgileri alınır.
        if (gameFlowManager != null)
        {
            if (gameFlowManager.patientsPerDay > 0)
                patientsPerDay = gameFlowManager.patientsPerDay;

            if (gameFlowManager.totalPatientsInRun > 0)
                totalPatients = gameFlowManager.totalPatientsInRun;
        }

        int patientInDay = ((save.currentPatientNumber - 1) % patientsPerDay) + 1;
        string selectedDoctorText = DoctorProfileManager.GetDoctorDisplayName(save.selectedDoctorSpecialization);

        return
            "Seçilen Devam Kaydı\n\n" +
            "Kayıt Tarihi: " + (string.IsNullOrEmpty(save.savedDateText) ? "Bilinmiyor" : save.savedDateText) + "\n" +
            "Seçilen Doktor: " + selectedDoctorText + "\n" +
            "Gün: " + save.currentDay + "\n" +
            "Hasta: " + patientInDay + " (" + save.currentPatientNumber + "/" + totalPatients + ")\n" +
            "Toplam Doğru: " + save.totalCorrect + "\n" +
            "Toplam Yanlış: " + save.totalWrong + "\n" +
            "Kalan Can: " + save.remainingHearts + "\n" +
            "Enerji: " + save.currentEnergy + " / " + save.maxEnergy;
    }

    // =====================================================
    // DEVAM KAYDINDAN OYUNA DEVAM ETME
    // =====================================================

    public void ContinueSelectedSave()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Seçili kayıt kontrol edilir.
        if (selectedContinueSave == null)
        {
            Debug.LogWarning("Seçili devam kaydı yok.");
            return;
        }

        activeContinueSaveRunId = selectedContinueSave.runId;

        // Aktif save runtime sisteme atanır.
        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.SetCurrentSave(selectedContinueSave);

        ContinueGameFromSelectedSave(selectedContinueSave);
    }

    private void ContinueGameFromSelectedSave(RuntimeSaveData save)
    {
        // Save verisinin geçerliliği kontrol edilir.
        if (save == null || !save.hasSave)
        {
            Debug.LogWarning("Devam edilecek kayıt geçersiz.");
            return;
        }

        Time.timeScale = 1f;

        // Menü ve geçiş panelleri kapatılır.
        if (continueSavePanel != null)
            continueSavePanel.SetActive(false);

        if (doctorSelectionPanel != null)
            doctorSelectionPanel.SetActive(false);

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        // Gameplay UI aktif edilir.
        if (heartsPanel != null)
            heartsPanel.SetActive(true);

        // Gameplay müziği başlatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();

        MoveCameraRootTo(gameplayCameraPoint);

        // Kamera kontrolü tekrar açılır.
        if (mouseLook != null)
            mouseLook.SetLookEnabled(true);

        // Save verileri oyun akışına uygulanır.
        if (gameFlowManager != null)
        {
            gameFlowManager.ResetGame();
            gameFlowManager.ApplyRuntimeSaveData(save);
            gameFlowManager.RefreshGameUIAfterLoad();

            if (save.diagnosisSubmittedForCurrentPatient)
                gameFlowManager.ContinueAfterLoadedSave();
            else
                gameFlowManager.StartGameplayFlow();
        }
        else
        {
            Debug.LogWarning("GameFlowManager bağlı değil!");
        }
    }

    public void ContinueGameFromMenu()
    {
        OpenContinueSavePanel();
    }

    // =====================================================
    // DEVAM KAYDI SİLME MODU
    // =====================================================

    private void ToggleContinueSaveDeleteMode()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Silme modu açılıp kapatılır.
        isContinueSaveDeleteMode = !isContinueSaveDeleteMode;

        UpdateContinueSaveDeleteModeButtonText();
        ApplyContinueSaveDeleteModeToItems();
    }

    private void UpdateContinueSaveDeleteModeButtonText()
    {
        if (continueSaveDeleteModeButtonText != null)
            continueSaveDeleteModeButtonText.text = isContinueSaveDeleteMode ? "İptal Et" : "Sil";
    }

    private void ApplyContinueSaveDeleteModeToItems()
    {
        if (continueSaveContent == null)
            return;

        // Tüm kayıt itemlarına silme modu uygulanır.
        foreach (Transform child in continueSaveContent)
        {
            ContinueSaveItemUI item = child.GetComponent<ContinueSaveItemUI>();
            if (item != null)
                item.SetDeleteMode(isContinueSaveDeleteMode);
        }
    }

    // =====================================================
    // TEKİL DEVAM KAYDI SİLME
    // =====================================================

    private void DeleteContinueSaveItemAt(int index)
    {
        if (RuntimeSaveManager.Instance == null)
            return;

        RuntimeSaveManager.Instance.RemoveContinueSaveAt(index);

        ClearContinueSaveSelection();

        // Hiç kayıt kalmadığında panel kapatılır.
        if (!RuntimeSaveManager.Instance.HasAnyContinueSave())
        {
            isContinueSaveDeleteMode = false;
            UpdateContinueSaveDeleteModeButtonText();

            CloseContinueSavePanel();
            return;
        }

        RebuildContinueSaveUI();
        UpdateContinueButtonState(RuntimeSaveManager.Instance.HasAnyContinueSave());
    }

    // =====================================================
    // TÜM DEVAM KAYITLARINI SİLME ONAY PANELİ
    // =====================================================

    public void OpenConfirmClearAllContinuePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Toplu silme onay paneli açılır.
        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(true);
    }

    public void CloseConfirmClearAllContinuePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Toplu silme onay paneli kapatılır.
        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);
    }

    public void ConfirmClearAllContinueSaves()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Tüm continue kayıtları temizlenir.
        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearAllContinueSaves();

        // UI seçim ve silme modu sıfırlanır.
        isContinueSaveDeleteMode = false;
        UpdateContinueSaveDeleteModeButtonText();
        ClearContinueSaveSelection();

        if (confirmClearAllContinuePanel != null)
            confirmClearAllContinuePanel.SetActive(false);

        UpdateContinueButtonState(false);

        CloseContinueSavePanel();
    }

    // =====================================================
    // MENÜYE DÖNÜŞ VE RUN TEMİZLEME
    // =====================================================

    public void ReturnToMenu(int correct, int wrong)
    {
        // Oyun geçmişi oluşturulup history sistemine eklenir.
        if (runHistoryManager != null && gameFlowManager != null)
        {
            SessionHistoryData session = gameFlowManager.BuildSessionHistoryData();

            if (session != null && session.days != null && session.days.Count > 0)
            {
                runHistoryManager.AddSession(session);
            }
            else
            {
                Debug.LogWarning("History session boş geldi, history'ye eklenmedi.");
            }
        }
        else
        {
            Debug.LogWarning("ReturnToMenu -> runHistoryManager veya gameFlowManager null.");
        }

        // Aktif continue kaydı temizlenir.
        if (!string.IsNullOrEmpty(activeContinueSaveRunId) && RuntimeSaveManager.Instance != null)
        {
            RuntimeSaveManager.Instance.RemoveContinueSaveByRunId(activeContinueSaveRunId);
            activeContinueSaveRunId = "";
        }

        // Runtime save verisi sıfırlanır.
        if (RuntimeSaveManager.Instance != null)
            RuntimeSaveManager.Instance.ClearSave();

        ShowMainMenu();
    }

    // =====================================================
    // KAMERA ROOT TAŞIMA
    // =====================================================

    private void MoveCameraRootTo(Transform targetPoint)
    {
        if (doctorLookRoot == null || targetPoint == null)
            return;

        // Kamera root pozisyonu hedef noktaya taşınır.
        doctorLookRoot.position = targetPoint.position;
        doctorLookRoot.rotation = targetPoint.rotation;
    }

    // =====================================================
    // HISTORY BÖLÜMÜ GÜNCELLEME
    // =====================================================

    private void UpdateHistorySection()
    {
        if (historySection == null)
            return;

        // History verisi olup olmadığı kontrol edilir.
        bool hasAnyHistory =
            runHistoryManager != null &&
            runHistoryManager.SessionHistoryList != null &&
            runHistoryManager.SessionHistoryList.Count > 0;

        historySection.SetActive(hasAnyHistory);

        // History yoksa silme modu sıfırlanır.
        if (!hasAnyHistory)
        {
            isHistoryDeleteMode = false;
            UpdateDeleteModeButtonText();

            if (confirmDeleteAllPanel != null)
                confirmDeleteAllPanel.SetActive(false);
        }

        // Buton görünürlükleri güncellenir.
        if (deleteModeButton != null)
            deleteModeButton.gameObject.SetActive(hasAnyHistory);

        if (clearAllButton != null)
            clearAllButton.gameObject.SetActive(hasAnyHistory);

        // History UI yeniden oluşturulur.
        if (hasAnyHistory)
            RebuildHistoryUI();
    }

    // =====================================================
    // HISTORY UI OLUŞTURMA
    // =====================================================

    private void RebuildHistoryUI()
    {
        if (historyContent == null || historyItemPrefab == null || runHistoryManager == null)
        {
            Debug.LogWarning("RebuildHistoryUI -> historyContent / historyItemPrefab / runHistoryManager eksik.");
            return;
        }

        // Eski history itemları temizlenir.
        for (int i = historyContent.childCount - 1; i >= 0; i--)
        {
            Transform child = historyContent.GetChild(i);
            if (child.GetComponent<HistoryItemUI>() != null)
                Destroy(child.gameObject);
        }

        Canvas.ForceUpdateCanvases();

        // Session geçmişleri UI listesine eklenir.
        for (int i = 0; i < runHistoryManager.SessionHistoryList.Count; i++)
        {
            SessionHistoryData session = runHistoryManager.SessionHistoryList[i];
            if (session == null)
                continue;

            HistoryItemUI item = Instantiate(historyItemPrefab, historyContent, false);
            item.gameObject.SetActive(true);

            item.SetHistoryContentRect(historyContent as RectTransform);
            item.Setup(session, i, DeleteHistoryItemAt);
            item.SetDeleteMode(isHistoryDeleteMode);

            // Transform değerleri normalize edilir.
            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localScale = Vector3.one;
                itemRect.localRotation = Quaternion.identity;
            }

        }

        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    // =====================================================
    // HISTORY SİLME MODU
    // =====================================================

    private void ToggleHistoryDeleteMode()
    {
        // Silme modu açılıp kapatılır.
        isHistoryDeleteMode = !isHistoryDeleteMode;

        UpdateDeleteModeButtonText();
        ApplyDeleteModeToHistoryItems();
    }

    private void UpdateDeleteModeButtonText()
    {
        if (deleteModeButtonText != null)
            deleteModeButtonText.text = isHistoryDeleteMode ? "İptal Et" : "Sil";
    }

    private void ApplyDeleteModeToHistoryItems()
    {
        if (historyContent == null)
            return;

        // Tüm history itemlarına silme modu uygulanır.
        foreach (Transform child in historyContent)
        {
            HistoryItemUI item = child.GetComponent<HistoryItemUI>();
            if (item != null)
                item.SetDeleteMode(isHistoryDeleteMode);
        }
    }

    // =====================================================
    // TEKİL HISTORY KAYDI SİLME
    // =====================================================

    private void DeleteHistoryItemAt(int index)
    {
        if (runHistoryManager == null)
            return;

        runHistoryManager.RemoveSessionAt(index);

        // History tamamen bittiyse silme modu sıfırlanır.
        if (runHistoryManager.SessionHistoryList.Count == 0)
        {
            isHistoryDeleteMode = false;
            UpdateDeleteModeButtonText();
        }

        UpdateHistorySection();
        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    // =====================================================
    // TÜM HISTORY KAYITLARINI SİLME ONAY PANELİ
    // =====================================================

    public void OpenConfirmDeleteAllPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(true);
    }

    public void CloseConfirmDeleteAllPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);
    }

    public void ConfirmClearAllHistory()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick();

        // Tüm history kayıtları temizlenir.
        if (runHistoryManager != null)
            runHistoryManager.ClearAllHistory();

        isHistoryDeleteMode = false;
        UpdateDeleteModeButtonText();

        if (confirmDeleteAllPanel != null)
            confirmDeleteAllPanel.SetActive(false);

        UpdateHistorySection();
        StartCoroutine(ForceHistoryLayoutRefreshNextFrame());
    }

    // =====================================================
    // HISTORY LAYOUT YENİLEME
    // =====================================================

    private IEnumerator ForceHistoryLayoutRefreshNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        // Content layout yeniden hesaplanır.
        RectTransform contentRect = historyContent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        // Viewport layout güncellenir.
        if (historyContent != null && historyContent.parent is RectTransform viewportRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);

        // History section layout güncellenir.
        if (historySection != null)
        {
            RectTransform historySectionRect = historySection.GetComponent<RectTransform>();
            if (historySectionRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(historySectionRect);
        }

        // Main menu layout güncellenir.
        if (mainMenuPanel != null)
        {
            RectTransform mainMenuRect = mainMenuPanel.GetComponent<RectTransform>();
            if (mainMenuRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(mainMenuRect);
        }

        // ScrollRect referansı alınır.
        if (historyScrollRect == null && historyContent != null)
            historyScrollRect = historyContent.GetComponentInParent<ScrollRect>();

        // Scroll pozisyonu başlangıca çekilir.
        if (historyScrollRect != null)
        {
            historyScrollRect.StopMovement();
            historyScrollRect.horizontalNormalizedPosition = 0f;
            historyScrollRect.verticalNormalizedPosition = 1f;
        }

        Canvas.ForceUpdateCanvases();
    }

    // =====================================================
    // CONTINUE SAVE LAYOUT YENİLEME
    // =====================================================

    private IEnumerator ForceContinueSaveLayoutRefreshNextFrame()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        // Content layout yeniden hesaplanır.
        RectTransform contentRect = continueSaveContent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchoredPosition = Vector2.zero;
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        // Viewport layout güncellenir.
        if (continueSaveContent != null && continueSaveContent.parent is RectTransform viewportRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(viewportRect);

        // Continue panel layout güncellenir.
        if (continueSavePanel != null)
        {
            RectTransform panelRect = continueSavePanel.GetComponent<RectTransform>();
            if (panelRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        // ScrollRect referansı alınır.
        if (continueSaveScrollRect == null && continueSaveContent != null)
            continueSaveScrollRect = continueSaveContent.GetComponentInParent<ScrollRect>();

        // Scroll pozisyonu başlangıca çekilir.
        if (continueSaveScrollRect != null)
        {
            continueSaveScrollRect.StopMovement();
            continueSaveScrollRect.horizontalNormalizedPosition = 0f;
            continueSaveScrollRect.verticalNormalizedPosition = 1f;
        }

        Canvas.ForceUpdateCanvases();
    }

    // =====================================================
    // CONTINUE BUTON DURUMU
    // =====================================================

    private void UpdateContinueButtonState(bool canContinue)
    {
        if (continueButton == null)
            return;

        // Continue butonu aktif/pasif yapılır.
        continueButton.interactable = canContinue;
    }
}