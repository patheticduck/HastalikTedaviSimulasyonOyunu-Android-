using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DialogueOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public TextMeshProUGUI buttonText;

    [Header("Colors")]
    public Color normalColor = new Color32(190, 110, 45, 255);
    public Color hoverColor = new Color32(140, 75, 25, 255);

    [HideInInspector] public DialogueChoiceData choiceData;
    [HideInInspector] public DiagnosisChoiceData diagnosisData;

    private DialogueManager dialogueManager;
    private Button button;

    // =====================================================
    // REFERANS VE BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Awake()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    private void OnEnable()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    private void OnDisable()
    {
        ApplyNormalColor();

        // Tooltip açık kaldıysa gizlenir.
        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();
    }

    private void Reset()
    {
        EnsureReferences();
        ApplyNormalColor();
    }

    // =====================================================
    // NORMAL DİYALOG SEÇENEĞİ AYARLARI
    // =====================================================

    public void SetupChoice(DialogueChoiceData choice, DialogueManager manager)
    {
        choiceData = choice;
        diagnosisData = null;
        dialogueManager = manager;

        EnsureReferences();

        // Seçenek yazısını UI'a aktarır.
        if (buttonText != null && choice != null)
        {
            buttonText.text = choice.choiceText;
            ApplyNormalColor();
            buttonText.raycastTarget = false;
        }
        else
        {
            Debug.LogError("SetupChoice: buttonText veya choice null.");
        }

        // Buton click ayarları yapılır.
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickChoice);

            button.transition = Selectable.Transition.None;
            button.interactable = true;
        }
        else
        {
            Debug.LogError("SetupChoice: Button component bulunamadı.");
        }
    }

    // =====================================================
    // TANI SEÇENEĞİ AYARLARI
    // =====================================================

    public void SetupDiagnosisChoice(DiagnosisChoiceData diagnosis, DialogueManager manager)
    {
        diagnosisData = diagnosis;
        choiceData = null;
        dialogueManager = manager;

        EnsureReferences();

        // Tanı yazısını UI'a aktarır.
        if (buttonText != null && diagnosis != null)
        {
            buttonText.text = diagnosis.diagnosisName;
            ApplyNormalColor();
            buttonText.raycastTarget = false;
        }
        else
        {
            Debug.LogError("SetupDiagnosisChoice: buttonText veya diagnosis null.");
        }

        // Buton click ayarları yapılır.
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickDiagnosisChoice);

            button.transition = Selectable.Transition.None;
            button.interactable = true;
        }
        else
        {
            Debug.LogError("SetupDiagnosisChoice: Button component bulunamadı.");
        }
    }

    // =====================================================
    // NORMAL SEÇENEK TIKLAMA İŞLEMLERİ
    // =====================================================

    private void OnClickChoice()
    {
        // Tooltip gizlenir.
        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();

        // Çift tıklamayı engellemek için buton devre dışı bırakılır.
        if (button != null)
            button.interactable = false;

        // DialogueManager üzerinden seçim çalıştırılır.
        if (dialogueManager != null && choiceData != null)
        {
            dialogueManager.SelectChoice(choiceData);
        }
        else
        {
            Debug.LogError("Choice click: dialogueManager veya choiceData null");
        }
    }

    // =====================================================
    // TANI SEÇENEĞİ TIKLAMA İŞLEMLERİ
    // =====================================================

    private void OnClickDiagnosisChoice()
    {
        // Tooltip gizlenir.
        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();

        // Çift tıklamayı engellemek için buton devre dışı bırakılır.
        if (button != null)
            button.interactable = false;

        // Tanı seçimi DialogueManager'a gönderilir.
        if (dialogueManager != null && diagnosisData != null)
        {
            dialogueManager.SelectDiagnosis(diagnosisData);
        }
        else
        {
            Debug.LogError("Diagnosis click: dialogueManager veya diagnosisData null");
        }
    }

    // =====================================================
    // POINTER EVENTLERİ
    // =====================================================

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyHoverColor();

        // Test seçeneklerinde başarı şansı tooltipi gösterilir.
        if (choiceData != null &&
            choiceData.choiceType == ChoiceType.Test &&
            choiceData.useTestChance &&
            TestChanceTooltipUI.Instance != null)
        {
            TestChanceTooltipUI.Instance.Show(choiceData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalColor();

        if (TestChanceTooltipUI.Instance != null)
            TestChanceTooltipUI.Instance.Hide();
    }

    // =====================================================
    // COMPONENT REFERANSLARINI ALMA
    // =====================================================

    private void EnsureReferences()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button == null)
            button = GetComponent<Button>();
    }

    // =====================================================
    // YAZI RENK KONTROLLERİ
    // =====================================================

    private void ApplyNormalColor()
    {
        if (buttonText != null)
            buttonText.color = normalColor;
    }

    private void ApplyHoverColor()
    {
        if (buttonText != null)
            buttonText.color = hoverColor;
    }
}