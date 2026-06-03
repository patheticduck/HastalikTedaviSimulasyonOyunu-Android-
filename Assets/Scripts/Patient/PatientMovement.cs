using UnityEngine;

public class PatientMovement : MonoBehaviour
{
    [Header("Movement")]
    public Transform startPoint;
    public Transform corridorPoint;
    public Transform doorPoint;
    public Transform insideDoorPoint;
    public Transform targetPoint;
    public float moveSpeed = 2f;
    public float stopDistance = 0.35f;

    [Header("Door")]
    public DoorAutoController doorController;
    public float doorTriggerDistance = 0.6f;

    [Header("Flow")]
    public GameFlowManager gameFlowManager;

    [Header("Patient Visuals")]
    public GameObject[] patientVisuals;

    private Animator animator;
    private bool canMove = false;
    private bool walkingIn = true;
    private bool doorOpened = false;
    private bool doorClosedAfterEntry = false;
    private bool doorClosedAfterExit = false;

    private int pathIndex = 0;
    private Vector3[] walkPath;

    // =====================================================
    // COMPONENT REFERANSLARINI ALMA
    // =====================================================

    void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // =====================================================
    // HASTA HAREKET VE YÖN KONTROLLERİ
    // =====================================================

    void Update()
    {
        if (!canMove || walkPath == null || walkPath.Length == 0)
            return;

        HandleDoorByDistance();

        Vector3 currentTarget = walkPath[pathIndex];

        Vector3 targetPosition = new Vector3(
            currentTarget.x,
            transform.position.y,
            currentTarget.z
        );

        // Hastayı mevcut hedef noktaya doğru hareket ettirir.
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Hastanın hareket yönüne dönmesini sağlar.
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction.normalized;

        // Hedef noktaya ulaşıldığında sıradaki noktaya geçilir.
        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            pathIndex++;

            if (pathIndex >= walkPath.Length)
                ArriveAtTarget();
        }
    }

    // =====================================================
    // KAPI AÇMA VE KAPATMA KONTROLLERİ
    // =====================================================

    private void HandleDoorByDistance()
    {
        if (doorController == null)
            return;

        // Odaya giriş sırasında çalışan kapı kontrolleri.
        if (walkingIn)
        {
            if (!doorOpened && doorPoint != null &&
                Vector3.Distance(transform.position, doorPoint.position) <= doorTriggerDistance)
            {
                doorController.OpenDoor();
                doorOpened = true;
            }

            if (!doorClosedAfterEntry && insideDoorPoint != null &&
                Vector3.Distance(transform.position, insideDoorPoint.position) <= doorTriggerDistance)
            {
                doorController.CloseDoor();
                doorClosedAfterEntry = true;
            }
        }

        // Odadan çıkış sırasında çalışan kapı kontrolleri.
        else
        {
            if (!doorOpened && insideDoorPoint != null &&
                Vector3.Distance(transform.position, insideDoorPoint.position) <= doorTriggerDistance)
            {
                doorController.OpenDoor();
                doorOpened = true;
            }

            if (!doorClosedAfterExit && corridorPoint != null &&
                Vector3.Distance(transform.position, corridorPoint.position) <= doorTriggerDistance)
            {
                doorController.CloseDoor();
                doorClosedAfterExit = true;
            }
        }
    }

    // =====================================================
    // HEDEF NOKTAYA ULAŞMA İŞLEMLERİ
    // =====================================================

    private void ArriveAtTarget()
    {
        canMove = false;
        PlayIdle();

        // Hasta koltuğa ulaştığında çalışır.
        if (walkingIn)
        {
            if (targetPoint != null)
                transform.rotation = targetPoint.rotation;

            if (gameFlowManager != null)
                gameFlowManager.OnPatientArrived();
        }

        // Hasta odadan çıktığında çalışır.
        else
        {
            if (doorController != null)
                doorController.CloseDoorInstant();

            gameObject.SetActive(false);

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();
        }
    }

    // =====================================================
    // HASTANIN ODAYA GİRİŞ HAREKETİNİ BAŞLATMA
    // =====================================================

    public void BeginWalk()
    {
        if (doorController != null)
            doorController.CloseDoorInstant();

        gameObject.SetActive(true);

        ResetPatientBehaviours();

        animator = GetComponentInChildren<Animator>();

        // Giriş yürüyüş yolu oluşturulur.
        if (startPoint != null && targetPoint != null)
        {
            transform.position = startPoint.position;
            transform.rotation = startPoint.rotation;

            walkPath = new Vector3[]
            {
                corridorPoint != null ? corridorPoint.position : startPoint.position,
                doorPoint != null ? doorPoint.position : targetPoint.position,
                insideDoorPoint != null ? insideDoorPoint.position : targetPoint.position,
                targetPoint.position
            };

            pathIndex = 0;
            LookAtCurrentTarget();
        }

        walkingIn = true;
        doorOpened = false;
        doorClosedAfterEntry = false;
        doorClosedAfterExit = false;

        canMove = true;
        PlayWalk();
    }

    // =====================================================
    // HASTANIN ODADAN ÇIKIŞ HAREKETİNİ BAŞLATMA
    // =====================================================

    public void BeginExit()
    {
        animator = GetComponentInChildren<Animator>();

        // Başlangıç noktası yoksa hasta direkt gizlenir.
        if (startPoint == null)
        {
            HidePatient();

            if (gameFlowManager != null)
                gameFlowManager.OnPatientExited();

            return;
        }

        // Çıkış yürüyüş yolu oluşturulur.
        walkPath = new Vector3[]
        {
            insideDoorPoint != null ? insideDoorPoint.position : targetPoint.position,
            doorPoint != null ? doorPoint.position : startPoint.position,
            corridorPoint != null ? corridorPoint.position : startPoint.position,
            startPoint.position
        };

        pathIndex = 0;
        walkingIn = false;

        doorOpened = false;
        doorClosedAfterEntry = false;
        doorClosedAfterExit = false;

        canMove = true;

        LookAtCurrentTarget();
        PlayWalk();
    }

    // =====================================================
    // HASTA DAVRANIŞLARINI SIFIRLAMA
    // =====================================================

    private void ResetPatientBehaviours()
    {
        PatientCough[] coughScripts = GetComponentsInChildren<PatientCough>(true);

        foreach (PatientCough cough in coughScripts)
        {
            if (cough != null)
                cough.ResetCough();
        }
    }

    // =====================================================
    // HASTAYI MEVCUT HEDEF NOKTASINA ÇEVİRME
    // =====================================================

    private void LookAtCurrentTarget()
    {
        if (walkPath == null || walkPath.Length == 0)
            return;

        Vector3 direction = walkPath[pathIndex] - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction.normalized;
    }

    // =====================================================
    // HASTA GÖRÜNÜRLÜK VE HAREKET KONTROLLERİ
    // =====================================================

    public void HidePatient()
    {
        canMove = false;
        PlayIdle();
        gameObject.SetActive(false);
    }

    public void StopMovementImmediately()
    {
        canMove = false;
        PlayIdle();
    }

    // =====================================================
    // HASTA MODELİNİ AKTİF ETME
    // =====================================================

    public void SetPatientVisual(int index)
    {
        if (patientVisuals == null || patientVisuals.Length == 0)
        {
            animator = GetComponentInChildren<Animator>();
            return;
        }

        int visualIndex = Mathf.Abs(index) % patientVisuals.Length;

        for (int i = 0; i < patientVisuals.Length; i++)
        {
            if (patientVisuals[i] != null)
                patientVisuals[i].SetActive(i == visualIndex);
        }

        animator = GetComponentInChildren<Animator>();
    }

    // =====================================================
    // HASTA ANİMASYON KONTROLLERİ
    // =====================================================

    private void PlayWalk()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.Play("walk", 0, 0f);
        }
    }

    private void PlayIdle()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.Play("Idle 0", 0, 0f);
        }
    }
}