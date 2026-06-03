using UnityEngine;

public class DoctorProfileManager : MonoBehaviour
{
    public static DoctorProfileManager Instance { get; private set; }

    [Header("Selected Doctor Profile")]
    public DoctorSpecialization selectedSpecialization = DoctorSpecialization.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectSpecialization(DoctorSpecialization specialization)
    {
        selectedSpecialization = specialization;
    }

    public DoctorSpecialization GetSelectedSpecialization()
    {
        return selectedSpecialization;
    }

    public bool HasSpecialization(DoctorSpecialization specialization)
    {
        return selectedSpecialization == specialization;
    }

    public string GetSelectedDoctorDisplayName()
    {
        return GetDoctorDisplayName(selectedSpecialization);
    }

    public static string GetDoctorDisplayName(DoctorSpecialization specialization)
    {
        switch (specialization)
        {
            case DoctorSpecialization.Respiratory:
                return "Solunum Uzmanı";

            case DoctorSpecialization.Gastro:
                return "Gastro Uzmanı";

            case DoctorSpecialization.Generalist:
                return "Genel Hekim";

            default:
                return "Seçilmedi";
        }
    }
}