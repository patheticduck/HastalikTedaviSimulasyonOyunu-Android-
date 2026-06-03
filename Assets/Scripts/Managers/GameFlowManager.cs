using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class PatientAnalysisEntry
{
    public int dayNumber;
    public int patientNumberInDay;
    public int globalPatientNumber;
    public bool isCorrect;
    public string correctDiagnosisName;
    public string playerDiagnosisName;
}

public class GameFlowManager : MonoBehaviour
{
    // =========================
    // ANA MANAGER VE HASTA SİSTEMLERİ
    // =========================

    [Header("Main Managers")]
    public MainMenuManager mainMenuManager;

    [Header("Interaction Mode")]
    public PlayerInteractionModeManager interactionModeManager;

    [Header("Patient")]
    public PatientMovement patientMovement;

    // =========================
    // HASTA HAVUZLARI VE VAKA PLANLAMA
    // =========================

    [Header("Patient Pools")]
    public List<PatientCaseData> respiratoryCases = new List<PatientCaseData>();
    public List<PatientCaseData> gastroCases = new List<PatientCaseData>();
    public List<PatientCaseData> specialCases = new List<PatientCaseData>();

    [Header("Old Patient Cases - Deprecated")]
    [Tooltip("Eski sistemden kalma. Yeni sistemde respiratoryCases listesine ekle.")]
    public PatientCaseData gripCase;

    [Tooltip("Eski sistemden kalma. Yeni sistemde respiratoryCases listesine ekle.")]
    public PatientCaseData coldCase;

    private readonly List<PatientCaseData> plannedPatientCases = new List<PatientCaseData>();
    private readonly List<string> plannedPatientCaseIds = new List<string>();

    // =========================
    // UI PANEL VE SONUÇ EKRANLARI
    // =========================

    [Header("Panels")]
    public GameObject dialoguePanel;
    public GameObject endDayPanel;
    public GameObject gameOverPanel;
    public GameObject dayTransitionPanel;
    public GameObject heartsPanel;

    [Header("Patient Result Panel")]
    public GameObject patientResultPanel;
    public TextMeshProUGUI patientResultTitleText;
    public TextMeshProUGUI patientResultBodyText;
    public Button patientResultContinueButton;

    [Header("Patient Result Title Colors")]
    [SerializeField] private Color correctTitleColor = new Color32(80, 200, 120, 255);
    [SerializeField] private Color wrongTitleColor = new Color32(220, 70, 70, 255);

    [Header("End Day UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("Game Over UI")]
    public TextMeshProUGUI gameOverTitleText;
    public TextMeshProUGUI gameOverResultText;
    public Button gameOverContinueButton;

    [Header("Mobile UI")]
    public GameObject mobileButtonsPanel;

    [Header("Game Over Title Colors")]
    [SerializeField] private Color gameWinTitleColor = new Color32(90, 220, 120, 255);
    [SerializeField] private Color gameLoseTitleColor = new Color32(255, 60, 60, 255);

    [Header("Day Transition UI")]
    public TextMeshProUGUI dayTransitionText;
    public CanvasGroup dayTransitionCanvasGroup;

    [Header("Heart UI")]
    public GameObject[] hearts;

    // =========================
    // ENERJİ VE NOTEBOOK ERİŞİMİ
    // =========================

    [Header("Energy UI")]
    public GameObject energyPanel;
    public EnergyUIManager energyUIManager;
    public int maxEnergy = 5;
    public int currentEnergy = 5;

    private bool doctorBookUnlockedForCurrentPatient = false;
    private bool aiAnalysisUnlockedForCurrentPatient = false;
    private bool aiAnalysisUsedForCurrentPatient = false;

    [Header("Notebook Access")]
    [SerializeField] private bool blockNotebookInteraction = true;

    // =========================
    // OYUN VE İLERLEME VERİLERİ
    // =========================

    [Header("Game Data")]
    public int currentDay = 1;
    public int maxHearts = 5;
    public int remainingHearts = 5;

    public int totalCorrect = 0;
    public int totalWrong = 0;

    [Header("Patient Flow")]
    public int totalPatientsInRun = 15;
    public int patientsPerDay = 3;
    public int currentPatientNumber = 1;

    private bool gameEnded = false;
    private bool waitingForDayContinue = false;
    private bool waitingForPatientResultContinue = false;
    private bool pendingLoseGameOver = false;

    private bool patientTransitionInProgress = false;
    private bool diagnosisFlowInProgress = false;
    private bool diagnosisSubmittedForCurrentPatient = false;
    private bool currentPatientDiagnosisResolved = false;

    private int dayCorrect = 0;
    private int dayWrong = 0;

    private bool lastDiagnosisWasCorrect = false;
    private string lastDiagnosisResultText = "";
    private string lastCorrectDiagnosisName = "";

    private readonly List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();

    private string currentRunId = "";

    // Oyunun başlangıç UI ve sistem durumlarını hazırlar.
    private void Start()
    {
        Application.targetFrameRate = 60;


        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);

        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        currentEnergy = maxEnergy;
        ResetDoctorBookForNewPatient();

        UpdateHeartsUI();
        UpdateEnergyUI();
    }

    // =========================
    // YENİ OYUN VE RESET SİSTEMİ
    // =========================

    // Oyunu tamamen başlangıç durumuna sıfırlar.
    public void ResetGame()
    {
        currentRunId = System.Guid.NewGuid().ToString();

        currentDay = 1;
        currentPatientNumber = 1;

        remainingHearts = maxHearts;
        currentEnergy = maxEnergy;
        ResetDoctorBookForNewPatient();
        aiAnalysisUsedForCurrentPatient = false;

        totalCorrect = 0;
        totalWrong = 0;

        dayCorrect = 0;
        dayWrong = 0;

        lastDiagnosisWasCorrect = false;
        lastDiagnosisResultText = "";
        lastCorrectDiagnosisName = "";

        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;

        pendingLoseGameOver = false;
        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        gameEnded = false;

        patientAnalysisHistory.Clear();

        GeneratePlannedPatientCases();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (heartsPanel != null) heartsPanel.SetActive(true);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(true);

        if (dayTransitionCanvasGroup != null)
            dayTransitionCanvasGroup.alpha = 0f;

        UpdateHeartsUI();
        UpdateEnergyUI();
        SetNotebookBlocked(true);

        if (patientMovement != null)
            patientMovement.HidePatient();

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }
    }

