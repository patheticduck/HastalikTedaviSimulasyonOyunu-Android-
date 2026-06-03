using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class NotebookUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject notebookPanel;
    public TextMeshProUGUI symptomsText;
    public TextMeshProUGUI tagsText;
    public TextMeshProUGUI testsText;

    private DialogueManager currentDialogueManager;

    // =====================================================
    // BAŞLANGIÇ PANEL AYARLARI
    // =====================================================

    private void Start()
    {
        // Notebook paneli başlangıçta kapalı tutulur.
        if (notebookPanel != null)
            notebookPanel.SetActive(false);
    }

    // =====================================================
    // DIALOGUE MANAGER REFERANSI ATAMA
    // =====================================================

    public void SetDialogueManager(DialogueManager manager)
    {
        currentDialogueManager = manager;
    }

    // =====================================================
    // NOTEBOOK PANELİNİ AÇMA
    // =====================================================

    public void OpenNotebookUI()
    {
        // Açılmadan önce notebook verileri güncellenir.
        RefreshNotebook();

        if (notebookPanel != null)
            notebookPanel.SetActive(true);
    }

    // =====================================================
    // NOTEBOOK PANELİNİ KAPATMA
    // =====================================================

    public void CloseNotebookUI()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);
    }

    // =====================================================
    // NOTEBOOK VERİLERİNİ UI'A AKTARMA
    // =====================================================

    public void RefreshNotebook()
    {
        // DialogueManager atanmadıysa işlem yapılmaz.
        if (currentDialogueManager == null)
        {
            Debug.LogWarning("NotebookUIManager -> currentDialogueManager atanmadı.");
            return;
        }

        // Notebook kayıtları DialogueManager üzerinden alınır.
        HashSet<string> symptoms =
            currentDialogueManager.GetNotebookSymptoms();

        HashSet<string> notes =
            currentDialogueManager.GetNotebookNotes();

        HashSet<string> tests =
            currentDialogueManager.GetNotebookTests();

        // Belirti listesi güncellenir.
        if (symptomsText != null)
        {
            symptomsText.text =
                BuildSectionText(symptoms, "Henüz belirti kaydı yok.");
        }

        // İpucu/not listesi güncellenir.
        if (tagsText != null)
        {
            tagsText.text =
                BuildSectionText(notes, "Henüz ipucu kaydı yok.");
        }

        // Yapılan test listesi güncellenir.
        if (testsText != null)
        {
            testsText.text =
                BuildSectionText(tests, "Henüz test yapılmadı.");
        }
    }

    // =====================================================
    // NOTEBOOK BÖLÜM METNİ OLUŞTURMA
    // =====================================================

    private string BuildSectionText(HashSet<string> items, string emptyMessage)
    {
        if (items == null || items.Count == 0)
            return emptyMessage;

        StringBuilder sb = new StringBuilder();

        foreach (string item in items)
        {
            sb.AppendLine("• " + FormatLabel(item));
        }

        return sb.ToString().TrimEnd();
    }

    // =====================================================
    // NOTEBOOK YAZI FORMATINI DÜZENLEME
    // =====================================================

    private string FormatLabel(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "-";

        string formatted = raw.Trim();

        // Tek karakterli ifadeler direkt büyük harfe çevrilir.
        if (formatted.Length == 1)
            return formatted.ToUpper();

        // İlk harf büyük olacak şekilde düzenlenir.
        return char.ToUpper(formatted[0]) + formatted.Substring(1);
    }
}