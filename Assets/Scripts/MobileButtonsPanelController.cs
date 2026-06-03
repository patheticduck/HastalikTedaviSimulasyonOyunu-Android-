using UnityEngine;

public class MobileButtonsPanelController : MonoBehaviour
{
    [Header("Mobile Panel")]
    public GameObject mobileButtonsPanel;

    [Header("Computer Camera")]
    public AIComputerCameraController aiComputerCameraController;

    [Header("Panels That Hide Mobile UI")]
    public GameObject[] blockingPanels;

    private void Update()
    {
        bool shouldShow = true;

        // Bilgisayara geçiş sırasında mobil UI gizlenir.
        if (aiComputerCameraController != null)
        {
            if (aiComputerCameraController.IsMovingToComputer() ||
                aiComputerCameraController.IsComputerModeActive())
            {
                shouldShow = false;
            }
        }

        // Herhangi bir engelleyici panel açıksa
        // mobil butonlar gizlenir.
        foreach (GameObject panel in blockingPanels)
        {
            if (panel != null &&
                panel.activeInHierarchy)
            {
                shouldShow = false;
                break;
            }
        }

        // Panel görünürlüğü güncellenir.
        if (mobileButtonsPanel != null &&
            mobileButtonsPanel.activeSelf != shouldShow)
        {
            mobileButtonsPanel.SetActive(shouldShow);
        }
    }
}