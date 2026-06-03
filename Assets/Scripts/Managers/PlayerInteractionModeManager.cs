using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionModeManager : MonoBehaviour
{
    // =====================================================
    // OYUNCU ETKİLEŞİM MODU YÖNETİMİ
    // =====================================================

    [Header("References")]
    public MouseLookLimited mouseLook;
    public DialogueManager dialogueManager;
    public GameObject dialoguePanel;
    public GameFlowManager gameFlowManager;
    public EnergyUIManager energyUIManager;

    [Header("Mobile")]
    public GameObject mobileDialogueButton;

    [Header("Optional Focus")]
    public Transform doctorLookRoot;
    public Transform dialogueFocusPoint;
    public bool snapToDialogueFocusOnOpen = false;

    [Header("Input")]
    public Key toggleDialogueKey = Key.Tab;

    private bool dialogueModeActive = false;
    private bool dialogueAvailable = false;
    private bool forceUIMode = false;

    // =====================================================
    // BAŞLANGIÇ AYARLARI
    // =====================================================

    // Gerekli manager referanslarını sahneden otomatik bulur.
    private void Start()
    {
        if (gameFlowManager == null)
            gameFlowManager = FindFirstObjectByType<GameFlowManager>();

        if (energyUIManager == null)
            energyUIManager = FindFirstObjectByType<EnergyUIManager>();
    }

    // =====================================================
    // INPUT VE MOD KONTROLLERİ
    // =====================================================

    // Oyuncunun dialog açma/kapatma inputlarını kontrol eder.
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        PauseMenuController pause = FindFirstObjectByType<PauseMenuController>();
        if (pause != null && pause.IsPaused())
            return;

        // Enerji onay paneli açıksa Tab ile dialogue aç/kapat yapılmasın.
        // Oyuncu önce Evet/Hayır seçsin.
        if (energyUIManager != null && energyUIManager.IsConfirmPanelOpen())
            return;

        if (forceUIMode)
            return;

        if (Keyboard.current[toggleDialogueKey].wasPressedThisFrame)
        {
            if (!dialogueAvailable)
                return;

            // Tanı seçildikten sonra son konuşma akarken
            // oyuncu dialog panelini manuel kapatamasın.
            if (gameFlowManager != null && gameFlowManager.IsDiagnosisFlowInProgress())
                return;

            ToggleDialogueMode();
        }
    }

    // Oyuncuyu zorunlu UI moduna geçirir.
    public void EnterUIMode()
    {
        forceUIMode = true;

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);
    }

    // Oyuncuyu normal gameplay moduna geri döndürür.
    public void ExitUIMode()
    {
        forceUIMode = false;

        if (!dialogueModeActive && mouseLook != null)
            mouseLook.SetLookEnabled(true);

        if (!dialogueModeActive && gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();
    }

    // Dialog sisteminin kullanılabilir olup olmadığını ayarlar.
    public void SetDialogueAvailable(bool available)
    {
        dialogueAvailable = available;

        // Mobil dialog butonu güncellenir.
        if (mobileDialogueButton != null)
            mobileDialogueButton.SetActive(available);

        if (!available)
        {
            dialogueModeActive = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (!forceUIMode && mouseLook != null)
                mouseLook.SetLookEnabled(true);

            // Dialogue tamamen kapandıysa güvenli free-look durumunu yeniden değerlendirir.
            if (!forceUIMode && gameFlowManager != null)
                gameFlowManager.NotifyFreeLookStateEntered();
        }
    }

    // =====================================================
    // DİYALOG MODU YÖNETİMİ
    // =====================================================

    // Dialog modunu açar ve oyuncu kontrolünü UI moduna geçirir.
    public void OpenDialogueMode()
    {
        if (!dialogueAvailable)
            dialogueAvailable = true;

        dialogueModeActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (mouseLook != null)
            mouseLook.SetLookEnabled(false);

        // İstenirse kamerayı dialog focus noktasına hizalar.
        if (snapToDialogueFocusOnOpen && doctorLookRoot != null && dialogueFocusPoint != null)
        {
            doctorLookRoot.position = dialogueFocusPoint.position;
            doctorLookRoot.rotation = dialogueFocusPoint.rotation;
        }

        // Dialogue açıkken notebook hiçbir şekilde açılmamalı.
        if (gameFlowManager != null)
            gameFlowManager.SetNotebookBlocked(true);
    }

    // Dialog modunu kapatır ve oyuncu kontrolünü geri verir.
    public void CloseDialogueMode()
    {
        dialogueModeActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (!forceUIMode && mouseLook != null)
            mouseLook.SetLookEnabled(true);

        // Dialogue kapanınca GameFlowManager güvenli durumu tekrar değerlendirir.
        if (!forceUIMode && gameFlowManager != null)
            gameFlowManager.NotifyFreeLookStateEntered();
    }

    // Dialog modunu açıp kapatan toggle sistemi.
    public void ToggleDialogueMode()
    {
        if (dialogueModeActive)
            CloseDialogueMode();
        else
            OpenDialogueMode();
    }

    // =====================================================
    // DURUM KONTROL METODLARI
    // =====================================================

    public bool IsDialogueModeActive()
    {
        return dialogueModeActive;
    }

    public bool IsDialogueAvailable()
    {
        return dialogueAvailable;
    }

    public bool IsInForcedUIMode()
    {
        return forceUIMode;
    }
}