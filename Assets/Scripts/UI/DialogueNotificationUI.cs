using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueNotificationUI : MonoBehaviour
{
    [Header("UI")]
    public Button notificationButton;
    public GameObject notificationInfoPanel;
    public Image notificationButtonImage;
    public TextMeshProUGUI notificationText;

    [Header("Linked Panels")]
    public GameObject dialoguePanel;

    [Header("Text")]
    [TextArea]
    public string message =
        "Yeni sorular ve testler kullanılabilir!\nTanı koymak istiyorsanız diyalog panelini kaydırınız.";

    [Header("Blink")]
    public float blinkSpeed = 0.45f;
    public float minAlpha = 0.35f;
    public float maxAlpha = 1f;

    [Header("Pulse")]
    public float minScale = 1f;
    public float maxScale = 1.18f;

    private Coroutine blinkRoutine;
    private bool isBlinking = false;

    // =====================================================
    // UI BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Awake()
    {
        // Bilgilendirme paneli başlangıçta kapatılır.
        if (notificationInfoPanel != null)
            notificationInfoPanel.SetActive(false);

        // Bildirim mesajı UI'a aktarılır.
        if (notificationText != null)
            notificationText.text = message;

        // Bildirim butonu click eventi atanır.
        if (notificationButton != null)
        {
            notificationButton.onClick.RemoveAllListeners();
            notificationButton.onClick.AddListener(ToggleNotificationPanel);
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    // =====================================================
    // DİYALOG DURUMU KONTROLLERİ
    // =====================================================

    private void Update()
    {
        // Diyalog paneli kapalıysa bildirim sistemi durdurulur.
        if (!IsDialogueContextActive())
        {
            if (notificationInfoPanel != null &&
                notificationInfoPanel.activeSelf)
            {
                notificationInfoPanel.SetActive(false);
            }

            if (isBlinking)
                StopNotification();
        }
    }

    // =====================================================
    // YENİ SEÇENEK BİLDİRİMİNİ BAŞLATMA
    // =====================================================

    public void NotifyNewOptions()
    {
        // Obje aktif değilse bildirim başlatılmaz.
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("DialogueNotificationUI -> Obje aktif değil, bildirim başlatılmadı.");
            return;
        }

        // Diyalog paneli aktif değilse çalışmaz.
        if (!IsDialogueContextActive())
            return;

        // Bildirim zaten çalışıyorsa tekrar başlatılmaz.
        if (isBlinking)
        {
            return;
        }

        isBlinking = true;

        // Bildirim sesi oynatılır.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayNotification();
        else
            Debug.LogWarning("DialogueNotificationUI -> AudioManager NULL!");

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    // =====================================================
    // BİLDİRİM EFEKTİNİ DURDURMA
    // =====================================================

    public void StopNotification()
    {
        isBlinking = false;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    // =====================================================
    // BİLDİRİM DURUMUNU SIFIRLAMA
    // =====================================================

    public void ResetNotificationState()
    {
        StopNotification();

        if (notificationInfoPanel != null)
            notificationInfoPanel.SetActive(false);
    }

    // =====================================================
    // BİLGİ PANELİNİ AÇMA / KAPATMA
    // =====================================================

    private void ToggleNotificationPanel()
    {
        if (notificationInfoPanel == null)
            return;

        // Diyalog paneli aktif değilse bilgi paneli kapatılır.
        if (!IsDialogueContextActive())
        {
            notificationInfoPanel.SetActive(false);
            return;
        }

        bool nextState = !notificationInfoPanel.activeSelf;
        notificationInfoPanel.SetActive(nextState);

        StopNotification();
    }

    // =====================================================
    // DİYALOG PANELİ AKTİFLİK KONTROLÜ
    // =====================================================

    private bool IsDialogueContextActive()
    {
        return dialoguePanel != null &&
               dialoguePanel.activeInHierarchy;
    }

    // =====================================================
    // YANIP SÖNME VE PULSE ANİMASYONU
    // =====================================================

    private IEnumerator BlinkRoutine()
    {
        while (isBlinking)
        {
            float t = Mathf.PingPong(Time.unscaledTime / blinkSpeed, 1f);

            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            float scale = Mathf.Lerp(minScale, maxScale, t);

            SetButtonAlpha(alpha);

            if (notificationButton != null)
                notificationButton.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        SetButtonAlpha(maxAlpha);

        if (notificationButton != null)
            notificationButton.transform.localScale = Vector3.one;
    }

    // =====================================================
    // BUTON ALPHA DEĞERİNİ GÜNCELLEME
    // =====================================================

    private void SetButtonAlpha(float alpha)
    {
        if (notificationButtonImage == null)
            return;

        Color c = notificationButtonImage.color;
        c.a = alpha;
        notificationButtonImage.color = c;
    }
}