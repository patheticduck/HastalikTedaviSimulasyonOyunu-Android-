using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    // =========================
    // UI VE DİYALOG REFERANSLARI
    // =========================

    [Header("UI")]
    public GameObject dialoguePanel;
    public Transform contentRoot;
    public GameObject dialogueLinePrefab;
    public GameObject optionButtonPrefab;
    public ScrollRect scrollRect;

    [Header("Diagnosis Return")]
    [SerializeField] private string returnFromDiagnosisText = "GERİ DÖN";

    [Header("Typewriter")]
    [SerializeField] private float dialogueTypeSpeed = 0.02f;
    [SerializeField] private float optionTypeSpeed = 0.015f;
    [SerializeField] private float lineEndPause = 0.12f;

    [Header("Smart Scroll")]
    [SerializeField] private float bottomScrollThreshold = 0.08f;

    [Header("Skip Input")]
    [SerializeField] private bool allowMouseSkip = true;

    [Header("Flow")]
    public GameFlowManager gameFlowManager;

    [Header("Notebook")]
    public NotebookUIManager notebookUIManager;

    [Header("Case")]
    public PatientCaseData currentCase;

    [Header("Notification")]
    public DialogueNotificationUI notificationUI;

    private GameObject currentOptionsGroup;

    private Transform questionsContainer;
    private Transform testsContainer;
    private Transform diagnosisContainer;

    private GameObject lastConversationLine;
    private GameObject currentTurnFocusLine;

    // =========================
    // OYUNCU VERİLERİ VE NOTEBOOK SİSTEMİ
    // =========================

    private readonly HashSet<string> discoveredTags = new HashSet<string>();
    private readonly HashSet<string> usedHiddenChoices = new HashSet<string>();
    private readonly HashSet<string> notifiedQuestionTestKeys = new HashSet<string>();

    private readonly HashSet<string> discoveredSymptoms = new HashSet<string>();
    private readonly HashSet<string> performedTests = new HashSet<string>();
    private readonly HashSet<string> shownAssistantHints = new HashSet<string>();

    private readonly HashSet<string> notebookSymptoms = new HashSet<string>();
    private readonly HashSet<string> notebookTests = new HashSet<string>();
    private readonly HashSet<string> notebookNotes = new HashSet<string>();
    private readonly HashSet<string> aiFeatures = new HashSet<string>();

    // Runtime sırasında kullanılan dinamik veriler.
    private readonly Dictionary<string, string> runtimeTextValues = new Dictionary<string, string>();
    private readonly Dictionary<string, int> diseaseScores = new Dictionary<string, int>();

    // Oyuncunun ilerleme sayaçları.
    private int askedQuestionCount = 0;
    private int performedTestCount = 0;
    private int totalStepCount = 0;

    private string currentNodeId;
    private string rootNodeId;

    // =========================
    // DİYALOG DURUM KONTROLLERİ
    // =========================

    private bool isBusy = false;
    private bool isTyping = false;
    private bool skipRequested = false;

    private PlayerInteractionModeManager interactionModeManager;

    // Oyun başlangıcında diyalog panelini gizler.
    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // Gerekli manager referanslarını sahneden otomatik bulur.
    private void Start()
    {
        if (notebookUIManager == null)
            notebookUIManager = FindFirstObjectByType<NotebookUIManager>();

        if (interactionModeManager == null)
            interactionModeManager = FindFirstObjectByType<PlayerInteractionModeManager>();
    }

    // Notification UI referansını runtime sırasında çözümlemeye çalışır.
    private void ResolveNotificationUI()
    {
        if (notificationUI != null)
            return;

        notificationUI = FindFirstObjectByType<DialogueNotificationUI>();

        if (notificationUI == null)
            Debug.LogWarning("DialogueManager -> DialogueNotificationUI bulunamadı!");
    }

    // =========================
    // INPUT VE SKIP SİSTEMİ
    // =========================

    // Typewriter efekti sırasında oyuncunun skip inputlarını kontrol eder.
    private void Update()
    {
        PauseMenuController pause = FindFirstObjectByType<PauseMenuController>();
        if (pause != null && pause.IsPaused())
            return;

        if (!isTyping)
            return;

        bool spacePressed = Keyboard.current != null &&
                            Keyboard.current.spaceKey.wasPressedThisFrame;

        bool mousePressed = allowMouseSkip &&
                            Mouse.current != null &&
                            Mouse.current.leftButton.wasPressedThisFrame;

        bool touchPressed = Touchscreen.current != null &&
                            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (spacePressed || mousePressed || touchPressed)
            skipRequested = true;
    }

    // Yazı animasyonunu anında tamamlamak için kullanılır.
    public void RequestSkipTyping()
    {
        if (!isTyping)
            return;

        skipRequested = true;
    }

    // =========================
    // DİYALOG BAŞLATMA SİSTEMİ
    // =========================

    // Diyalog başlangıç isteğini kontrol eder.
    public void StartDialogue()
    {
        if (isBusy)
            return;

        StartCoroutine(StartDialogueRoutine());
    }

    // Yeni hasta diyaloğunu başlatır ve başlangıç verilerini hazırlar.
    private IEnumerator StartDialogueRoutine()
    {
        isBusy = true;

        // Gerekli referans kontrolleri.
        if (currentCase == null)
        {
            Debug.LogError("DialogueManager: currentCase atanmadı.");
            isBusy = false;
            yield break;
        }

        if (string.IsNullOrEmpty(currentCase.rootNodeId))
        {
            Debug.LogError("DialogueManager: rootNodeId boş.");
            isBusy = false;
            yield break;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueManager: dialoguePanel bağlı değil.");
            isBusy = false;
            yield break;
        }

        if (contentRoot == null)
        {
            Debug.LogError("DialogueManager: contentRoot bağlı değil.");
            isBusy = false;
            yield break;
        }

        if (dialogueLinePrefab == null)
        {
            Debug.LogError("DialogueManager: dialogueLinePrefab bağlı değil.");
            isBusy = false;
            yield break;
        }

        if (optionButtonPrefab == null)
        {
            Debug.LogError("DialogueManager: optionButtonPrefab bağlı değil.");
            isBusy = false;
            yield break;
        }

        ClearDialogueContent();

        dialoguePanel.SetActive(true);

        ResolveNotificationUI();

        if (notificationUI != null)
            notificationUI.ResetNotificationState();

        // Önceki hastaya ait runtime verilerini temizler.
        discoveredTags.Clear();
        usedHiddenChoices.Clear();
        notifiedQuestionTestKeys.Clear();
        discoveredSymptoms.Clear();
        performedTests.Clear();
        shownAssistantHints.Clear();
        diseaseScores.Clear();

        notebookSymptoms.Clear();
        notebookTests.Clear();
        notebookNotes.Clear();

        aiFeatures.Clear();

        runtimeTextValues.Clear();

        askedQuestionCount = 0;
        performedTestCount = 0;
        totalStepCount = 0;

        rootNodeId = currentCase.rootNodeId;
        currentNodeId = rootNodeId;

        // Hastaya özel dinamik veri üretir.
        GenerateRuntimeVariationData();

        // Notebook sistemini günceller.
        if (notebookUIManager != null)
        {
            notebookUIManager.SetDialogueManager(this);
            notebookUIManager.RefreshNotebook();
        }

        // Hastanın başlangıç konuşmasını gösterir.
        currentTurnFocusLine = CreateDialogueLineObject();
        yield return StartCoroutine(TypeDialogueIntoLine(
            currentTurnFocusLine,
            "HASTA",
            ResolveDynamicText(currentCase.startText),
            false
        ));

        // Varsa başlangıç asistan mesajını gösterir.
        if (!string.IsNullOrEmpty(currentCase.startAssistantText))
        {
            GameObject assistantLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(
                assistantLine,
                "ASİSTAN",
                ResolveDynamicText(currentCase.startAssistantText),
                false
            ));
        }

        // İlk node'u yükler.
        yield return StartCoroutine(ShowNodeRoutine(rootNodeId, false));

        Canvas.ForceUpdateCanvases();
        yield return StartCoroutine(ScrollToConversationFocus(true));

        isBusy = false;
    }

    // Diyalog panelini kapatır ve aktif coroutine'leri durdurur.
    public void CloseDialoguePanel()
    {
        StopAllCoroutines();
        isBusy = false;
        isTyping = false;
        skipRequested = false;

        ClearDialogueContent();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // =========================
    // CHOICE VE NODE AKIŞ SİSTEMİ
    // =========================

    // Oyuncunun seçtiği seçeneğin işlenmesini başlatır.
    public void SelectChoice(DialogueChoiceData choice)
    {
        if (isBusy)
            return;

        StartCoroutine(SelectChoiceRoutine(choice));
    }

    // Choice seçimine göre diyalog akışını yönetir.
    private IEnumerator SelectChoiceRoutine(DialogueChoiceData choice)
    {
        isBusy = true;

        // Choice verisinin geçerli olup olmadığını kontrol eder.
        if (choice == null)
        {
            Debug.LogError("SelectChoice: choice null.");
            isBusy = false;
            yield break;
        }

        totalStepCount++;

        if (choice.choiceType == ChoiceType.Question)
            askedQuestionCount++;

        if (choice.choiceType == ChoiceType.Test)
            performedTestCount++;

        RemoveCurrentOptions();

        currentTurnFocusLine = null;

        // Doktor konuşmasını gösterir.
        if (!string.IsNullOrEmpty(choice.doctorLine))
        {
            currentTurnFocusLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(
                currentTurnFocusLine,
                "DOKTOR",
                ResolveDynamicText(choice.doctorLine),
                false
            ));
        }

        // Test seçimlerinde başarı şansı sistemi uygular.
        if (ShouldRollTestChance(choice))
        {
            int finalChance = CalculateFinalTestSuccessChance(choice);
            int roll = Random.Range(1, 101);

            if (roll > finalChance)
            {
                GameObject failedLine = CreateDialogueLineObject();

                string failedText = string.IsNullOrWhiteSpace(choice.failedTestText)
                    ? "Testten net sonuç alınamadı. Klinik bulgularla devam etmelisin."
                    : ResolveDynamicText(choice.failedTestText);

                yield return StartCoroutine(TypeDialogueIntoLine(
                    failedLine,
                    "SİSTEM",
                    failedText,
                    false
                ));

                if (currentTurnFocusLine == null)
                    currentTurnFocusLine = failedLine;

                if (choice.hideAfterUse)
                {
                    string key = BuildChoiceKey(currentNodeId, choice.choiceText);
                    usedHiddenChoices.Add(key);
                }

                Canvas.ForceUpdateCanvases();
                yield return StartCoroutine(ScrollToConversationFocus(true));

                yield return StartCoroutine(ShowNodeRoutine(currentNodeId, false));

                isBusy = false;
                yield break;
            }
        }

        // Hastanın cevabını gösterir.
        if (!string.IsNullOrEmpty(choice.patientReply))
        {
            GameObject patientLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(
                patientLine,
                "HASTA",
                ResolveDynamicText(choice.patientReply),
                false
            ));

            if (currentTurnFocusLine == null)
                currentTurnFocusLine = patientLine;
        }

        // Asistan yorumunu gösterir.
        if (!string.IsNullOrEmpty(choice.assistantReply))
        {
            GameObject assistantLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(
                assistantLine,
                "ASİSTAN",
                ResolveDynamicText(choice.assistantReply),
                false
            ));
        }

        // Choice üzerinden gelen verileri kaydeder.
        if (choice.gainedTags != null)
        {
            foreach (string tag in choice.gainedTags)
            {
                if (!string.IsNullOrEmpty(tag))
                    discoveredTags.Add(tag);
            }
        }

        if (choice.discoveredSymptoms != null)
        {
            foreach (string symptom in choice.discoveredSymptoms)
            {
                if (!string.IsNullOrEmpty(symptom))
                    discoveredSymptoms.Add(symptom);
            }
        }

        if (choice.performedTests != null)
        {
            foreach (string test in choice.performedTests)
            {
                if (!string.IsNullOrEmpty(test))
                    performedTests.Add(test);
            }
        }

        // Notebook kayıtlarını günceller.
        if (choice.notebookSymptoms != null)
        {
            foreach (string item in choice.notebookSymptoms)
            {
                string resolved = ResolveDynamicText(item);
                if (!string.IsNullOrWhiteSpace(resolved))
                    notebookSymptoms.Add(resolved);
            }
        }

        if (choice.notebookTests != null)
        {
            foreach (string item in choice.notebookTests)
            {
                string resolved = ResolveDynamicText(item);
                if (!string.IsNullOrWhiteSpace(resolved))
                    notebookTests.Add(resolved);
            }
        }

        if (choice.notebookNotes != null)
        {
            foreach (string item in choice.notebookNotes)
            {
                string resolved = ResolveDynamicText(item);
                if (!string.IsNullOrWhiteSpace(resolved))
                    notebookNotes.Add(resolved);
            }
        }

        // Hastalık skor sistemini günceller.
        ApplyDiseaseEffects(choice.diseaseEffects);

        // Yapay zeka analiz sistemi için feature verilerini toplar.
        if (choice.gainedTags != null)
            aiFeatures.UnionWith(choice.gainedTags);

        if (choice.discoveredSymptoms != null)
            aiFeatures.UnionWith(choice.discoveredSymptoms);

        if (choice.performedTests != null)
            aiFeatures.UnionWith(choice.performedTests);

        if (choice.notebookSymptoms != null)
            aiFeatures.UnionWith(choice.notebookSymptoms);

        if (choice.notebookTests != null)
            aiFeatures.UnionWith(choice.notebookTests);

        if (choice.notebookNotes != null)
            aiFeatures.UnionWith(choice.notebookNotes);

        // Notebook ekranını günceller.
        if (notebookUIManager != null)
        {
            notebookUIManager.SetDialogueManager(this);
            notebookUIManager.RefreshNotebook();
        }

        // Tek kullanımlık seçimleri gizler.
        if (choice.hideAfterUse)
        {
            string key = BuildChoiceKey(currentNodeId, choice.choiceText);
            usedHiddenChoices.Add(key);
        }

        // Gerekirse asistan ipucu gösterir.
        yield return StartCoroutine(TryShowAssistantHintsRoutine());

        // Tanı panelini açar.
        if (choice.opensDiagnosisPanel)
        {
            yield return StartCoroutine(OpenDiagnosisPanelRoutine());
            isBusy = false;
            yield break;
        }

        // Ana hub node'una dönüş yapar.
        if (choice.returnToHub)
        {
            yield return StartCoroutine(ShowNodeRoutine(rootNodeId, false));
            isBusy = false;
            yield break;
        }

        // Sonraki node'a geçiş yapar.
        if (!string.IsNullOrEmpty(choice.nextNodeId))
        {
            yield return StartCoroutine(ShowNodeRoutine(choice.nextNodeId, true));
            isBusy = false;
            yield break;
        }

        yield return StartCoroutine(ShowNodeRoutine(rootNodeId, false));

        isBusy = false;
    }
    // =========================
    // TANI SİSTEMİ VE TANI PANELİ
    // =========================

    // Oyuncunun seçtiği tanıyı işleme alır.
    public void SelectDiagnosis(DiagnosisChoiceData diagnosis)
    {
        if (isBusy)
            return;

        StartCoroutine(SelectDiagnosisRoutine(diagnosis));
    }

    // Tanı seçimi sonrası gerekli UI ve oyun akışını yönetir.
    private IEnumerator SelectDiagnosisRoutine(DiagnosisChoiceData diagnosis)
    {
        isBusy = true;

        // Tanı verisinin geçerli olup olmadığını kontrol eder.
        if (diagnosis == null)
        {
            Debug.LogError("SelectDiagnosis: diagnosis null.");
            isBusy = false;
            yield break;
        }

        RemoveCurrentOptions();

        // Tanı aşamasında notebook kullanımını engeller.
        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);

        if (interactionModeManager != null)
            interactionModeManager.EnterUIMode();

        // Doktorun koyduğu tanıyı ekrana yazdırır.
        currentTurnFocusLine = CreateDialogueLineObject();
        yield return StartCoroutine(
            TypeDialogueIntoLine(
                currentTurnFocusLine,
                "DOKTOR",
                "Sizin hastalığınızın " + diagnosis.diagnosisName + " olduğunu düşünüyorum.",
                false
            )
        );

        DebugTopDiseaseScores();

        Canvas.ForceUpdateCanvases();
        yield return StartCoroutine(ScrollToConversationFocus(true));

        // Diyalog modundan çıkış yapar.
        if (interactionModeManager != null)
        {
            interactionModeManager.SetDialogueAvailable(false);
            interactionModeManager.ExitUIMode();
        }
        else if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Tanı sonucunu oyun akış sistemine gönderir.
        if (gameFlowManager != null)
        {
            gameFlowManager.SubmitDiagnosis(diagnosis.diseaseId);
        }
        else
        {
            Debug.LogError("DialogueManager: gameFlowManager bağlı değil.");
            GameObject systemLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(systemLine, "SİSTEM", "GameFlowManager bağlı değil.", false));
        }

        isBusy = false;
    }

    // Tanı seçeneklerini oluşturur ve tanı panelini açar.
    private IEnumerator OpenDiagnosisPanelRoutine()
    {
        if (currentCase == null)
        {
            Debug.LogError("OpenDiagnosisPanel: currentCase null.");
            yield break;
        }

        // Hastaya ait tanı seçeneklerini kontrol eder.
        if (currentCase.diagnosisChoices == null || currentCase.diagnosisChoices.Count == 0)
        {
            GameObject systemLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(systemLine, "SİSTEM", "Teşhis seçenekleri tanımlanmamış.", true));
            yield break;
        }

        DebugTopDiseaseScores();

        RemoveCurrentOptions();
        CreateOptionsGroup();

        CreateSectionHeaderAndContainer("Tanı", out diagnosisContainer);

        // Tüm tanı seçeneklerini oluşturur.
        foreach (DiagnosisChoiceData diagnosis in currentCase.diagnosisChoices)
        {
            if (diagnosis == null)
                continue;

            GameObject optionObj = Instantiate(optionButtonPrefab, diagnosisContainer);
            DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

            if (optionScript != null)
            {
                optionScript.SetupDiagnosisChoice(diagnosis, this);
                SetOptionButtonTextInstant(optionScript, diagnosis.diagnosisName);
            }
            else
            {
                Debug.LogError("OpenDiagnosisPanel: optionButtonPrefab içinde DialogueOptionButton yok.");
            }
        }

        CreateReturnFromDiagnosisButton();

        yield return StartCoroutine(ScrollToConversationFocus(false));
    }

    // Tanı paneline geri dönüş butonu ekler.
    private void CreateReturnFromDiagnosisButton()
    {
        if (optionButtonPrefab == null || diagnosisContainer == null)
            return;

        GameObject optionObj = Instantiate(optionButtonPrefab, diagnosisContainer);

        DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();
        if (optionScript != null)
            SetOptionButtonTextInstant(optionScript, returnFromDiagnosisText);

        Button button = optionObj.GetComponent<Button>();
        if (button == null)
            button = optionObj.GetComponentInChildren<Button>();

        if (button == null)
        {
            Debug.LogError("CreateReturnFromDiagnosisButton: optionButtonPrefab içinde Button bulunamadı.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ReturnFromDiagnosisPanel);
    }

    // =========================
    // NODE VE CHOICE AKIŞ SİSTEMİ
    // =========================

    // Tanı panelinden mevcut node'a geri dönüş işlemini başlatır.
    public void ReturnFromDiagnosisPanel()
    {
        if (isBusy)
            return;

        StartCoroutine(ReturnFromDiagnosisPanelRoutine());
    }

    // Tanı panelinden çıkıp mevcut node'a geri döner.
    private IEnumerator ReturnFromDiagnosisPanelRoutine()
    {
        isBusy = true;

        RemoveCurrentOptions();

        if (string.IsNullOrEmpty(currentNodeId))
            currentNodeId = rootNodeId;

        yield return StartCoroutine(ShowNodeRoutine(currentNodeId, false));

        isBusy = false;
    }

    // Verilen node'u yükler ve intro mesajlarını gösterir.
    private IEnumerator ShowNodeRoutine(string nodeId, bool showNodeIntro)
    {
        DialogueNodeData node = GetNodeById(nodeId);

        if (node == null)
        {
            Debug.LogError("Node bulunamadı: " + nodeId);
            yield break;
        }

        currentNodeId = nodeId;

        // Node intro ve asistan mesajlarını gösterir.
        if (showNodeIntro)
        {
            if (!string.IsNullOrEmpty(node.nodeIntroText))
            {
                GameObject systemLine = CreateDialogueLineObject();
                yield return StartCoroutine(TypeDialogueIntoLine(
                    systemLine,
                    "SİSTEM",
                    ResolveDynamicText(node.nodeIntroText),
                    false
                ));
            }

            if (!string.IsNullOrEmpty(node.nodeAssistantText))
            {
                GameObject assistantLine = CreateDialogueLineObject();
                yield return StartCoroutine(TypeDialogueIntoLine(
                    assistantLine,
                    "ASİSTAN",
                    ResolveDynamicText(node.nodeAssistantText),
                    false
                ));
            }
        }

        yield return StartCoroutine(ShowChoicesRoutine(node));
    }

    // Aktif node için uygun seçenekleri oluşturur.
    private IEnumerator ShowChoicesRoutine(DialogueNodeData node)
    {
        List<DialogueChoiceData> availableChoices = GetAvailableChoices(node);

        RemoveCurrentOptions();

        // Kullanılabilir seçenek yoksa sistem mesajı gösterir.
        if (availableChoices.Count == 0)
        {
            GameObject systemLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(systemLine, "SİSTEM", "Şu anda kullanılabilir seçenek yok.", true));
            yield break;
        }

        // Choice türlerine göre listeleme yapar.
        List<DialogueChoiceData> questionChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Question || c.choiceType == ChoiceType.General)
            .ToList();

        List<DialogueChoiceData> testChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Test)
            .ToList();

        List<DialogueChoiceData> diagnosisChoices = availableChoices
            .Where(c => c.choiceType == ChoiceType.Diagnosis)
            .ToList();

        CreateOptionsGroup();

        // Soru seçeneklerini oluşturur.
        if (questionChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Sorular", out questionsContainer);

            foreach (DialogueChoiceData choice in questionChoices)
                yield return StartCoroutine(CreateChoiceButtonRoutine(choice, questionsContainer));
        }

        // Test seçeneklerini oluşturur.
        if (testChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Testler", out testsContainer);

            foreach (DialogueChoiceData choice in testChoices)
                yield return StartCoroutine(CreateChoiceButtonRoutine(choice, testsContainer));
        }

        // Tanı seçeneklerini oluşturur.
        if (diagnosisChoices.Count > 0)
        {
            CreateSectionHeaderAndContainer("Tanı", out diagnosisContainer);

            foreach (DialogueChoiceData choice in diagnosisChoices)
                yield return StartCoroutine(CreateChoiceButtonRoutine(choice, diagnosisContainer));
        }

        yield return StartCoroutine(ScrollToConversationFocus(false));

        ResolveNotificationUI();

        // Yeni seçenek oluştuğunda bildirim sistemini çalıştırır.
        if (notificationUI != null && HasNewQuestionOrTestOptions(availableChoices, node.nodeId))
        {
            notificationUI.NotifyNewOptions();
        }
        else if (notificationUI == null)
        {
            Debug.LogWarning("DialogueManager -> notificationUI NULL!");
        }
    }

    // Tek bir choice butonu oluşturur.
    private IEnumerator CreateChoiceButtonRoutine(DialogueChoiceData choice, Transform parent)
    {
        GameObject optionObj = Instantiate(optionButtonPrefab, parent);
        DialogueOptionButton optionScript = optionObj.GetComponent<DialogueOptionButton>();

        if (optionScript != null)
        {
            optionScript.SetupChoice(choice, this);
            SetOptionButtonTextInstant(optionScript, ResolveDynamicText(choice.choiceText));
        }
        else
        {
            Debug.LogError("CreateChoiceButton: optionButtonPrefab içinde DialogueOptionButton yok.");
        }

        yield return null;
    }

    // =========================
    // CHOICE FİLTRELEME VE TAG SİSTEMİ
    // =========================

    // Yeni soru veya test seçenekleri oluşup oluşmadığını kontrol eder.
    private bool HasNewQuestionOrTestOptions(List<DialogueChoiceData> choices, string nodeId)
    {
        if (choices == null || choices.Count == 0)
            return false;

        bool hasNew = false;

        foreach (DialogueChoiceData choice in choices)
        {
            if (choice == null)
                continue;

            if (choice.choiceType != ChoiceType.Question &&
                choice.choiceType != ChoiceType.Test)
                continue;

            string key = BuildChoiceKey(nodeId, choice.choiceText);

            if (!notifiedQuestionTestKeys.Contains(key))
            {
                notifiedQuestionTestKeys.Add(key);
                hasNew = true;
            }
        }

        return hasNew;
    }

    // Choice butonunun yazısını anında günceller.
    private void SetOptionButtonTextInstant(DialogueOptionButton optionScript, string fullText)
    {
        if (optionScript == null)
            return;

        TextMeshProUGUI targetText = optionScript.buttonText;

        if (targetText == null)
            targetText = optionScript.GetComponentInChildren<TextMeshProUGUI>();

        if (targetText == null)
        {
            Debug.LogError("SetOptionButtonTextInstant: Option button içinde TextMeshProUGUI bulunamadı.");
            return;
        }

        targetText.text = fullText;
    }

    // Mevcut node için kullanılabilir seçimleri filtreler.
    private List<DialogueChoiceData> GetAvailableChoices(DialogueNodeData node)
    {
        List<DialogueChoiceData> result = new List<DialogueChoiceData>();

        if (node == null || node.choices == null)
            return result;

        foreach (DialogueChoiceData choice in node.choices)
        {
            if (choice == null)
                continue;

            // Tek kullanımlık seçimleri tekrar göstermez.
            if (choice.hideAfterUse)
            {
                string key = BuildChoiceKey(node.nodeId, choice.choiceText);
                if (usedHiddenChoices.Contains(key))
                    continue;
            }

            if (!AreRequiredTagsMet(choice.requiredTags))
                continue;

            if (HasAnyBlockedTag(choice.blockedTags))
                continue;

            result.Add(choice);
        }

        return result;
    }

    // Test seçiminin başarı şansı sistemini kullanıp kullanmayacağını kontrol eder.
    private bool ShouldRollTestChance(DialogueChoiceData choice)
    {
        if (choice == null)
            return false;

        return choice.choiceType == ChoiceType.Test && choice.useTestChance;
    }

    // Doktor uzmanlığına göre testin başarı şansını hesaplar.
    private int CalculateFinalTestSuccessChance(DialogueChoiceData choice)
    {
        if (choice == null)
            return 0;

        int finalChance = choice.baseSuccessChance;
        DoctorSpecialization selectedSpecialization = DoctorSpecialization.None;

        if (DoctorProfileManager.Instance != null)
            selectedSpecialization = DoctorProfileManager.Instance.GetSelectedSpecialization();

        if (selectedSpecialization == DoctorSpecialization.Generalist)
        {
            finalChance += choice.generalistBonus;
        }
        else if (choice.bonusSpecialization != DoctorSpecialization.None &&
                 selectedSpecialization == choice.bonusSpecialization)
        {
            finalChance += choice.specializationBonus;
        }

        return Mathf.Clamp(finalChance, 0, 100);
    }

    // Choice üzerinden gelen hastalık skor etkilerini uygular.
    private void ApplyDiseaseEffects(List<DiseaseScoreEffect> effects)
    {
        if (effects == null || effects.Count == 0)
            return;

        foreach (DiseaseScoreEffect effect in effects)
        {
            if (effect == null || string.IsNullOrEmpty(effect.diseaseId))
                continue;

            if (!diseaseScores.ContainsKey(effect.diseaseId))
                diseaseScores[effect.diseaseId] = 0;

            diseaseScores[effect.diseaseId] += effect.score;
        }
    }

    // Gerekli koşullar sağlandığında asistan ipuçlarını gösterir.
    private IEnumerator TryShowAssistantHintsRoutine()
    {
        if (currentCase == null || currentCase.assistantHints == null || currentCase.assistantHints.Count == 0)
            yield break;

        foreach (AssistantHintRuleData hint in currentCase.assistantHints)
        {
            if (hint == null)
                continue;

            if (hint.showOnlyOnce && shownAssistantHints.Contains(hint.hintId))
                continue;

            if (askedQuestionCount < hint.minAskedQuestions)
                continue;

            if (!AreRequiredTagsMet(hint.requiredTags))
                continue;

            if (HasAnyBlockedTag(hint.blockedTags))
                continue;

            GameObject assistantLine = CreateDialogueLineObject();
            yield return StartCoroutine(TypeDialogueIntoLine(
                assistantLine,
                "ASİSTAN",
                ResolveDynamicText(hint.hintText),
                false
            ));

            shownAssistantHints.Add(hint.hintId);

            if (hint.showOnlyOnce)
                break;
        }
    }

    // Gerekli tag koşullarının sağlanıp sağlanmadığını kontrol eder.
    private bool AreRequiredTagsMet(List<string> requiredTags)
    {
        if (requiredTags == null || requiredTags.Count == 0)
            return true;

        foreach (string tag in requiredTags)
        {
            if (!string.IsNullOrEmpty(tag) && !discoveredTags.Contains(tag))
                return false;
        }

        return true;
    }

    // Yasaklı taglerden herhangi birinin aktif olup olmadığını kontrol eder.
    private bool HasAnyBlockedTag(List<string> blockedTags)
    {
        if (blockedTags == null || blockedTags.Count == 0)
            return false;

        foreach (string tag in blockedTags)
        {
            if (!string.IsNullOrEmpty(tag) && discoveredTags.Contains(tag))
                return true;
        }

        return false;
    }

    // Verilen node ID'sine ait node verisini bulur.
    private DialogueNodeData GetNodeById(string nodeId)
    {
        if (currentCase == null || currentCase.allNodes == null)
            return null;

        return currentCase.allNodes.FirstOrDefault(n => n != null && n.nodeId == nodeId);
    }

    // Node ve choice kombinasyonu için benzersiz anahtar oluşturur.
    private string BuildChoiceKey(string nodeId, string choiceText)
    {
        return nodeId + "::" + choiceText;
    }

    // Hastalık skor listesini sıralı şekilde hazırlar.
    private void DebugTopDiseaseScores()
    {
        if (diseaseScores == null || diseaseScores.Count == 0)
        {
            return;
        }

        var sortedScores = diseaseScores
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();
    }
    // =========================
    // RUNTIME VARYASYON VE DİNAMİK METİN SİSTEMİ
    // =========================

    // Hastaya özel runtime varyasyon verilerini oluşturur.
    private void GenerateRuntimeVariationData()
    {
        runtimeTextValues.Clear();

        if (currentCase == null || currentCase.variationProfile == null)
            return;

        CaseVariationProfile profile = currentCase.variationProfile;

        // Sayısal değişkenler için rastgele değer üretir.
        if (profile.floatVariables != null)
        {
            foreach (FloatVariableRange variable in profile.floatVariables)
            {
                if (variable == null || string.IsNullOrWhiteSpace(variable.key))
                    continue;

                float min = Mathf.Min(variable.minValue, variable.maxValue);
                float max = Mathf.Max(variable.minValue, variable.maxValue);

                float value = Random.Range(min, max);

                int decimals = Mathf.Clamp(variable.decimalPlaces, 0, 3);
                float roundedValue = (float)System.Math.Round(value, decimals);

                string format = "F" + decimals;
                runtimeTextValues[variable.key] = roundedValue.ToString(format);
            }
        }

        // String değişkenler için rastgele seçenek belirler.
        if (profile.stringVariables != null)
        {
            foreach (StringVariableOptions variable in profile.stringVariables)
            {
                if (variable == null || string.IsNullOrWhiteSpace(variable.key))
                    continue;

                if (variable.options == null || variable.options.Count == 0)
                    continue;

                List<string> validOptions = variable.options
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (validOptions.Count == 0)
                    continue;

                int index = Random.Range(0, validOptions.Count);
                runtimeTextValues[variable.key] = validOptions[index];
            }
        }
    }

    // Dinamik placeholder içeren metinleri runtime verileriyle değiştirir.
    private string ResolveDynamicText(string rawText)
    {
        if (string.IsNullOrEmpty(rawText))
            return rawText;

        return Regex.Replace(rawText, @"\{([a-zA-Z0-9_]+)\}", match =>
        {
            string key = match.Groups[1].Value;

            if (runtimeTextValues.TryGetValue(key, out string value))
                return value;

            return match.Value;
        });
    }

    // =========================
    // DIŞ SİSTEM VERİ ERİŞİMLERİ
    // =========================

    // Runtime sırasında oluşturulan tüm varyasyon verilerini döndürür.
    public Dictionary<string, string> GetRuntimeVariationValues()
    {
        return new Dictionary<string, string>(runtimeTextValues);
    }

    // Belirli bir runtime değişken değerini döndürür.
    public string GetRuntimeValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "";

        if (runtimeTextValues.TryGetValue(key, out string value))
            return value;

        return "";
    }

    // Belirli bir hastalığın mevcut skorunu döndürür.
    public int GetDiseaseScore(string diseaseId)
    {
        if (string.IsNullOrEmpty(diseaseId))
            return 0;

        if (diseaseScores.ContainsKey(diseaseId))
            return diseaseScores[diseaseId];

        return 0;
    }

    // Tüm hastalık skorlarını dış sistemlere kopya olarak verir.
    public Dictionary<string, int> GetAllDiseaseScores()
    {
        return new Dictionary<string, int>(diseaseScores);
    }

    // Oyuncunun keşfettiği tag verilerini döndürür.
    public HashSet<string> GetDiscoveredTags()
    {
        return new HashSet<string>(discoveredTags);
    }

    // Oyuncunun keşfettiği semptom verilerini döndürür.
    public HashSet<string> GetDiscoveredSymptoms()
    {
        return new HashSet<string>(discoveredSymptoms);
    }

    // Oyuncunun yaptığı testleri döndürür.
    public HashSet<string> GetPerformedTests()
    {
        return new HashSet<string>(performedTests);
    }

    // Yapay zeka analiz sistemi için gerekli tüm feature verilerini birleştirir.
    public List<string> GetAIFeatures()
    {
        HashSet<string> mergedFeatures = new HashSet<string>();

        mergedFeatures.UnionWith(aiFeatures);
        mergedFeatures.UnionWith(discoveredTags);
        mergedFeatures.UnionWith(discoveredSymptoms);
        mergedFeatures.UnionWith(performedTests);

        List<string> cleanFeatures = mergedFeatures
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => ResolveDynamicText(x).Trim())
            .Distinct()
            .ToList();

        return cleanFeatures;
    }

    // Notebook ekranındaki semptom kayıtlarını döndürür.
    public HashSet<string> GetNotebookSymptoms()
    {
        return new HashSet<string>(notebookSymptoms);
    }

    // Notebook ekranındaki test kayıtlarını döndürür.
    public HashSet<string> GetNotebookTests()
    {
        return new HashSet<string>(notebookTests);
    }

    // Notebook ekranındaki not kayıtlarını döndürür.
    public HashSet<string> GetNotebookNotes()
    {
        return new HashSet<string>(notebookNotes);
    }

    // Oyuncunun istatistik verilerini döndürür.
    public int GetAskedQuestionCount()
    {
        return askedQuestionCount;
    }

    public int GetPerformedTestCount()
    {
        return performedTestCount;
    }

    public void ToggleDialoguePanelMobile()
    {
        if (dialoguePanel == null)
            return;

        bool isActive = dialoguePanel.activeSelf;

        dialoguePanel.SetActive(!isActive);

        if (interactionModeManager != null)
            interactionModeManager.SetDialogueAvailable(!isActive);
    }

    public int GetTotalStepCount()
    {
        return totalStepCount;
    }

    // =========================
    // DİYALOG SATIRI VE TYPEWRITER SİSTEMİ
    // =========================

    // Konuşmacıya göre diyalog satırının görsel ön ekini oluşturur.
    private string GetSpeakerPrefix(string speaker)
    {
        switch (speaker)
        {
            case "HASTA":
                return "<size=45><color=#EEC28F><b>HASTA —</b></color> <size=40><color=#FFFFFF>";

            case "DOKTOR":
                return "<size=45><color=#C3D9A8><b>DOKTOR —</b></color> <size=40><color=#FFFFFF>";

            case "ASİSTAN":
                return "<size=45><color=#3498DB><b>ASİSTAN —</b></color> <size=40><color=#FFFFFF>";

            case "SİSTEM":
                return "<size=45><color=#4A4A4A><b>SİSTEM —</b></color> <size=40><color=#FFFFFF>";

            default:
                return "<size=20><color=#FFFFFF>";
        }
    }

    // Konuşmacı yazısının kapanış formatını döndürür.
    private string GetSpeakerSuffix(string speaker)
    {
        return "</color></size>";
    }

    // Yeni bir diyalog satırı UI objesi oluşturur.
    private GameObject CreateDialogueLineObject()
    {
        if (dialogueLinePrefab == null)
        {
            Debug.LogError("CreateDialogueLineObject: dialogueLinePrefab bağlı değil.");
            return null;
        }

        GameObject lineObj = Instantiate(dialogueLinePrefab, contentRoot);
        lastConversationLine = lineObj;
        return lineObj;
    }

    // Oyuncunun scroll pozisyonunun alt bölgede olup olmadığını kontrol eder.
    private bool IsUserNearBottom()
    {
        if (scrollRect == null)
            return true;

        return scrollRect.verticalNormalizedPosition <= bottomScrollThreshold;
    }

    // Belirtilen süre boyunca bekler veya skip isteği gelirse işlemi atlar.
    private IEnumerator WaitOrSkip(float seconds)
    {
        if (seconds <= 0f)
            yield break;

        float timer = 0f;

        while (timer < seconds)
        {
            if (skipRequested)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }

    // Diyalog satırını harf harf yazdırır ve scroll yönetimini sağlar.
    private IEnumerator TypeDialogueIntoLine(GameObject lineObj, string speaker, string bodyText, bool autoScroll)
    {
        if (lineObj == null)
            yield break;

        TextMeshProUGUI lineText = lineObj.GetComponentInChildren<TextMeshProUGUI>();

        if (lineText == null)
        {
            Debug.LogError("TypeDialogueIntoLine: prefab içinde TextMeshProUGUI bulunamadı.");
            yield break;
        }

        string prefix = GetSpeakerPrefix(speaker);
        string suffix = GetSpeakerSuffix(speaker);

        lineText.text = prefix + suffix;

        if (string.IsNullOrEmpty(bodyText))
        {
            if (autoScroll)
                yield return StartCoroutine(ScrollToConversationFocus(false));

            yield break;
        }

        isTyping = true;
        skipRequested = false;

        for (int i = 0; i <= bodyText.Length; i++)
        {
            if (skipRequested)
            {
                lineText.text = prefix + bodyText + suffix;
                break;
            }

            string visibleText = bodyText.Substring(0, i);
            lineText.text = prefix + visibleText + suffix;

            Canvas.ForceUpdateCanvases();

            if (autoScroll || lineObj == currentTurnFocusLine || lineObj == lastConversationLine)
                yield return StartCoroutine(ScrollToConversationFocus(false));

            if (i < bodyText.Length)
                yield return StartCoroutine(WaitOrSkip(dialogueTypeSpeed));
        }

        isTyping = false;

        if (skipRequested)
        {
            skipRequested = false;

            if (autoScroll)
                yield return StartCoroutine(ScrollToConversationFocus(false));

            yield break;
        }

        yield return StartCoroutine(WaitOrSkip(lineEndPause));
        skipRequested = false;

        if (autoScroll)
            yield return StartCoroutine(ScrollToConversationFocus(false));
    }

    // Düz metinleri typewriter efektiyle yazdırır.
    private IEnumerator TypePlainText(TextMeshProUGUI targetText, string fullText, float speed)
    {
        if (targetText == null)
            yield break;

        targetText.text = "";

        if (string.IsNullOrEmpty(fullText))
            yield break;

        isTyping = true;
        skipRequested = false;

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (skipRequested)
            {
                targetText.text = fullText;
                break;
            }

            targetText.text = fullText.Substring(0, i);

            Canvas.ForceUpdateCanvases();

            if (i < fullText.Length)
                yield return StartCoroutine(WaitOrSkip(speed));
        }

        isTyping = false;

        if (skipRequested)
        {
            skipRequested = false;
            yield break;
        }

        yield return StartCoroutine(WaitOrSkip(0.05f));
        skipRequested = false;
    }

    // Choice buton yazısını typewriter efektiyle oluşturur.
    private IEnumerator TypeOptionButtonText(DialogueOptionButton optionScript, string fullText)
    {
        if (optionScript == null)
            yield break;

        TextMeshProUGUI targetText = optionScript.buttonText;

        if (targetText == null)
            targetText = optionScript.GetComponentInChildren<TextMeshProUGUI>();

        if (targetText == null)
        {
            Debug.LogError("TypeOptionButtonText: Option button içinde TextMeshProUGUI bulunamadı.");
            yield break;
        }

        yield return StartCoroutine(TypePlainText(targetText, fullText, optionTypeSpeed));
        yield return StartCoroutine(ScrollToConversationFocus(false));
    }

    // =========================
    // OPTION UI VE SCROLL SİSTEMİ
    // =========================

    // Choice kategorilerini tutacak ana seçenek grubunu oluşturur.
    private void CreateOptionsGroup()
    {
        RemoveCurrentOptions();

        currentOptionsGroup = new GameObject("OptionsGroup");
        currentOptionsGroup.transform.SetParent(contentRoot, false);

        RectTransform rect = currentOptionsGroup.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        VerticalLayoutGroup layout = currentOptionsGroup.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.padding.top = 6;
        layout.padding.bottom = 6;
        layout.padding.left = 0;
        layout.padding.right = 0;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = currentOptionsGroup.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        questionsContainer = null;
        testsContainer = null;
        diagnosisContainer = null;
    }

    // Belirtilen kategori için başlık ve container oluşturur.
    private void CreateSectionHeaderAndContainer(string title, out Transform sectionContainer)
    {
        CreateSectionHeader(title);
        CreateDivider();

        GameObject containerObj = new GameObject(title + "Container");
        containerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = containerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.padding = new RectOffset(0, 0, 0, 4);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = containerObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        sectionContainer = containerObj.transform;
    }

    // Seçim kategorisi için görsel başlık oluşturur.
    private void CreateSectionHeader(string title)
    {
        GameObject headerObj = new GameObject(title + "Header");
        headerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = headerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        LayoutElement layoutElement = headerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 26f;
        layoutElement.preferredHeight = 26f;

        TextMeshProUGUI text = headerObj.AddComponent<TextMeshProUGUI>();
        text.text = title.ToUpper();
        text.fontSize = 22;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color32(180, 180, 180, 255);
        text.alignment = TextAlignmentOptions.Left;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }

    // Bölümler arasında görsel ayırıcı oluşturur.
    private void CreateDivider()
    {
        GameObject dividerObj = new GameObject("Divider");
        dividerObj.transform.SetParent(currentOptionsGroup.transform, false);

        RectTransform rect = dividerObj.AddComponent<RectTransform>();
        rect.localScale = Vector3.one;

        LayoutElement layoutElement = dividerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 2f;
        layoutElement.preferredHeight = 2f;

        Image image = dividerObj.AddComponent<Image>();
        image.color = new Color32(90, 90, 90, 255);
    }

    // Aktif seçenek grubunu temizler.
    private void RemoveCurrentOptions()
    {
        if (currentOptionsGroup != null)
        {
            Destroy(currentOptionsGroup);
            currentOptionsGroup = null;
        }

        questionsContainer = null;
        testsContainer = null;
        diagnosisContainer = null;
    }

    // Tüm diyalog içeriğini temizler.
    private void ClearDialogueContent()
    {
        RemoveCurrentOptions();
        lastConversationLine = null;
        currentTurnFocusLine = null;

        if (contentRoot != null)
        {
            foreach (Transform child in contentRoot)
                Destroy(child.gameObject);
        }
    }

    // Aktif konuşma satırına doğru akıllı scroll uygular.
    private IEnumerator ScrollToConversationFocus(bool forceScroll = false)
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        if (scrollRect == null)
            yield break;

        RectTransform viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : scrollRect.GetComponent<RectTransform>();

        if (viewport == null || contentRect == null)
            yield break;

        if (!forceScroll && !IsUserNearBottom())
            yield break;

        GameObject focusObj = currentTurnFocusLine != null ? currentTurnFocusLine : lastConversationLine;
        if (focusObj == null)
            yield break;

        RectTransform focusRect = focusObj.GetComponent<RectTransform>();
        if (focusRect == null)
            yield break;

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
        {
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0f;
            yield break;
        }

        float focusTop = Mathf.Abs(focusRect.anchoredPosition.y);
        float topPadding = 50f;

        float scrollY = Mathf.Clamp(focusTop - topPadding, 0f, contentHeight - viewportHeight);
        float normalized = 1f - (scrollY / (contentHeight - viewportHeight));

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = normalized;
    }
}