    // =========================
    // HASTA HAVUZU OLUŞTURMA
    // =========================

    // Oyun başlangıcında kullanılacak hasta sıralamasını oluşturur.
    private void GeneratePlannedPatientCases()
    {
        plannedPatientCases.Clear();
        plannedPatientCaseIds.Clear();

        List<PatientCaseData> respiratoryPool = BuildCleanPool(respiratoryCases);
        List<PatientCaseData> gastroPool = BuildCleanPool(gastroCases);
        List<PatientCaseData> specialPool = BuildCleanPool(specialCases);

        AddDeprecatedCasesIfNeeded(respiratoryPool);

        AddRandomCasesFromPool(respiratoryPool, 6, "Solunum");
        AddRandomCasesFromPool(gastroPool, 6, "Mide/Gastro");
        AddRandomCasesFromPool(specialPool, 3, "Özel");

        totalPatientsInRun = plannedPatientCases.Count;

        foreach (PatientCaseData patientCase in plannedPatientCases)
            plannedPatientCaseIds.Add(GetCaseSaveId(patientCase));

    }

    // Null veya tekrar eden vakaları temizleyerek güvenli havuz oluşturur.
    private List<PatientCaseData> BuildCleanPool(List<PatientCaseData> source)
    {
        List<PatientCaseData> cleanPool = new List<PatientCaseData>();

        if (source == null)
            return cleanPool;

        foreach (PatientCaseData patientCase in source)
        {
            if (patientCase == null)
                continue;

            if (!cleanPool.Contains(patientCase))
                cleanPool.Add(patientCase);
        }

        return cleanPool;
    }

    // Eski sistemden kalan vakaları yeni havuza ekler.
    private void AddDeprecatedCasesIfNeeded(List<PatientCaseData> respiratoryPool)
    {
        if (gripCase != null && !respiratoryPool.Contains(gripCase))
            respiratoryPool.Add(gripCase);

        if (coldCase != null && !respiratoryPool.Contains(coldCase))
            respiratoryPool.Add(coldCase);
    }

    // Belirtilen havuzdan rastgele vaka seçerek planlanan listeye ekler.
    private void AddRandomCasesFromPool(List<PatientCaseData> pool, int targetCount, string poolName)
    {
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning(poolName + " hasta havuzu boş.");
            return;
        }

        if (pool.Count < targetCount)
        {
            Debug.LogWarning(poolName + " havuzunda yeterli hasta yok. İstenen: " + targetCount + ", Var olan: " + pool.Count);
        }

        List<PatientCaseData> tempPool = new List<PatientCaseData>(pool);
        int count = Mathf.Min(targetCount, tempPool.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempPool.Count);
            PatientCaseData selectedCase = tempPool[randomIndex];

