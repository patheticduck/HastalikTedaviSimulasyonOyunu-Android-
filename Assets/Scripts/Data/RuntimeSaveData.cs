using System.Collections.Generic;

[System.Serializable]
public class RuntimeSaveData
{
    // =====================================================
    // OYUN İLERLEME VERİLERİ
    // =====================================================

    public int currentDay;
    public int currentPatientNumber;
    public int totalCorrect;
    public int totalWrong;
    public int remainingHearts;

    // =====================================================
    // ENERJİ VE DOCTOR BOOK DURUMU
    // =====================================================

    public int maxEnergy;
    public int currentEnergy;
    public bool doctorBookUnlockedForCurrentPatient;

    // =====================================================
    // GÜNLÜK SONUÇ VERİLERİ
    // =====================================================

    public int dayCorrect;
    public int dayWrong;

    // =====================================================
    // HASTA VE TANI DURUMU
    // =====================================================

    public bool diagnosisSubmittedForCurrentPatient;
    public bool patientResultPanelPendingAfterLoad;

    public bool lastDiagnosisWasCorrect;
    public string lastDiagnosisResultText;
    public string lastCorrectDiagnosisName;

    public bool pendingLoseGameOver;

    // =====================================================
    // SAVE VE DOKTOR BİLGİLERİ
    // =====================================================

    public bool hasSave;

    public DoctorSpecialization selectedDoctorSpecialization = DoctorSpecialization.None;

    // Aynı oyun döngüsünü ayırt etmek için kullanılır.
    public string runId;

    // =====================================================
    // SAVE TARİH BİLGİLERİ
    // =====================================================

    public string savedDateText;
    public long savedAtTicks;

    // =====================================================
    // PLANLANAN HASTA SIRASI
    // =====================================================

    public List<string> plannedPatientCaseIds = new List<string>();

    // =====================================================
    // HASTA ANALİZ GEÇMİŞİ
    // =====================================================

    public List<PatientAnalysisEntry> patientAnalysisHistory = new List<PatientAnalysisEntry>();
}