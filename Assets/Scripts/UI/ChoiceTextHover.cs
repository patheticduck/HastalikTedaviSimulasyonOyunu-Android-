using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ChoiceTextHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI label;

    private readonly Color normalColor = new Color32(176, 216, 242, 255);
    private readonly Color hoverColor = new Color32(70, 130, 180, 255);

    // =====================================================
    // EDITOR REFERANS KONTROLÜ
    // =====================================================

    private void Reset()
    {
        // Label referansı otomatik alınır.
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();
    }

    // =====================================================
    // BAŞLANGIÇ REFERANS VE RENK AYARLARI
    // =====================================================

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>();

        ApplyNormalColor();
    }

    private void OnEnable()
    {
        ApplyNormalColor();
    }

    // =====================================================
    // HOVER EVENTLERİ
    // =====================================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mouse üzerine geldiğinde hover rengi uygulanır.
        if (label != null)
            label.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyNormalColor();
    }

    // =====================================================
    // OBJECT DEVRE DIŞI KALDIĞINDA RENK SIFIRLAMA
    // =====================================================

    private void OnDisable()
    {
        ApplyNormalColor();
    }

    // =====================================================
    // NORMAL YAZI RENGİNİ UYGULAMA
    // =====================================================

    private void ApplyNormalColor()
    {
        if (label != null)
            label.color = normalColor;
    }
}