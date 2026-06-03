using UnityEngine;

public class FanRotate : MonoBehaviour
{
    public float speed = 500f;

    // =====================================================
    // FAN DÖNÜŞ ANİMASYONU
    // =====================================================

    void Update()
    {
        // Fan objesi sürekli kendi ekseni etrafında döndürülür.
        transform.localRotation *= Quaternion.Euler(
            0f,
            speed * Time.deltaTime,
            0f
        );
    }
}