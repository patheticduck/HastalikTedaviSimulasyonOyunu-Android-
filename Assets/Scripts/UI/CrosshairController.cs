using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair")]
    public GameObject crosshair;

    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject DialoguePanel;
    public GameObject NotebookPanel;
    public GameObject PatientResultPanel;
    public GameObject EndDayPanel;
    public GameObject DayTransitionPanel;
    public GameObject GameOverPanel;
    public GameObject AIAnalysisPanel;

    // =====================================================
    // CROSSHAIR GÖRÜNÜRLÜK KONTROLLERİ
    // =====================================================

    void Update()
    {
        // Belirli UI panelleri açıkken crosshair gizlenir.
        bool shouldHide =
            IsOpen(MainMenuPanel) ||
            IsOpen(DialoguePanel) ||
            IsOpen(NotebookPanel) ||
            IsOpen(PatientResultPanel) ||
            IsOpen(EndDayPanel) ||
            IsOpen(DayTransitionPanel) ||
            IsOpen(GameOverPanel) ||
            IsOpen(AIAnalysisPanel);

        if (crosshair != null)
            crosshair.SetActive(!shouldHide);
    }

    // =====================================================
    // PANEL AKTİFLİK KONTROLÜ
    // =====================================================

    bool IsOpen(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }
}