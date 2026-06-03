using UnityEngine;

public class DoorAutoController : MonoBehaviour
{
    public Transform doorObject;
    public Vector3 closedRotation = Vector3.zero;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float speed = 10f;

    private Quaternion targetRotation;

    // =====================================================
    // BAŞLANGIÇ KAPI DURUMU
    // =====================================================

    void Start()
    {
        // Oyun başlangıcında kapı kapalı hale getirilir.
        CloseDoorInstant();
    }

    // =====================================================
    // KAPI ROTASYON ANİMASYONU
    // =====================================================

    void Update()
    {
        if (doorObject == null)
            return;

        // Kapı hedef rotasyona yumuşak şekilde döndürülür.
        doorObject.localRotation = Quaternion.Lerp(
            doorObject.localRotation,
            targetRotation,
            Time.deltaTime * speed
        );
    }

    // =====================================================
    // KAPI AÇMA İŞLEMİ
    // =====================================================

    public void OpenDoor()
    {
        targetRotation = Quaternion.Euler(openRotation);
    }

    // =====================================================
    // KAPI KAPATMA İŞLEMİ
    // =====================================================

    public void CloseDoor()
    {
        targetRotation = Quaternion.Euler(closedRotation);
    }

    // =====================================================
    // KAPIYI ANINDA KAPATMA
    // =====================================================

    public void CloseDoorInstant()
    {
        targetRotation = Quaternion.Euler(closedRotation);

        if (doorObject != null)
            doorObject.localRotation = targetRotation;
    }
}