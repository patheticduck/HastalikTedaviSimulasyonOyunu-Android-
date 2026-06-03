using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIScrollForwarder : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect parentScrollRect;

    // =====================================================
    // PARENT SCROLLRECT REFERANSINI ALMA
    // =====================================================

    private void Awake()
    {
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    // =====================================================
    // SCROLL EVENTLERİNİ PARENT SCROLLRECT'E AKTARMA
    // =====================================================

    public void OnScroll(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnScroll(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
            parentScrollRect.OnEndDrag(eventData);
    }
}