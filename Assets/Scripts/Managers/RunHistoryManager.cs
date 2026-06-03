using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RunHistoryManager : MonoBehaviour
{
    // =====================================================
    // RUN GEÇMİŞİ YÖNETİMİ
    // =====================================================

    public static RunHistoryManager Instance;

    [Header("History Limit")]
    [SerializeField] private int maxHistoryCount = 5;

    [Header("Persistent Save")]
    [SerializeField] private bool usePersistentSave = true;

    private readonly List<byte[]> sessionHistoryBytes = new List<byte[]>();
    private readonly List<SessionHistoryData> cachedSessionHistoryList = new List<SessionHistoryData>();

    public IReadOnlyList<SessionHistoryData> SessionHistoryList => cachedSessionHistoryList;

    private string FilePath => Path.Combine(Application.persistentDataPath, "run_history.bytes");

    [Serializable]
    private class SessionHistoryJsonWrapper
    {
        public List<string> sessionsBase64 = new List<string>();
    }

    // =====================================================
    // BAŞLANGIÇ VE SINGLETON KONTROLÜ
    // =====================================================

    // Singleton yapısını kurar ve kayıtlı history verilerini yükler.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (usePersistentSave)
                LoadFromFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // SESSION HISTORY YÖNETİMİ
    // =====================================================

    // Yeni bir oyun geçmişini listeye ekler.
    public void AddSession(SessionHistoryData session)
    {
        if (session == null)
            return;

        byte[] sessionBytes = SerializeSessionToBytes(session);

        sessionHistoryBytes.Insert(0, sessionBytes);
        cachedSessionHistoryList.Insert(0, session);

        TrimHistoryLimit();
        SaveToFile();
    }

    // Belirtilen indexteki geçmiş kaydını siler.
    public void RemoveSessionAt(int index)
    {
        if (index < 0 || index >= sessionHistoryBytes.Count)
            return;

        sessionHistoryBytes.RemoveAt(index);

        if (index < cachedSessionHistoryList.Count)
            cachedSessionHistoryList.RemoveAt(index);

        SaveToFile();
    }

    // Herhangi bir kayıt olup olmadığını kontrol eder.
    public bool HasHistory()
    {
        return cachedSessionHistoryList.Count > 0;
    }

    // Tüm geçmiş listesini temizler.
    public void ClearHistory()
    {
        sessionHistoryBytes.Clear();
        cachedSessionHistoryList.Clear();

        DeleteHistoryFile();
    }

    // Tüm geçmişi tamamen sıfırlar.
    public void ClearAllHistory()
    {
        ClearHistory();
    }

    // =====================================================
    // SERIALIZE / DESERIALIZE İŞLEMLERİ
    // =====================================================

    // Session verisini byte dizisine dönüştürür.
    private byte[] SerializeSessionToBytes(SessionHistoryData session)
    {
        string json = JsonUtility.ToJson(session, true);
        return Encoding.UTF8.GetBytes(json);
    }

    // Byte dizisini tekrar Session verisine dönüştürür.
    private SessionHistoryData DeserializeBytesToSession(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        string json = Encoding.UTF8.GetString(bytes);
        return JsonUtility.FromJson<SessionHistoryData>(json);
    }

    // History listesini maksimum kayıt sınırında tutar.
    private void TrimHistoryLimit()
    {
        while (sessionHistoryBytes.Count > maxHistoryCount)
            sessionHistoryBytes.RemoveAt(sessionHistoryBytes.Count - 1);

        while (cachedSessionHistoryList.Count > maxHistoryCount)
            cachedSessionHistoryList.RemoveAt(cachedSessionHistoryList.Count - 1);
    }

    // =====================================================
    // DOSYA KAYDETME VE YÜKLEME
    // =====================================================

    // History verilerini persistent dosyaya kaydeder.
    private void SaveToFile()
    {
        if (!usePersistentSave)
            return;

        try
        {
            SessionHistoryJsonWrapper wrapper = new SessionHistoryJsonWrapper();

            foreach (byte[] bytes in sessionHistoryBytes)
            {
                if (bytes == null || bytes.Length == 0)
                    continue;

                wrapper.sessionsBase64.Add(Convert.ToBase64String(bytes));
            }

            string wrapperJson = JsonUtility.ToJson(wrapper, true);
            byte[] fileBytes = Encoding.UTF8.GetBytes(wrapperJson);

            File.WriteAllBytes(FilePath, fileBytes);
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyaya kaydedilemedi: " + e.Message);
        }
    }

    // Kayıtlı history verilerini dosyadan yükler.
    private void LoadFromFile()
    {
        if (!File.Exists(FilePath))
            return;

        try
        {
            byte[] fileBytes = File.ReadAllBytes(FilePath);
            string wrapperJson = Encoding.UTF8.GetString(fileBytes);

            SessionHistoryJsonWrapper wrapper =
                JsonUtility.FromJson<SessionHistoryJsonWrapper>(wrapperJson);

            sessionHistoryBytes.Clear();
            cachedSessionHistoryList.Clear();

            if (wrapper == null || wrapper.sessionsBase64 == null)
                return;

            foreach (string base64 in wrapper.sessionsBase64)
            {
                if (string.IsNullOrEmpty(base64))
                    continue;

                byte[] sessionBytes = Convert.FromBase64String(base64);
                SessionHistoryData session = DeserializeBytesToSession(sessionBytes);

                if (session == null)
                    continue;

                sessionHistoryBytes.Add(sessionBytes);
                cachedSessionHistoryList.Add(session);
            }

            TrimHistoryLimit();
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyadan okunamadı: " + e.Message);
            sessionHistoryBytes.Clear();
            cachedSessionHistoryList.Clear();
        }
    }

    // Kayıtlı history dosyasını diskten siler.
    private void DeleteHistoryFile()
    {
        if (!usePersistentSave)
            return;

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("History dosyası silinemedi: " + e.Message);
        }
    }
}