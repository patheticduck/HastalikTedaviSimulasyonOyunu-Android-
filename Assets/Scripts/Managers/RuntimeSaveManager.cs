using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RuntimeSaveManager : MonoBehaviour
{
    // =====================================================
    // RUNTIME SAVE YÖNETİMİ
    // =====================================================

    public static RuntimeSaveManager Instance;

    [Header("Current Runtime Save")]
    public RuntimeSaveData currentSave;

    [Header("Continue Save Settings")]
    [SerializeField] private int maxContinueSaveCount = 5;
    [SerializeField] private bool usePersistentContinueSaves = true;

    private readonly List<byte[]> continueSaveBytesList = new List<byte[]>();
    private readonly List<RuntimeSaveData> cachedContinueSaveList = new List<RuntimeSaveData>();

    public IReadOnlyList<RuntimeSaveData> ContinueSaveList => cachedContinueSaveList;

    private string FilePath => Path.Combine(Application.persistentDataPath, "continue_saves.bytes");

    [Serializable]
    private class RuntimeSaveJsonWrapper
    {
        public List<string> savesBase64 = new List<string>();
    }

    // =====================================================
    // BAŞLANGIÇ VE SINGLETON KONTROLÜ
    // =====================================================

    // Singleton yapısını oluşturur ve kayıtlı continue save verilerini yükler.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentSave = new RuntimeSaveData();

            if (usePersistentContinueSaves)
                LoadContinueSavesFromFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // SAVE OLUŞTURMA VE YÖNETİMİ
    // =====================================================

    // Aktif oyun akışından runtime save verisi oluşturur.
    public void SaveGame(GameFlowManager gameFlow)
    {
        if (gameFlow == null)
        {
            Debug.LogWarning("RuntimeSaveManager -> GameFlowManager null, kayıt alınamadı.");
            return;
        }

        currentSave = gameFlow.BuildRuntimeSaveData();
        currentSave.hasSave = true;

        AddContinueSave(currentSave);
    }

    // Mevcut aktif save verisini manuel olarak ayarlar.
    public void SetCurrentSave(RuntimeSaveData save)
    {
        if (save == null)
        {
            currentSave = new RuntimeSaveData();
            return;
        }

        currentSave = CloneRuntimeSave(save);
        currentSave.hasSave = true;
    }

    // Aktif runtime save verisini temizler.
    public void ClearSave()
    {
        currentSave = new RuntimeSaveData();
    }

    // Herhangi bir continue save kaydı olup olmadığını kontrol eder.
    public bool HasAnyContinueSave()
    {
        return cachedContinueSaveList.Count > 0;
    }

    // Yeni bir continue save kaydı ekler veya mevcut run kaydını günceller.
    public void AddContinueSave(RuntimeSaveData save)
    {
        if (save == null)
            return;

        RuntimeSaveData clonedSave = CloneRuntimeSave(save);
        clonedSave.hasSave = true;

        if (string.IsNullOrEmpty(clonedSave.runId))
            clonedSave.runId = Guid.NewGuid().ToString();

        DateTime now = DateTime.Now;
        clonedSave.savedDateText = now.ToString("dd.MM.yyyy HH:mm");
        clonedSave.savedAtTicks = now.Ticks;

        byte[] saveBytes = SerializeSaveToBytes(clonedSave);

        int existingIndex = FindContinueSaveIndexByRunId(clonedSave.runId);

        if (existingIndex >= 0)
        {
            continueSaveBytesList.RemoveAt(existingIndex);
            cachedContinueSaveList.RemoveAt(existingIndex);

            continueSaveBytesList.Insert(0, saveBytes);
            cachedContinueSaveList.Insert(0, clonedSave);
        }
        else
        {
            continueSaveBytesList.Insert(0, saveBytes);
            cachedContinueSaveList.Insert(0, clonedSave);
        }

        TrimContinueSaveLimit();
        SaveContinueSavesToFile();
    }

    // Belirtilen indexteki continue save kaydını döndürür.
    public RuntimeSaveData GetContinueSaveAt(int index)
    {
        if (index < 0 || index >= cachedContinueSaveList.Count)
            return null;

        return CloneRuntimeSave(cachedContinueSaveList[index]);
    }

    // Belirtilen indexteki continue save kaydını siler.
    public void RemoveContinueSaveAt(int index)
    {
        if (index < 0 || index >= cachedContinueSaveList.Count)
            return;

        if (index < continueSaveBytesList.Count)
            continueSaveBytesList.RemoveAt(index);

        cachedContinueSaveList.RemoveAt(index);

        SaveContinueSavesToFile();
    }

    // Run ID üzerinden ilgili continue save kaydını siler.
    public void RemoveContinueSaveByRunId(string runId)
    {
        int index = FindContinueSaveIndexByRunId(runId);

        if (index >= 0)
            RemoveContinueSaveAt(index);
    }

    // Tüm continue save kayıtlarını temizler.
    public void ClearAllContinueSaves()
    {
        continueSaveBytesList.Clear();
        cachedContinueSaveList.Clear();

        DeleteContinueSaveFile();

        ClearSave();
    }

    // =====================================================
    // SAVE ARAMA VE SERIALIZE İŞLEMLERİ
    // =====================================================

    // Verilen run ID için kayıtlı save indexini bulur.
    private int FindContinueSaveIndexByRunId(string runId)
    {
        if (string.IsNullOrEmpty(runId))
            return -1;

        for (int i = 0; i < cachedContinueSaveList.Count; i++)
        {
            RuntimeSaveData save = cachedContinueSaveList[i];

            if (save == null)
                continue;

            if (save.runId == runId)
                return i;
        }

        return -1;
    }

    // Runtime save verisini byte dizisine dönüştürür.
    private byte[] SerializeSaveToBytes(RuntimeSaveData save)
    {
        string json = JsonUtility.ToJson(save, true);
        return Encoding.UTF8.GetBytes(json);
    }

    // Byte dizisini tekrar runtime save verisine dönüştürür.
    private RuntimeSaveData DeserializeBytesToSave(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        string json = Encoding.UTF8.GetString(bytes);
        return JsonUtility.FromJson<RuntimeSaveData>(json);
    }

    // Continue save listesini maksimum limitte tutar.
    private void TrimContinueSaveLimit()
    {
        while (continueSaveBytesList.Count > maxContinueSaveCount)
            continueSaveBytesList.RemoveAt(continueSaveBytesList.Count - 1);

        while (cachedContinueSaveList.Count > maxContinueSaveCount)
            cachedContinueSaveList.RemoveAt(cachedContinueSaveList.Count - 1);
    }

    // =====================================================
    // DOSYA KAYDETME VE YÜKLEME
    // =====================================================

    // Continue save listesini persistent dosyaya kaydeder.
    private void SaveContinueSavesToFile()
    {
        if (!usePersistentContinueSaves)
            return;

        try
        {
            RuntimeSaveJsonWrapper wrapper = new RuntimeSaveJsonWrapper();

            foreach (byte[] bytes in continueSaveBytesList)
            {
                if (bytes == null || bytes.Length == 0)
                    continue;

                wrapper.savesBase64.Add(Convert.ToBase64String(bytes));
            }

            string wrapperJson = JsonUtility.ToJson(wrapper, true);
            byte[] fileBytes = Encoding.UTF8.GetBytes(wrapperJson);

            File.WriteAllBytes(FilePath, fileBytes);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Continue saves dosyaya kaydedilemedi: " + e.Message);
        }
    }

    // Persistent dosyadan continue save kayıtlarını yükler.
    private void LoadContinueSavesFromFile()
    {
        if (!File.Exists(FilePath))
            return;

        try
        {
            byte[] fileBytes = File.ReadAllBytes(FilePath);
            string wrapperJson = Encoding.UTF8.GetString(fileBytes);

            RuntimeSaveJsonWrapper wrapper =
                JsonUtility.FromJson<RuntimeSaveJsonWrapper>(wrapperJson);

            continueSaveBytesList.Clear();
            cachedContinueSaveList.Clear();

            if (wrapper == null || wrapper.savesBase64 == null)
                return;

            foreach (string base64 in wrapper.savesBase64)
            {
                if (string.IsNullOrEmpty(base64))
                    continue;

                byte[] saveBytes = Convert.FromBase64String(base64);
                RuntimeSaveData save = DeserializeBytesToSave(saveBytes);

                if (save == null)
                    continue;

                save.hasSave = true;

                continueSaveBytesList.Add(saveBytes);
                cachedContinueSaveList.Add(save);
            }

            TrimContinueSaveLimit();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Continue saves dosyadan okunamadı: " + e.Message);
            continueSaveBytesList.Clear();
            cachedContinueSaveList.Clear();
        }
    }

    // Persistent continue save dosyasını diskten siler.
    private void DeleteContinueSaveFile()
    {
        if (!usePersistentContinueSaves)
            return;

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Continue save dosyası silinemedi: " + e.Message);
        }
    }

    // =====================================================
    // SAVE CLONE İŞLEMLERİ
    // =====================================================

    // Runtime save verisinin güvenli bir kopyasını oluşturur.
    private RuntimeSaveData CloneRuntimeSave(RuntimeSaveData source)
    {
        if (source == null)
            return null;

        RuntimeSaveData clone = new RuntimeSaveData();

        clone.currentDay = source.currentDay;
        clone.currentPatientNumber = source.currentPatientNumber;
        clone.totalCorrect = source.totalCorrect;
        clone.totalWrong = source.totalWrong;
        clone.remainingHearts = source.remainingHearts;

        clone.maxEnergy = source.maxEnergy;
        clone.currentEnergy = source.currentEnergy;
        clone.doctorBookUnlockedForCurrentPatient = source.doctorBookUnlockedForCurrentPatient;

        clone.dayCorrect = source.dayCorrect;
        clone.dayWrong = source.dayWrong;

        clone.diagnosisSubmittedForCurrentPatient = source.diagnosisSubmittedForCurrentPatient;
        clone.patientResultPanelPendingAfterLoad = source.patientResultPanelPendingAfterLoad;

        clone.lastDiagnosisWasCorrect = source.lastDiagnosisWasCorrect;
        clone.lastDiagnosisResultText = source.lastDiagnosisResultText;
        clone.lastCorrectDiagnosisName = source.lastCorrectDiagnosisName;

        clone.pendingLoseGameOver = source.pendingLoseGameOver;

        clone.hasSave = source.hasSave;
        clone.selectedDoctorSpecialization = source.selectedDoctorSpecialization;

        clone.runId = source.runId;

        clone.savedDateText = source.savedDateText;
        clone.savedAtTicks = source.savedAtTicks;

        // Planlanan hasta sırasını kopyalar.
        clone.plannedPatientCaseIds = new List<string>();
        if (source.plannedPatientCaseIds != null)
        {
            foreach (string id in source.plannedPatientCaseIds)
                clone.plannedPatientCaseIds.Add(id);
        }

        // Hasta analiz geçmişini kopyalar.
        clone.patientAnalysisHistory = new List<PatientAnalysisEntry>();
        if (source.patientAnalysisHistory != null)
        {
            foreach (PatientAnalysisEntry entry in source.patientAnalysisHistory)
            {
                if (entry == null)
                    continue;

                clone.patientAnalysisHistory.Add(new PatientAnalysisEntry
                {
                    dayNumber = entry.dayNumber,
                    patientNumberInDay = entry.patientNumberInDay,
                    globalPatientNumber = entry.globalPatientNumber,
                    isCorrect = entry.isCorrect,
                    correctDiagnosisName = entry.correctDiagnosisName,
                    playerDiagnosisName = entry.playerDiagnosisName
                });
            }
        }

        return clone;
    }
}