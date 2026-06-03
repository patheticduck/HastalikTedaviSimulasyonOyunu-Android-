using System.Collections.Generic;

[System.Serializable]
public class PatientHistoryData
{
    // =====================================================
    // HASTA SONUÇ BİLGİLERİ
    // =====================================================

    public string patientLabel;
    public string playerAnswer;
    public bool isCorrect;
    public string correctAnswer;
}

[System.Serializable]
public class DayHistoryData
{
    // =====================================================
    // GÜN GEÇMİŞİ VERİLERİ
    // =====================================================

    public int dayNumber;
    public List<PatientHistoryData> patients = new List<PatientHistoryData>();

    // =====================================================
    // GÜNLÜK DOĞRU / YANLIŞ HESAPLAMA
    // =====================================================

    public int TotalCorrect()
    {
        int count = 0;

        foreach (var patient in patients)
        {
            if (patient.isCorrect)
                count++;
        }

        return count;
    }

    public int TotalWrong()
    {
        int count = 0;

        foreach (var patient in patients)
        {
            if (!patient.isCorrect)
                count++;
        }

        return count;
    }
}

[System.Serializable]
public class SessionHistoryData
{
    // =====================================================
    // OTURUM GENEL BİLGİLERİ
    // =====================================================

    public string completedDateText;
    public string relativeTimeText;
    public long completedAtTicks;

    public string selectedDoctorName;

    // =====================================================
    // GÜN GEÇMİŞİ LİSTESİ
    // =====================================================

    public List<DayHistoryData> days = new List<DayHistoryData>();

    // =====================================================
    // TOPLAM DOĞRU / YANLIŞ HESAPLAMA
    // =====================================================

    public int TotalCorrect()
    {
        int total = 0;

        foreach (var day in days)
        {
            total += day.TotalCorrect();
        }

        return total;
    }

    public int TotalWrong()
    {
        int total = 0;

        foreach (var day in days)
        {
            total += day.TotalWrong();
        }

        return total;
    }
}