            plannedPatientCases.Add(selectedCase);
            tempPool.RemoveAt(randomIndex);
        }
    }

    // Save/load sistemi için vaka ID'si oluşturur.
    private string GetCaseSaveId(PatientCaseData patientCase)
    {
        if (patientCase == null)
            return "";

        if (!string.IsNullOrEmpty(patientCase.caseId))
            return patientCase.caseId;

        return patientCase.name;
    }

    // Kayıtlı ID üzerinden ilgili hasta vakasını bulur.
    private PatientCaseData FindCaseBySaveId(string caseId)
    {
        if (string.IsNullOrEmpty(caseId))
            return null;

        List<PatientCaseData> allCases = new List<PatientCaseData>();
        allCases.AddRange(BuildCleanPool(respiratoryCases));
        allCases.AddRange(BuildCleanPool(gastroCases));
        allCases.AddRange(BuildCleanPool(specialCases));

        AddDeprecatedCasesIfNeeded(allCases);

        foreach (PatientCaseData patientCase in allCases)
        {
            if (patientCase == null)
                continue;

            if (patientCase.caseId == caseId || patientCase.name == caseId)
                return patientCase;
        }

        return null;
    }

    // Aktif hasta numarasına göre mevcut vakayı döndürür.
    private PatientCaseData GetCurrentPlannedPatientCase()
    {
        if (plannedPatientCases.Count == 0)
            GeneratePlannedPatientCases();

        int index = currentPatientNumber - 1;

        if (index < 0 || index >= plannedPatientCases.Count)
        {
            Debug.LogError("Hasta index hatası. currentPatientNumber: " + currentPatientNumber + ", planned count: " + plannedPatientCases.Count);
            return null;
        }

        return plannedPatientCases[index];
    }

    // =========================
    // GAMEPLAY VE HASTA AKIŞI
    // =========================

    // Yeni hasta akışını başlatır ve gerekli sistemleri hazırlar.
    public void StartGameplayFlow()
    {
        if (gameEnded) return;

        SetEnergyPanelVisible(true);

        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        ResetDoctorBookForNewPatient();
        aiAnalysisUsedForCurrentPatient = false;
        patientTransitionInProgress = true;
        diagnosisFlowInProgress = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);

        if (mobileButtonsPanel != null)
            mobileButtonsPanel.SetActive(true);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);

        AssignCurrentPatientCase();

        if (patientMovement != null)
            patientMovement.BeginWalk();
        else
            Debug.LogWarning("PatientMovement bağlı değil!");
    }

    // Mevcut hasta vakasını DialogueManager sistemine atar.
    private void AssignCurrentPatientCase()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager bulunamadı, vaka atanamadı.");
            return;
        }

        PatientCaseData selectedCase = GetCurrentPlannedPatientCase();

        if (selectedCase != null)
        {
            dialogueManager.currentCase = selectedCase;

            if (patientMovement != null)
            {
                patientMovement.SetPatientVisual(selectedCase.patientVisualIndex);
            }

            return;
        }

        Debug.LogWarning("Planlanan hasta bulunamadı. Vaka atanamadı.");
    }

    // =========================
    // SAVE / LOAD SİSTEMİ
    // =========================

    // Oyun akışındaki runtime verilerini save datasına dönüştürür.
    public RuntimeSaveData BuildRuntimeSaveData()
    {
        RuntimeSaveData save = new RuntimeSaveData();

        save.currentDay = currentDay;
        save.currentPatientNumber = currentPatientNumber;
        save.totalCorrect = totalCorrect;
        save.totalWrong = totalWrong;
        save.remainingHearts = remainingHearts;
        save.maxEnergy = maxEnergy;
        save.currentEnergy = currentEnergy;

        if (!diagnosisSubmittedForCurrentPatient && aiAnalysisUsedForCurrentPatient)
        {
            save.currentEnergy = Mathf.Clamp(save.currentEnergy + 1, 0, maxEnergy);
        }

        save.doctorBookUnlockedForCurrentPatient = doctorBookUnlockedForCurrentPatient;
        save.dayCorrect = dayCorrect;
        save.dayWrong = dayWrong;
        save.diagnosisSubmittedForCurrentPatient = diagnosisSubmittedForCurrentPatient;
        save.patientResultPanelPendingAfterLoad = diagnosisSubmittedForCurrentPatient;
        save.lastDiagnosisWasCorrect = lastDiagnosisWasCorrect;
        save.lastDiagnosisResultText = lastDiagnosisResultText;
        save.lastCorrectDiagnosisName = lastCorrectDiagnosisName;
        save.pendingLoseGameOver = pendingLoseGameOver;
        save.hasSave = true;
        save.runId = currentRunId;

        if (DoctorProfileManager.Instance != null)
        {
            save.selectedDoctorSpecialization = DoctorProfileManager.Instance.GetSelectedSpecialization();
        }
        else
        {
            save.selectedDoctorSpecialization = DoctorSpecialization.None;
        }

        save.patientAnalysisHistory = ClonePatientAnalysisHistory();
        save.plannedPatientCaseIds = new List<string>(plannedPatientCaseIds);

        return save;
    }

    // Save datasındaki bilgileri oyun akışına geri yükler.
    public void ApplyRuntimeSaveData(RuntimeSaveData save)
    {
        if (save == null)
            return;

        if (DoctorProfileManager.Instance != null)
            DoctorProfileManager.Instance.SelectSpecialization(save.selectedDoctorSpecialization);

        currentRunId = save.runId;

        if (string.IsNullOrEmpty(currentRunId))
            currentRunId = System.Guid.NewGuid().ToString();

        currentDay = save.currentDay;
        currentPatientNumber = save.currentPatientNumber;
        totalCorrect = save.totalCorrect;
        totalWrong = save.totalWrong;
        remainingHearts = save.remainingHearts;

        maxEnergy = save.maxEnergy > 0 ? save.maxEnergy : maxEnergy;
        currentEnergy = Mathf.Clamp(save.currentEnergy, 0, maxEnergy);
        doctorBookUnlockedForCurrentPatient = save.doctorBookUnlockedForCurrentPatient;

        if (!save.diagnosisSubmittedForCurrentPatient &&
            save.doctorBookUnlockedForCurrentPatient)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + 1, 0, maxEnergy);
            doctorBookUnlockedForCurrentPatient = false;
        }

        aiAnalysisUnlockedForCurrentPatient = false;
        aiAnalysisUsedForCurrentPatient = false;

        dayCorrect = save.dayCorrect;
        dayWrong = save.dayWrong;

        diagnosisSubmittedForCurrentPatient = save.diagnosisSubmittedForCurrentPatient;

        lastDiagnosisWasCorrect = save.lastDiagnosisWasCorrect;
        lastDiagnosisResultText = save.lastDiagnosisResultText;
        lastCorrectDiagnosisName = save.lastCorrectDiagnosisName;
        pendingLoseGameOver = save.pendingLoseGameOver;

        currentPatientDiagnosisResolved = save.diagnosisSubmittedForCurrentPatient;

        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;
        gameEnded = false;

        RestorePlannedPatientCasesFromSave(save.plannedPatientCaseIds);

        patientAnalysisHistory.Clear();

        if (save.patientAnalysisHistory != null)
        {
            foreach (PatientAnalysisEntry entry in save.patientAnalysisHistory)
            {
                if (entry == null)
                    continue;

                patientAnalysisHistory.Add(new PatientAnalysisEntry
                {
                    dayNumber = entry.dayNumber,
                    patientNumberInDay = entry.patientNumberInDay,
                    globalPatientNumber = entry.globalPatientNumber,
                    isCorrect = entry.isCorrect,
                    correctDiagnosisName = entry.correctDiagnosisName,
                    playerDiagnosisName = entry.playerDiagnosisName
                });
            }
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        SetEnergyPanelVisible(true);

        if (patientMovement != null)
            patientMovement.HidePatient();

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);
        UpdateHeartsUI();
        UpdateEnergyUI();
    }
    // =========================
    // SAVE / LOAD DEVAM AKIŞI
    // =========================

    // Save dosyasındaki hasta sırasını geri yükler.
    private void RestorePlannedPatientCasesFromSave(List<string> savedCaseIds)
    {
        plannedPatientCases.Clear();
        plannedPatientCaseIds.Clear();

        if (savedCaseIds == null || savedCaseIds.Count == 0)
        {
            Debug.LogWarning("Save içinde hasta sırası bulunamadı. Yeni sıra oluşturuluyor.");
            GeneratePlannedPatientCases();
            return;
        }

        foreach (string caseId in savedCaseIds)
        {
            PatientCaseData patientCase = FindCaseBySaveId(caseId);

            if (patientCase == null)
            {
                Debug.LogWarning("Save içindeki hasta bulunamadı: " + caseId);
                continue;
            }

            plannedPatientCases.Add(patientCase);
            plannedPatientCaseIds.Add(caseId);
        }

        totalPatientsInRun = plannedPatientCases.Count;

        if (plannedPatientCases.Count == 0)
            GeneratePlannedPatientCases();
    }

    // Save yüklendikten sonra gameplay akışını doğru noktadan devam ettirir.
    public void ContinueAfterLoadedSave()
    {
        SetEnergyPanelVisible(true);

        if (diagnosisSubmittedForCurrentPatient)
        {
            SetNotebookBlocked(true);
            patientTransitionInProgress = false;
            diagnosisFlowInProgress = false;

            waitingForPatientResultContinue = true;
            ShowPatientResultPanel();

            return;
        }

        if (remainingHearts <= 0)
        {
            ShowGameOverPanel(false);
            return;
        }

        StartGameplayFlow();
    }

    // Patient analysis geçmişinin güvenli bir kopyasını oluşturur.
    private List<PatientAnalysisEntry> ClonePatientAnalysisHistory()
    {
        List<PatientAnalysisEntry> clone = new List<PatientAnalysisEntry>();

        foreach (PatientAnalysisEntry entry in patientAnalysisHistory)
        {
            if (entry == null)
                continue;

            clone.Add(new PatientAnalysisEntry
            {
                dayNumber = entry.dayNumber,
                patientNumberInDay = entry.patientNumberInDay,
                globalPatientNumber = entry.globalPatientNumber,
                isCorrect = entry.isCorrect,
                correctDiagnosisName = entry.correctDiagnosisName,
                playerDiagnosisName = entry.playerDiagnosisName
            });
        }

        return clone;
    }

    // =========================
    // DOĞRU / YANLIŞ TEŞHİS SİSTEMİ
    // =========================

    // Doğru teşhis sonrası skor verilerini günceller.
    public void AddCorrectAnswer()
    {
        if (gameEnded) return;

        totalCorrect++;
        dayCorrect++;
    }

    // Yanlış teşhis sonrası kalp ve skor sistemini günceller.
    public void AddWrongAnswer()
    {
        if (gameEnded) return;

        totalWrong++;
        dayWrong++;
        remainingHearts--;

        if (remainingHearts < 0)
            remainingHearts = 0;

        UpdateHeartsUI();

        if (remainingHearts <= 0)
            pendingLoseGameOver = true;
    }

    // =========================
    // GÜN GEÇİŞ VE SONUÇ PANELLERİ
    // =========================

    // Gün geçiş panelini belirtilen mesaj ile açar.
    public void ShowDayTransitionPanel(string message)
    {
        SetNotebookBlocked(true);

        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(true);

        if (dayTransitionText != null)
            dayTransitionText.text = message;
    }

    // Gün geçiş panelini kapatır.
    public void CloseDayTransitionPanel()
    {
        if (dayTransitionPanel != null)
            dayTransitionPanel.SetActive(false);

        SetNotebookBlocked(true);
    }

    // Gün sonu değerlendirme panelini oluşturur.
    private void ShowEndOfDayPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (endDayPanel != null)
            endDayPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "GÜN " + currentDay + " TAMAMLANDI";

        if (resultText != null)
        {
            resultText.text =
                currentDay + ". Gün Değerlendirmesi\n\n" +
                "Bu Gün Doğru: " + dayCorrect + "\n" +
                "Bu Gün Yanlış: " + dayWrong + "\n\n" +
                "Toplam Doğru: " + totalCorrect + "\n" +
                "Toplam Yanlış: " + totalWrong + "\n\n" +
                "Hasta: " + currentPatientNumber + " / " + totalPatientsInRun;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Devam Et";
        }
    }

    // Gün sonu panelini kapatır ve gameplay moduna döner.
    private void CloseEndOfDayPanel()
    {
        if (endDayPanel != null)
            endDayPanel.SetActive(false);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        SetNotebookBlocked(true);
    }

    // =========================
    // GAME OVER VE RAPOR SİSTEMİ
    // =========================

    // Oyun sonu panelini oluşturur ve sonucu ekrana yansıtır.
    private void ShowGameOverPanel(bool isVictory)
    {
        gameEnded = true;
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        SetNotebookBlocked(true);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.EnterUIMode();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Kazanma / kaybetme ses efektlerini oynatır.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllMusic();

            if (isVictory)
                AudioManager.Instance.PlayWin();
            else
                AudioManager.Instance.PlayLose();
        }

        SetEnergyPanelVisible(false);

        if (gameOverTitleText != null)
        {
            if (isVictory)
            {
                gameOverTitleText.text = "OYUN TAMAMLANDI";
                gameOverTitleText.color = gameWinTitleColor;
            }
            else
            {
                gameOverTitleText.text = "OYUN BİTTİ";
                gameOverTitleText.color = gameLoseTitleColor;
            }
        }

        if (gameOverResultText != null)
            gameOverResultText.text = BuildDetailedGameOverText(isVictory);

        if (gameOverContinueButton != null)
        {
            gameOverContinueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = gameOverContinueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Ana Menüye Dön";
        }
    }

    // Oyun sonunda detaylı analiz raporu oluşturur.
    private string BuildDetailedGameOverText(bool isVictory)
    {
        StringBuilder sb = new StringBuilder();

        if (isVictory)
            sb.AppendLine("5 günü başarıyla tamamladın.");
        else
            sb.AppendLine("3 yanlış teşhis hakkını doldurdun.");

        sb.AppendLine();
        sb.AppendLine("Toplam Doğru: " + totalCorrect);
        sb.AppendLine("Toplam Yanlış: " + totalWrong);
        sb.AppendLine();

        string doctorName = "Belirsiz";

        if (DoctorProfileManager.Instance != null)
            doctorName = DoctorProfileManager.Instance.GetSelectedDoctorDisplayName();

        sb.AppendLine("Seçilen Doktor: " + doctorName);
        sb.AppendLine();

        if (patientAnalysisHistory.Count == 0)
        {
            sb.AppendLine("Henüz analiz kaydı bulunamadı.");
            return sb.ToString();
        }

        int maxPlayedDay = patientAnalysisHistory.Max(x => x.dayNumber);

        // Gün gün hasta teşhis sonuçlarını raporlar.
        for (int day = 1; day <= maxPlayedDay; day++)
        {
            List<PatientAnalysisEntry> dayEntries = patientAnalysisHistory
                .Where(x => x.dayNumber == day)
                .OrderBy(x => x.patientNumberInDay)
                .ToList();

            if (dayEntries.Count == 0)
                continue;

            int dailyCorrectCount = dayEntries.Count(x => x.isCorrect);
            int dailyWrongCount = dayEntries.Count(x => !x.isCorrect);

            sb.AppendLine(day + ". Gün");

            foreach (PatientAnalysisEntry entry in dayEntries)
            {
                if (entry.isCorrect)
                {
                    sb.AppendLine(entry.patientNumberInDay + ". hasta: Doğru");
                    sb.AppendLine("Senin Teşhisin: " + entry.playerDiagnosisName);
                }
                else
                {
                    sb.AppendLine(entry.patientNumberInDay + ". hasta: Yanlış");
                    sb.AppendLine("Senin Teşhisin: " + entry.playerDiagnosisName);
                    sb.AppendLine("Doğrusu: " + entry.correctDiagnosisName);
                }

                sb.AppendLine();
            }

            sb.AppendLine("Gün " + day + " Doğru: " + dailyCorrectCount);
            sb.AppendLine("Gün " + day + " Yanlış: " + dailyWrongCount);

            if (day < maxPlayedDay)
                sb.AppendLine();
        }

        return sb.ToString();
    }

    // =========================
    // SESSION HISTORY SİSTEMİ
    // =========================

    // Oyun oturum geçmişi için kayıt verisi oluşturur.
    public SessionHistoryData BuildSessionHistoryData()
    {
        SessionHistoryData session = new SessionHistoryData();

        System.DateTime now = System.DateTime.Now;
        session.completedDateText = now.ToString("dd.MM.yyyy HH:mm");
        session.completedAtTicks = now.Ticks;
        session.relativeTimeText = "";

        if (DoctorProfileManager.Instance != null)
            session.selectedDoctorName =
            DoctorProfileManager.Instance.GetSelectedDoctorDisplayName();
        else
            session.selectedDoctorName = "Belirsiz";

        if (patientAnalysisHistory == null || patientAnalysisHistory.Count == 0)
            return session;

        int maxPlayedDay = patientAnalysisHistory.Max(x => x.dayNumber);

        // Günlük hasta analiz kayıtlarını session datasına ekler.
        for (int day = 1; day <= maxPlayedDay; day++)
        {
            List<PatientAnalysisEntry> dayEntries = patientAnalysisHistory
                .Where(x => x.dayNumber == day)
                .OrderBy(x => x.patientNumberInDay)
                .ToList();

            if (dayEntries.Count == 0)
                continue;

            DayHistoryData dayData = new DayHistoryData();
            dayData.dayNumber = day;

            foreach (PatientAnalysisEntry entry in dayEntries)
            {
                PatientHistoryData patientData = new PatientHistoryData();
                patientData.patientLabel = "Hasta " + entry.patientNumberInDay;
                patientData.playerAnswer = entry.playerDiagnosisName;
                patientData.isCorrect = entry.isCorrect;
                patientData.correctAnswer = entry.correctDiagnosisName;

                dayData.patients.Add(patientData);
            }

            session.days.Add(dayData);
        }

        return session;
    }

    // =========================
    // DEVAM BUTONU VE GÜN GEÇİŞ AKIŞI
    // =========================

    // Gün sonu veya oyun sonu devam butonlarını yönetir.
    public void OnContinueButtonPressed()
    {
        if (gameEnded)
        {
            ReturnToMainMenuFromEndPanel();
            return;
        }

        if (waitingForDayContinue)
        {
            waitingForDayContinue = false;
            StartCoroutine(PlayDayTransitionAndStartNextDay());
        }
    }

    // Hasta sonuç panelindeki devam akışını yönetir.
    public void OnPatientResultContinuePressed()
    {
        if (gameEnded)
            return;

        if (!waitingForPatientResultContinue)
            return;

        waitingForPatientResultContinue = false;
        diagnosisSubmittedForCurrentPatient = false;
        currentPatientDiagnosisResolved = false;
        diagnosisFlowInProgress = false;

        HidePatientResultPanel();

        if (pendingLoseGameOver || remainingHearts <= 0)
        {
            pendingLoseGameOver = false;
            ShowGameOverPanel(false);
            return;
        }

        if (currentPatientNumber >= totalPatientsInRun)
        {
            ShowGameOverPanel(true);
            return;
        }

        bool isEndOfDay = (currentPatientNumber % patientsPerDay == 0);

        if (isEndOfDay)
        {
            waitingForDayContinue = true;
            ShowEndOfDayPanel();
        }
        else
        {
            currentPatientNumber++;
            StartCoroutine(StartNextPatientAfterDelay(0.5f));
        }
    }

    // Gün geçiş animasyonunu oynatır ve yeni günü başlatır.
    private IEnumerator PlayDayTransitionAndStartNextDay()
    {
        CloseEndOfDayPanel();

        currentDay++;
        currentPatientNumber++;

        ShowDayTransitionPanel(currentDay + ". GÜN");

        if (dayTransitionCanvasGroup == null)
        {
            Debug.LogWarning("DayTransitionCanvasGroup bağlı değil.");

            yield return new WaitForSeconds(1.5f);

            CloseDayTransitionPanel();

            dayCorrect = 0;
            dayWrong = 0;

            if (!gameEnded)
                StartGameplayFlow();

            yield break;
        }

        dayTransitionCanvasGroup.alpha = 0f;

        float fadeInDuration = 0.8f;
        float holdDuration = 1.0f;
        float fadeOutDuration = 0.8f;

        float timer = 0f;

        // Fade in animasyonu.
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        // Fade out animasyonu.
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            dayTransitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }

        dayTransitionCanvasGroup.alpha = 0f;

        CloseDayTransitionPanel();

        dayCorrect = 0;
        dayWrong = 0;

        if (!gameEnded)
            StartGameplayFlow();
    }

    // =========================
    // HASTA SONUÇ PANELİ
    // =========================

    // Hastaya ait teşhis sonucunu oyuncuya gösterir.
    private void ShowPatientResultPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        if (patientResultPanel != null)
            patientResultPanel.SetActive(true);

        if (patientResultTitleText != null)
        {
            if (lastDiagnosisWasCorrect)
            {
                patientResultTitleText.text = "Doğru Teşhis";
                patientResultTitleText.color = correctTitleColor;
            }
            else
            {
                patientResultTitleText.text = "Yanlış Teşhis";
                patientResultTitleText.color = wrongTitleColor;
            }
        }

        if (patientResultBodyText != null)
        {
            if (lastDiagnosisWasCorrect)
            {
                patientResultBodyText.text = "Kalpler Korundu";
            }
            else
            {
                patientResultBodyText.text =
                    "Doğrusu: " + lastCorrectDiagnosisName + "\n" +
                    "-1 Kalp Gitti";
            }

            if (!string.IsNullOrEmpty(lastDiagnosisResultText))
                patientResultBodyText.text += "\n\n" + lastDiagnosisResultText;
        }

        if (patientResultContinueButton != null)
        {
            patientResultContinueButton.gameObject.SetActive(true);

            TextMeshProUGUI btnText = patientResultContinueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "Devam Et";
        }
    }

    // Hasta sonuç panelini kapatır.
    private void HidePatientResultPanel()
    {
        if (patientResultPanel != null)
            patientResultPanel.SetActive(false);

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        SetNotebookBlocked(true);
    }
    // =========================
    // ANA MENÜ VE GAMEPLAY DURDURMA
    // =========================

    // Oyun sonu panellerinden ana menüye dönüş yapar.
    public void ReturnToMainMenuFromEndPanel()
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;

        if (interactionModeManager != null)
            interactionModeManager.ExitUIMode();

        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (mainMenuManager != null)
            mainMenuManager.ReturnToMenu(totalCorrect, totalWrong);
    }

    // Gameplay sırasında çalışan tüm akışları zorla durdurur.
    public void ForceStopAllGameplay()
    {
        patientTransitionInProgress = false;
        diagnosisFlowInProgress = false;
        waitingForDayContinue = false;
        waitingForPatientResultContinue = false;
        pendingLoseGameOver = false;

        if (patientMovement != null)
            patientMovement.StopMovementImmediately();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (patientResultPanel != null) patientResultPanel.SetActive(false);
        if (endDayPanel != null) endDayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        SetEnergyPanelVisible(false);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }

        SetNotebookBlocked(true);
    }

    // =========================
    // ENERJİ VE HINT SİSTEMİ
    // =========================

    // DoctorBook kullanımı için enerji harcar.
    public bool TryUseEnergyForDoctorBook()
    {
        if (gameEnded)
        {
            return false;
        }

        if (doctorBookUnlockedForCurrentPatient)
        {
            return false;
        }

        if (currentEnergy <= 0)
        {
            return false;
        }

        currentEnergy--;
        doctorBookUnlockedForCurrentPatient = true;
        UpdateEnergyUI();

        return true;
    }

    // Mevcut hastada DoctorBook kullanım durumunu döndürür.
    public bool IsDoctorBookUnlockedForCurrentPatient()
    {
        return doctorBookUnlockedForCurrentPatient;
    }

    // Yeni hasta için DoctorBook ve AI erişimlerini sıfırlar.
    public void ResetDoctorBookForNewPatient()
    {
        doctorBookUnlockedForCurrentPatient = false;
        aiAnalysisUnlockedForCurrentPatient = false;
        aiAnalysisUsedForCurrentPatient = false;
        UpdateEnergyUI();
    }

    // DoctorBook açılabilir mi kontrol eder.
    public bool CanUseEnergyForDoctorBook()
    {
        if (gameEnded)
            return false;

        if (doctorBookUnlockedForCurrentPatient)
            return false;

        return currentEnergy > 0;
    }

    // Hint panelinin açılabilir olup olmadığını kontrol eder.
    public bool CanOpenHintPanel()
    {
        if (gameEnded)
            return false;

        if (patientTransitionInProgress)
            return false;

        if (diagnosisFlowInProgress)
            return false;

        if (waitingForPatientResultContinue)
            return false;

        if (waitingForDayContinue)
            return false;

        if (currentPatientDiagnosisResolved)
            return false;

        if (patientResultPanel != null && patientResultPanel.activeSelf)
            return false;

        if (endDayPanel != null && endDayPanel.activeSelf)
            return false;

        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return false;

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
            return false;

        if (doctorBookUnlockedForCurrentPatient && aiAnalysisUnlockedForCurrentPatient)
            return false;

        return currentEnergy > 0;
    }

    // AI analiz panelinin açılabilir olup olmadığını kontrol eder.
    public bool CanOpenAIAnalysisPanel()
    {
        if (gameEnded)
            return false;

        if (!aiAnalysisUnlockedForCurrentPatient)
            return false;

        if (patientTransitionInProgress)
            return false;

        if (diagnosisFlowInProgress)
            return false;

        if (waitingForPatientResultContinue)
            return false;

        if (waitingForDayContinue)
            return false;

        if (currentPatientDiagnosisResolved)
            return false;

        if (patientResultPanel != null && patientResultPanel.activeSelf)
            return false;

        if (endDayPanel != null && endDayPanel.activeSelf)
            return false;

        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return false;

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
            return false;

        return true;
    }

    // AI analiz sistemi için enerji kullanımını yönetir.
    public bool TryUseEnergyForAIAnalysis()
    {
        if (gameEnded)
            return false;

        if (aiAnalysisUsedForCurrentPatient)
        {
            aiAnalysisUnlockedForCurrentPatient = true;
            return true;
        }

        if (currentEnergy <= 0)
        {
            return false;
        }

        currentEnergy--;
        aiAnalysisUsedForCurrentPatient = true;
        aiAnalysisUnlockedForCurrentPatient = true;

        UpdateEnergyUI();

        return true;
    }

    // Mevcut hastada AI analiz erişim durumunu döndürür.
    public bool IsAIAnalysisUnlockedForCurrentPatient()
    {
        return aiAnalysisUnlockedForCurrentPatient;
    }

    // =========================
    // UI GÜNCELLEME SİSTEMİ
    // =========================

    // Enerji panelinin görünürlüğünü ayarlar.
    private void SetEnergyPanelVisible(bool visible)
    {
        if (energyPanel != null)
            energyPanel.SetActive(visible);

        if (visible)
            UpdateEnergyUI();
    }

    // Enerji UI sistemini günceller.
    private void UpdateEnergyUI()
    {
        if (energyUIManager == null)
            energyUIManager = FindFirstObjectByType<EnergyUIManager>();

        if (energyUIManager != null)
            energyUIManager.UpdateEnergyUI();
    }

    // Kalp UI görünümünü mevcut can sayısına göre günceller.
    private void UpdateHeartsUI()
    {
        if (hearts == null || hearts.Length == 0)
            return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].SetActive(i < remainingHearts);
        }
    }

    // Save sonrası UI verilerini tekrar senkronize eder.
    public void RefreshGameUIAfterLoad()
    {
        UpdateHeartsUI();
        UpdateEnergyUI();
    }

    // =========================
    // HASTA GİRİŞ / ÇIKIŞ AKIŞI
    // =========================

    // Hasta sahneye ulaştığında diyalog akışını başlatır.
    public void OnPatientArrived()
    {
        patientTransitionInProgress = false;
        SetNotebookBlocked(true);
        SetEnergyPanelVisible(true);

        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(true);
            interactionModeManager.OpenDialogueMode();
        }
        else if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
            dialogueManager.StartDialogue();
        else
            Debug.LogError("DialogueManager bulunamadı!");
    }

    // Hasta sahneden ayrıldığında sonuç paneline geçiş yapar.
    public void OnPatientExited()
    {
        diagnosisFlowInProgress = false;

        patientTransitionInProgress = false;
        SetNotebookBlocked(true);

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(false);
        else if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!currentPatientDiagnosisResolved)
        {
            Debug.LogWarning("Hasta çıktı ama teşhis çözülmüş görünmüyor. Akış durduruldu.");
            return;
        }

        if (gameEnded)
            return;

        waitingForPatientResultContinue = true;
        ShowPatientResultPanel();
    }

    // Belirli gecikme sonrası yeni hasta akışını başlatır.
    private IEnumerator StartNextPatientAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameEnded)
            yield break;

        StartGameplayFlow();
    }

    // =========================
    // HASTA ANALİZ VE TEŞHİS SİSTEMİ
    // =========================

    // Hasta teşhis sonucunu analiz geçmişine kaydeder.
    private void RegisterPatientAnalysis(bool isCorrect, string correctDiagnosisName, string playerDiagnosisName)
    {
        int patientNumberInDay = ((currentPatientNumber - 1) % patientsPerDay) + 1;

        PatientAnalysisEntry entry = new PatientAnalysisEntry
        {
            dayNumber = currentDay,
            patientNumberInDay = patientNumberInDay,
            globalPatientNumber = currentPatientNumber,
            isCorrect = isCorrect,
            correctDiagnosisName = string.IsNullOrEmpty(correctDiagnosisName) ? "Bilinmiyor" : correctDiagnosisName,
            playerDiagnosisName = string.IsNullOrEmpty(playerDiagnosisName) ? "Bilinmiyor" : playerDiagnosisName
        };

        patientAnalysisHistory.Add(entry);
    }

    // Basit doğru/yanlış teşhis sonucu gönderir.
    public void SubmitDiagnosis(bool isCorrect)
    {
        lastDiagnosisWasCorrect = isCorrect;
        lastDiagnosisResultText = "";
        lastCorrectDiagnosisName = "Doğru teşhis";

        RegisterPatientAnalysis(isCorrect, lastCorrectDiagnosisName, lastCorrectDiagnosisName);

        if (isCorrect)
            AddCorrectAnswer();
        else
            AddWrongAnswer();

        diagnosisSubmittedForCurrentPatient = true;
        currentPatientDiagnosisResolved = true;
        diagnosisFlowInProgress = true;
        SetNotebookBlocked(true);

        FinishDiagnosisFlow(null);
    }

    // Seçilen teşhis ID'sine göre sonucu değerlendirir.
    public void SubmitDiagnosis(string diagnosisId)
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        bool isCorrect = false;
        string diagnosisResultText = "";
        string correctDiagnosisName = "";
        string playerDiagnosisName = "";

        if (dialogueManager != null && dialogueManager.currentCase != null)
        {
            DiagnosisChoiceData selectedDiagnosis = dialogueManager.currentCase.diagnosisChoices
                .FirstOrDefault(d => d != null && d.diseaseId == diagnosisId);

            if (selectedDiagnosis != null)
            {
                isCorrect = selectedDiagnosis.isCorrectDiagnosis;
                diagnosisResultText = selectedDiagnosis.resultText;
                playerDiagnosisName = selectedDiagnosis.diagnosisName;
            }

            DiagnosisChoiceData correctDiagnosis = dialogueManager.currentCase.diagnosisChoices
                .FirstOrDefault(d => d != null && d.isCorrectDiagnosis);

            if (correctDiagnosis != null)
                correctDiagnosisName = correctDiagnosis.diagnosisName;
        }
        else
        {
            Debug.LogWarning("DialogueManager veya currentCase bulunamadı. Teşhis doğruluğu kontrol edilemedi.");
        }

        lastDiagnosisWasCorrect = isCorrect;
        lastDiagnosisResultText = diagnosisResultText;
        lastCorrectDiagnosisName = string.IsNullOrEmpty(correctDiagnosisName) ? "Bilinmiyor" : correctDiagnosisName;

        RegisterPatientAnalysis(isCorrect, lastCorrectDiagnosisName, playerDiagnosisName);

        if (isCorrect)
        {
            AddCorrectAnswer();
        }
        else
        {
            AddWrongAnswer();
        }

        diagnosisSubmittedForCurrentPatient = true;
        currentPatientDiagnosisResolved = true;
        diagnosisFlowInProgress = true;
        SetNotebookBlocked(true);

        FinishDiagnosisFlow(diagnosisResultText);
    }

    // Teşhis sonrası hasta çıkış akışını başlatır.
    private void FinishDiagnosisFlow(string diagnosisResultText)
    {
        SetNotebookBlocked(true);
        patientTransitionInProgress = true;

        if (!string.IsNullOrEmpty(diagnosisResultText))

            if (patientMovement != null)
                patientMovement.BeginExit();
    }

    // =========================
    // NOTEBOOK VE GAMEPLAY DURUM KONTROLLERİ
    // =========================

    // Notebook erişimini açıp kapatır.
    public void SetNotebookBlocked(bool blocked)
    {
        blockNotebookInteraction = blocked;
    }

    // Notebook erişim durumunu döndürür.
    public bool IsNotebookBlocked()
    {
        return blockNotebookInteraction;
    }

    // Hasta geçiş animasyonu aktif mi kontrol eder.
    public bool IsPatientTransitionInProgress()
    {
        return patientTransitionInProgress;
    }

    // Teşhis akışı aktif mi kontrol eder.
    public bool IsDiagnosisFlowInProgress()
    {
        return diagnosisFlowInProgress;
    }

    // Notebook panelinin açılabilir olup olmadığını kontrol eder.
    public bool CanOpenNotebook()
    {
        if (blockNotebookInteraction)
            return false;

        if (gameEnded)
            return false;

        if (diagnosisFlowInProgress)
            return false;

        if (patientTransitionInProgress)
            return false;

        if (dialoguePanel != null && dialoguePanel.activeSelf)
            return false;

        if (patientResultPanel != null && patientResultPanel.activeSelf)
            return false;

        if (endDayPanel != null && endDayPanel.activeSelf)
            return false;

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
            return false;

        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return false;

        return true;
    }

    // Free look moduna geçildiğinde notebook erişimini günceller.
    public void NotifyFreeLookStateEntered()
    {
        if (gameEnded)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (diagnosisFlowInProgress)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (patientTransitionInProgress)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (patientResultPanel != null && patientResultPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (endDayPanel != null && endDayPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (dayTransitionPanel != null && dayTransitionPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            SetNotebookBlocked(true);
            return;
        }

        SetNotebookBlocked(false);
    }
}