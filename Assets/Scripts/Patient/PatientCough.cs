using UnityEngine;

public class PatientCough : MonoBehaviour
{
    [Header("Cough Settings")]
    public Transform targetPoint;
    public float triggerDistance = 0.5f;

    private AudioSource audioSource;
    private Animator animator;
    private bool hasCoughed = false;

    // =====================================================
    // COMPONENT REFERANSLARINI ALMA
    // =====================================================

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        animator = GetComponent<Animator>();

        // Animator ana objede yoksa child objelerde aranır.
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // =====================================================
    // ÖKSÜRÜK TETİKLEME KONTROLLERİ
    // =====================================================

    void Update()
    {
        // SFX ses seviyesi AudioManager üzerinden güncellenir.
        if (audioSource != null && AudioManager.Instance != null)
        {
            audioSource.volume = AudioManager.Instance.sfxVolume;
        }

        // Öksürük bir kez tetiklendiyse tekrar çalışmaz.
        if (hasCoughed || targetPoint == null)
            return;

        // Y ekseni sabitlenerek sadece yatay mesafe hesaplanır.
        Vector3 patientPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetPos = new Vector3(targetPoint.position.x, 0f, targetPoint.position.z);

        float distance = Vector3.Distance(patientPos, targetPos);

        // =====================================================
        // ÖKSÜRÜK SESİ VE ANİMASYONUNU ÇALIŞTIRMA
        // =====================================================

        if (distance <= triggerDistance)
        {
            hasCoughed = true;

            // Öksürük sesi oynatılır.
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Animator referansı kaybolduysa tekrar alınır.
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Öksürük animasyonu tetiklenir.
            if (animator != null)
            {
                animator.ResetTrigger("Cough");
                animator.SetTrigger("Cough");
            }
        }
    }

    // =====================================================
    // ÖKSÜRÜK DURUMUNU SIFIRLAMA
    // =====================================================

    // Öksürük animasyonu ve sesi yeniden kullanılabilir hale getirilir.
    public void ResetCough()
    {
        hasCoughed = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Cough");
        }
    }
}