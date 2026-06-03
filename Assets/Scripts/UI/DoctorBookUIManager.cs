using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoctorBookUIManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject doctorBookPanel;

    [Header("Left Page")]
    public TextMeshProUGUI diseaseTitleText;
    public TextMeshProUGUI pageContentText;

    [Header("Right Page")]
    public Image diseaseImage;
    public TextMeshProUGUI extraInfoText;
    public TextMeshProUGUI pageNumberText;

    [Header("Pages")]
    public List<DiseaseBookPageData> pages;

    [Header("Highlight")]
    public string highlightColor = "#FFF176AA";

    private int currentPageIndex = 0;
    private DialogueManager dialogueManager;

    // =====================================================
    // BAŞLANGIÇ PANEL AYARLARI
    // =====================================================

    private void Start()
    {
        // DoctorBook paneli başlangıçta kapalı tutulur.
        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);
    }

    // =====================================================
    // DOCTOR BOOK PANELİNİ AÇMA
    // =====================================================

    public void OpenBook()
    {
        if (doctorBookPanel != null)
        {
            doctorBookPanel.SetActive(true);
            doctorBookPanel.transform.SetAsLastSibling();

            // Panelin UI etkileşim ayarları yapılır.
            CanvasGroup canvasGroup = doctorBookPanel.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        // Aktif DialogueManager referansı alınır.
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        currentPageIndex = 0;
        ShowCurrentPage();
    }

    // =====================================================
    // PANEL AKTİFLİK KONTROLÜ
    // =====================================================

    public bool IsBookPanelOpen()
    {
        return doctorBookPanel != null && doctorBookPanel.activeInHierarchy;
    }

    // =====================================================
    // DOCTOR BOOK PANELİNİ KAPATMA
    // =====================================================

    public void CloseBook()
    {
        if (doctorBookPanel != null)
            doctorBookPanel.SetActive(false);
    }

    // =====================================================
    // SAYFA GEÇİŞ KONTROLLERİ
    // =====================================================

    public void NextPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        currentPageIndex++;

        // Son sayfadan sonra ilk sayfaya döner.
        if (currentPageIndex >= pages.Count)
            currentPageIndex = 0;

        ShowCurrentPage();
    }

    public void PrevPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        currentPageIndex--;

        // İlk sayfadan geri gidildiğinde son sayfaya geçer.
        if (currentPageIndex < 0)
            currentPageIndex = pages.Count - 1;

        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        PrevPage();
    }

    // =====================================================
    // MEVCUT SAYFAYI UI'A AKTARMA
    // =====================================================

    private void ShowCurrentPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        DiseaseBookPageData page = pages[currentPageIndex];

        // Hastalık başlığı güncellenir.
        if (diseaseTitleText != null)
            diseaseTitleText.text = page.diseaseName;

        // Sol sayfa içeriği oluşturulur.
        if (pageContentText != null)
        {
            string content =
                "<b>GENEL BİLGİ</b>\n" + page.generalInfo + "\n\n" +
                "<b>SEMPTOMLAR</b>\n" + page.symptomsInfo;
            // + "\n\n" +
            //"<b>TANI İPUÇLARI</b>\n" + page.diagnosisTips;

            pageContentText.text = ApplyEvidenceHighlights(content, page);
        }

        // Sağ sayfa ek bilgi alanı temizlenir.
        if (extraInfoText != null)
            extraInfoText.text = "";

        // Hastalık görseli güncellenir.
        if (diseaseImage != null)
        {
            diseaseImage.sprite = page.diseaseImage;
            diseaseImage.gameObject.SetActive(page.diseaseImage != null);
        }

        // Sayfa numarası güncellenir.
        if (pageNumberText != null)
            pageNumberText.text = (currentPageIndex + 1) + " / " + pages.Count;
    }

    // =====================================================
    // NOTEBOOK KANITLARINA GÖRE KELİME HIGHLIGHT
    // =====================================================

    private string ApplyEvidenceHighlights(string text, DiseaseBookPageData page)
    {
        if (dialogueManager == null)
            return text;

        HashSet<string> evidenceTexts = new HashSet<string>();

        // Notebook ve diyalog verileri toplanır.
        foreach (string item in dialogueManager.GetNotebookSymptoms())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetNotebookNotes())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetNotebookTests())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetDiscoveredSymptoms())
            evidenceTexts.Add(item);

        foreach (string item in dialogueManager.GetPerformedTests())
            evidenceTexts.Add(item);

        if (evidenceTexts.Count == 0)
            return text;

        string result = text;

        if (page.highlightKeywords == null)
            return result;

        // Keşfedilen anahtar kelimeler highlight edilir.
        foreach (string keyword in page.highlightKeywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            if (IsKeywordDiscovered(keyword, evidenceTexts))
                result = HighlightWord(result, keyword);
        }

        return result;
    }

    // =====================================================
    // ANAHTAR KELİMENİN KEŞFEDİLİP KEŞFEDİLMEDİĞİNİ KONTROL ETME
    // =====================================================

    private bool IsKeywordDiscovered(string keyword, HashSet<string> evidenceTexts)
    {
        string normalizedKeyword = NormalizeText(keyword);

        foreach (string evidence in evidenceTexts)
        {
            string normalizedEvidence = NormalizeText(evidence);

            if (normalizedEvidence.Contains(normalizedKeyword))
                return true;

            if (normalizedKeyword.Contains(normalizedEvidence))
                return true;
        }

        return false;
    }

    // =====================================================
    // METİN İÇİNDE KELİMEYİ HIGHLIGHT ETME
    // =====================================================

    private string HighlightWord(string text, string word)
    {
        return text.Replace(
            word,
            $"<mark={highlightColor}>{word}</mark>",
            System.StringComparison.OrdinalIgnoreCase
        );
    }

    // =====================================================
    // TÜRKÇE KARAKTERLERİ NORMALLEŞTİRME
    // =====================================================

    private string NormalizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        return input
            .ToLower()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ü", "u")
            .Replace("ç", "c");
    }
}