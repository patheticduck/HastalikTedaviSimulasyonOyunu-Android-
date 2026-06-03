using UnityEngine;
using TMPro;

public class TestChanceTooltipUI : MonoBehaviour
{
    public static TestChanceTooltipUI Instance { get; private set; }

    [Header("UI")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    // =====================================================
    // SINGLETON VE TOOLTIP BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Awake()
    {
        Instance = this;

        // Tooltip panelinin raycast ayarları yapılır.
        if (tooltipPanel != null)
        {
            CanvasGroup cg = tooltipPanel.GetComponent<CanvasGroup>();

            if (cg == null)
                cg = tooltipPanel.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        Hide();
    }

    // =====================================================
    // TOOLTIP GÖSTERME İŞLEMİ
    // =====================================================

    public void Show(DialogueChoiceData choice)
    {
        if (choice == null || tooltipPanel == null || tooltipText == null)
            return;

        int finalChance = CalculateFinalChance(choice);

        // Tooltip içeriği oluşturulur.
        tooltipText.text =
            choice.choiceText +
            "\n\nBaşarı Şansı: %" + finalChance +
            "\n\nUzmanlık Bonusu: " + GetBonusText(choice);

        tooltipPanel.SetActive(true);
    }

    // =====================================================
    // TOOLTIP GİZLEME İŞLEMİ
    // =====================================================

    public void Hide()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    // =====================================================
    // SON BAŞARI ŞANSINI HESAPLAMA
    // =====================================================

    private int CalculateFinalChance(DialogueChoiceData choice)
    {
        int chance = choice.baseSuccessChance;

        DoctorProfileManager profile = DoctorProfileManager.Instance;

        // Doktor uzmanlığına göre bonus uygulanır.
        if (profile != null)
        {
            DoctorSpecialization selected =
                profile.GetSelectedSpecialization();

            if (selected == choice.bonusSpecialization)
                chance += choice.specializationBonus;

            else if (selected == DoctorSpecialization.Generalist)
                chance += choice.generalistBonus;
        }

        return Mathf.Clamp(chance, 0, 100);
    }

    // =====================================================
    // BONUS METNİNİ OLUŞTURMA
    // =====================================================

    private string GetBonusText(DialogueChoiceData choice)
    {
        DoctorProfileManager profile = DoctorProfileManager.Instance;

        if (profile == null)
            return "Yok";

        DoctorSpecialization selected =
            profile.GetSelectedSpecialization();

        // Uzmanlık bonusu bilgisi oluşturulur.
        if (selected == choice.bonusSpecialization)
        {
            return "+" +
                   choice.specializationBonus +
                   " (" + selected + ")";
        }

        // Generalist bonusu bilgisi oluşturulur.
        if (selected == DoctorSpecialization.Generalist)
        {
            return "+" +
                   choice.generalistBonus +
                   " (Generalist)";
        }

        return "Yok";
    }
}