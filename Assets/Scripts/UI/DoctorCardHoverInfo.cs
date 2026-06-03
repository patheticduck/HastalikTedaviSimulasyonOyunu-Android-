using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoctorCardHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Info")]
    public GameObject hoverInfoPanel;

    [Header("Scale Target")]
    public RectTransform scaleTarget;

    [Header("Animation")]
    public float hoverScale = 1.04f;
    public float animationDuration = 0.15f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    // =====================================================
    // BAŞLANGIÇ REFERANS VE DURUM AYARLARI
    // =====================================================

    private void Awake()
    {
        // Scale hedefi atanmadıysa mevcut RectTransform alınır.
        if (scaleTarget == null)
            scaleTarget = GetComponent<RectTransform>();

        originalScale = scaleTarget.localScale;

        ResetHoverState();
    }

    private void OnEnable()
    {
        ResetHoverState();
    }

    private void OnDisable()
    {
        ResetHoverState();
    }

    // =====================================================
    // HOVER GİRİŞ / ÇIKIŞ EVENTLERİ
    // =====================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Hover bilgi paneli açılır.
        if (hoverInfoPanel != null)
            hoverInfoPanel.SetActive(true);

        // Hover scale animasyonu başlatılır.
        StartScaleAnimation(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHoverState();
    }

    // =====================================================
    // HOVER DURUMUNU SIFIRLAMA
    // =====================================================

    private void ResetHoverState()
    {
        // Aktif animasyon durdurulur.
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // Hover bilgi paneli kapatılır.
        if (hoverInfoPanel != null)
            hoverInfoPanel.SetActive(false);

        // Kart eski scale değerine döndürülür.
        if (scaleTarget != null)
            scaleTarget.localScale = originalScale;
    }

    // =====================================================
    // SCALE ANİMASYONUNU BAŞLATMA
    // =====================================================

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (scaleTarget == null)
            return;

        // Önceki animasyon varsa durdurulur.
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    // =====================================================
    // SCALE GEÇİŞ ANİMASYONU
    // =====================================================

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = scaleTarget.localScale;
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / animationDuration);

            scaleTarget.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        scaleTarget.localScale = targetScale;
    }
}