using UnityEngine;
using UnityEngine.UI;

public class SensitivitySliderUI : MonoBehaviour
{
    [Header("References")]
    public Slider sensitivitySlider;

    public MouseLookLimited mouseLook;

    private void Start()
    {
        if (mouseLook == null || sensitivitySlider == null)
            return;

        sensitivitySlider.value = mouseLook.mouseSensitivity;

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        mouseLook.SetSensitivity(value);
    }
}