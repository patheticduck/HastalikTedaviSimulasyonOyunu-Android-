using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookLimited : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder;
    public FixedJoystick lookJoystick;
    [Header("Mobile Control")]
    public bool useJoystick = true;

    [Header("Sensitivity")]
    public float mouseSensitivity = 0.015f;

    [Header("Horizontal Limits")]
    public float horizontalLimit = 80f;

    [Header("Vertical Limits")]
    public float minPitch = -20f;
    public float maxPitch = 20f;

    [Header("State")]
    public bool startEnabled = false;

    private float yawOffset;
    private float pitch;
    private float baseYaw;
    private bool canLook;

    // =====================================================
    // BAŞLANGIÇ ROTASYON VE DURUM AYARLARI
    // =====================================================

    private void Start()
    {
        // Başlangıç yatay rotasyonu kaydedilir.
        baseYaw = NormalizeAngle(transform.localEulerAngles.y);

        // Kamera holder başlangıç pitch değeri alınır.
        if (cameraHolder != null)
        {
            pitch =
                NormalizeAngle(cameraHolder.localEulerAngles.x);
        }

        SetLookEnabled(startEnabled);

        ApplyRotation();
    }

    // =====================================================
    // MOUSE LOOK INPUT KONTROLLERİ
    // =====================================================

    private void Update()
    {
        if (!canLook)
            return;

        Vector2 delta = Vector2.zero;

        // =====================================================
        // PC MOUSE INPUT
        // =====================================================

        if (!Application.isMobilePlatform)
        {
            if (Mouse.current == null)
                return;

            delta = Mouse.current.delta.ReadValue();
        }

        else
        {
            // =====================================================
            // JOYSTICK KONTROL
            // =====================================================

            if (useJoystick)
            {
                if (lookJoystick == null)
                    return;

                delta = new Vector2(
                    lookJoystick.Horizontal * 100f,
                    lookJoystick.Vertical * 100f
                );
            }

            // =====================================================
            // TOUCH DRAG KONTROL
            // =====================================================

            else
            {
                if (Touchscreen.current == null)
                    return;

                if (Touchscreen.current.primaryTouch.press.isPressed)
                {
                    delta =
                        Touchscreen.current.primaryTouch.delta.ReadValue();
                }
            }
        }
        // =====================================================
        // LOOK HESAPLAMALARI
        // =====================================================

        yawOffset += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;

        yawOffset = Mathf.Clamp(
            yawOffset,
            -horizontalLimit,
            horizontalLimit
        );

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        ApplyRotation();
    }

    // =====================================================
    // KARAKTER VE KAMERA ROTASYONUNU UYGULAMA
    // =====================================================

    private void ApplyRotation()
    {
        // Karakter yatay rotasyonu uygulanır.
        transform.localRotation =
            Quaternion.Euler(
                0f,
                baseYaw + yawOffset,
                0f
            );

        // Kamera dikey rotasyonu uygulanır.
        if (cameraHolder != null)
        {
            cameraHolder.localRotation =
                Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    // =====================================================
    // MOUSE LOOK AKTİF / PASİF DURUMU
    // =====================================================

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;

        // Gameplay modunda cursor kilitlenir.
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // UI modunda cursor serbest bırakılır.
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =====================================================
    // AÇIYI -180 / 180 ARALIĞINA NORMALLEŞTİRME
    // =====================================================

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        while (angle < -180f)
            angle += 360f;

        return angle;
    }

    // =====================================================
    // SENSITIVITY AYARI
    // =====================================================

    public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
    }
}