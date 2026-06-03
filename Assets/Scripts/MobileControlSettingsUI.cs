using UnityEngine;
using UnityEngine.UI;

public class MobileControlSettingsUI : MonoBehaviour
{
    [Header("References")]
    public Toggle joystickToggle;

    public MouseLookLimited mouseLook;

    public GameObject joystickUI;

    // =====================================================
    // BAŞLANGIÇ AYARLARI
    // =====================================================

    private void Start()
    {
        if (mouseLook == null)
            return;

        // Toggle başlangıç durumunu mevcut ayardan alır.
        if (joystickToggle != null)
        {
            joystickToggle.isOn = mouseLook.useJoystick;

            joystickToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        UpdateJoystickUI();
    }

    // =====================================================
    // TOGGLE DURUM DEĞİŞİMİ
    // =====================================================

    private void OnToggleChanged(bool value)
    {
        if (mouseLook != null)
        {
            mouseLook.useJoystick = value;
        }

        UpdateJoystickUI();
    }

    // =====================================================
    // JOYSTICK UI GÖSTER / GİZLE
    // =====================================================

    private void UpdateJoystickUI()
    {
        if (joystickUI != null && mouseLook != null)
        {
            joystickUI.SetActive(mouseLook.useJoystick);
        }
    }
}