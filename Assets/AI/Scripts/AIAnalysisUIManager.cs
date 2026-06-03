using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIAnalysisUIManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject aiPanel;
    public TMP_Text resultText;
    public Button closeButton;

    [Header("References")]
    public DiagnosisAI diagnosisAI;
    public DialogueManager dialogueManager;

    private Action externalCloseAction;

    // =====================================================
    // REFERANS VE PANEL AYARLARI
    // =====================================================

    private void Start()
    {
        // Referanslar otomatik alınır.
        if (diagnosisAI == null)
            diagnosisAI = FindFirstObjectByType<DiagnosisAI>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        // AI paneli başlangıçta kapatılır.
        if (aiPanel != null)
            aiPanel.SetActive(false);

        // Kapatma butonu event bağlantısı yapılır.
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    // =====================================================
    // DIŞARIDAN PANEL KAPATMA AKSİYONU
    // =====================================================

    public void SetExternalCloseAction(Action closeAction)
    {
        externalCloseAction = closeAction;
    }

    // =====================================================
    // PANEL AÇMA / KAPATMA İŞLEMLERİ
    // =====================================================

    public void OpenPanel()
    {
        if (aiPanel != null)
            aiPanel.SetActive(true);

        AnalyzeCurrentPatient();
    }

    public void ClosePanel()
    {
        // Dışarıdan özel kapanış aksiyonu varsa çalıştırılır.
        if (externalCloseAction != null)
        {
            externalCloseAction.Invoke();
            return;
        }

        ClosePanelOnly();
    }

    public void ClosePanelOnly()
    {
        if (aiPanel != null)
            aiPanel.SetActive(false);
    }

    // =====================================================
    // AKTİF HASTA İÇİN AI ANALİZİ
    // =====================================================

    public void AnalyzeCurrentPatient()
    {
        // Referanslar yeniden alınır.
        if (diagnosisAI == null)
            diagnosisAI = FindFirstObjectByType<DiagnosisAI>();

        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // Bağlantı eksikse hata mesajı gösterilir.
        if (diagnosisAI == null || dialogueManager == null)
        {
            if (resultText != null)
                resultText.text = "AI bağlantısı eksik.";

            Debug.LogWarning("AI bağlantısı eksik.");
            return;
        }

        // Hastadan toplanan AI feature listesi alınır.
        List<string> features =
            dialogueManager.GetAIFeatures();

        // Yeterli veri yoksa kullanıcı bilgilendirilir.
        if (features.Count == 0)
        {
            if (resultText != null)
            {
                resultText.text =
                    "Henüz yeterli veri yok.\n\n" +
                    "Önce hastaya birkaç soru sorun veya test yapın.";
            }

            return;
        }

        // AI tahminleri hesaplanır.
        List<DiagnosisPrediction> results =
            diagnosisAI.Predict(features);

        // Sonuç üretilemediyse hata mesajı gösterilir.
        if (results == null || results.Count == 0)
        {
            if (resultText != null)
            {
                resultText.text =
                    "AI tahmin üretemedi.\n\n" +
                    "Model JSON, feature eşleşmesi veya DiagnosisAI bağlantısı kontrol edilmeli.";
            }

            Debug.LogWarning(
                "AI tahmin üretemedi. Results boş döndü."
            );

            return;
        }

        // Normalize edilmiş feature seti oluşturulur.
        HashSet<string> normalizedFeatures =
            BuildNormalizedFeatureSet(features);

        // İlk 4 hastalık tahmini alınır.
        List<string> topDiseases =
            GetTopDiseaseIds(results, 4);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Öne Çıkan Tanılar:\n");

        int maxResultCount =
            Mathf.Min(4, results.Count);

        // İlk 4 tahmin kullanıcıya gösterilir.
        for (int i = 0; i < maxResultCount; i++)
        {
            var r = results[i];

            sb.AppendLine(
                $"{i + 1}. {GetDisplayName(r.disease)} — {GetRankLabel(i)}"
            );
        }

        // Klinik yorum oluşturulur.
        sb.AppendLine("\nKlinik Yorum:");
        sb.AppendLine(
            BuildClinicalComment(
                results,
                normalizedFeatures,
                topDiseases
            )
        );

        // Öneri sistemi oluşturulur.
        string suggestion =
            BuildSuggestion(
                normalizedFeatures,
                topDiseases
            );

        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            sb.AppendLine("\nÖneri:");
            sb.AppendLine(suggestion);
        }

        // AI uyarısı eklenir.
        sb.AppendLine("\nUyarı:");
        sb.AppendLine(
            "Bu sonuç kesin tanı değildir. Toplanan bulgulara göre oluşturulmuş ön değerlendirmedir."
        );

        if (resultText != null)
            resultText.text = sb.ToString();
    }

    // =====================================================
    // NORMALIZED FEATURE SET OLUŞTURMA
    // =====================================================

    private HashSet<string> BuildNormalizedFeatureSet(
        List<string> rawFeatures)
    {
        HashSet<string> set =
            new HashSet<string>();

        foreach (string feature in rawFeatures)
        {
            if (string.IsNullOrWhiteSpace(feature))
                continue;

            string trimmed = feature.Trim();

            string normalized =
                AIFeatureNormalizer.Normalize(trimmed);

            if (!string.IsNullOrWhiteSpace(trimmed))
                set.Add(trimmed);

            if (!string.IsNullOrWhiteSpace(normalized))
                set.Add(normalized);
        }

        return set;
    }

    // =====================================================
    // EN YÜKSEK TAHMİN EDİLEN HASTALIKLARI ALMA
    // =====================================================

    private List<string> GetTopDiseaseIds(
        List<DiagnosisPrediction> results,
        int count)
    {
        List<string> diseaseIds =
            new List<string>();

        int maxCount =
            Mathf.Min(count, results.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (results[i] != null &&
                !string.IsNullOrWhiteSpace(results[i].disease))
            {
                diseaseIds.Add(results[i].disease);
            }
        }

        return diseaseIds;
    }

    // =====================================================
    // KLINIK YORUM OLUŞTURMA
    // =====================================================

    private string BuildClinicalComment(
        List<DiagnosisPrediction> results,
        HashSet<string> features,
        List<string> topDiseases)
    {
        // İlk iki sonuç birbirine yakınsa belirsizlik yorumu yapılır.
        if (results.Count >= 2 &&
            AreTopResultsClose(results[0], results[1]))
        {
            return
                "Bulgular birden fazla tanı ile örtüşüyor. AI kesin ayrım yapamıyor.";
        }

        // Solunum hastalıkları yorumları.
        if (ContainsAnyDisease(
            topDiseases,
            "GRIP",
            "COVID19",
            "COLD",
            "PHARYNGITIS",
            "BRONCHITIS",
            "PNEUMONIA"))
        {
            if (HasAnyFeature(features, "HAS_FEVER") &&
                HasAnyFeature(features, "HAS_COUGH"))
            {
                return
                    "Ateş ve öksürük solunum yolu enfeksiyonu ile uyumlu. Bulgular benzer hastalıklarla örtüşebilir.";
            }

            if (HasAnyFeature(features, "HAS_THROAT_PAIN") &&
                HasAnyFeature(features, "HAS_RUNNY_NOSE"))
            {
                return
                    "Boğaz ve burun bulguları üst solunum yolu hastalıklarıyla örtüşüyor.";
            }

            return
                "Solunum yolu bulguları öne çıkıyor. Ayırıcı tanılar birlikte değerlendirilmelidir.";
        }

        // Gastrointestinal hastalık yorumları.
        if (ContainsAnyDisease(
            topDiseases,
            "GERD",
            "GASTRITIS",
            "PEPTIC_ULCER",
            "GALLBLADDER",
            "PANCREATITIS",
            "CROHN"))
        {
            if (HasAnyFeature(features, "EPIGASTRIC_PAIN", "REFLUX"))
            {
                return
                    "Üst karın ve mide-bağırsak sistemi bulguları öne çıkıyor.";
            }

            if (HasAnyFeature(features, "DIARRHEA", "CALPRO_HIGH"))
            {
                return
                    "Bağırsak bulguları dikkat çekiyor. Ayırıcı tanılar birlikte değerlendirilmelidir.";
            }

            return
                "Gastrointestinal sistemle ilişkili tanılar öne çıkıyor.";
        }

        // Sistemik hastalık yorumları.
        if (ContainsAnyDisease(
            topDiseases,
            "LUPUS",
            "LYME",
            "HYPERTHYROIDISM"))
        {
            return
                "Sistemik bulgular birden fazla hastalıkla örtüşebilir. Mevcut bulgular birlikte değerlendirilmelidir.";
        }

        return
            "Bulgular mevcut tanılarla karşılaştırılarak ön değerlendirme oluşturuldu.";
    }

    // =====================================================
    // TEST VE MUAYENE ÖNERİLERİ
    // =====================================================

    private string BuildSuggestion(
        HashSet<string> features,
        List<string> topDiseases)
    {
        List<string> suggestions =
            new List<string>();

        // Yapılmış test kontrolleri.
        bool covidAlreadyChecked =
            HasAnyFeature(
                features,
                "COVID_POSITIVE",
                "COVID_NEGATIVE",
                "covid_positive",
                "covid_negative",
                "covid_test_positive"
            );

        bool fluAlreadyChecked =
            HasAnyFeature(
                features,
                "FLU_NEGATIVE",
                "flu_negative"
            );

        bool xrayAlreadyChecked =
            HasAnyFeature(
                features,
                "XRAY_INFILTRATION",
                "NO_XRAY_INFILTRATION",
                "xray_infiltration",
                "xray_no_infiltration",
                "xray_no_infiltrate",
                "no_infiltration",
                "right_lower_lobe_infiltrate"
            );

        bool hPyloriAlreadyChecked =
            HasAnyFeature(
                features,
                "HPYLORI_POSITIVE",
                "HPYLORI_NEGATIVE",
                "hp_pos",
                "hp_neg",
                "hpylori_positive",
                "hpylori_negative"
            );

        bool gallbladderAlreadyChecked =
            HasAnyFeature(
                features,
                "GALLBLADDER_SIGN",
                "GALLSTONE_PRESENT",
                "murphy_pos",
                "fat_meal",
                "fat_food",
                "usg_clean",
                "usg_no_stone",
                "no_stone"
            );

        bool pancreasAlreadyChecked =
            HasAnyFeature(
                features,
                "LIPASE_HIGH",
                "HIGH_LIPASE",
                "HIGH_AMYLASE",
                "lipase_high",
                "amylase_high"
            );

        bool crohnAlreadyChecked =
            HasAnyFeature(
                features,
                "CALPRO_HIGH",
                "calpro_high",
                "Kolonoskopi",
                "ileum_ulcer",
                "skip_lesion"
            );

        bool lymeAlreadyChecked =
            HasAnyFeature(
                features,
                "LYME_POSITIVE",
                "lyme_pos"
            );

        bool lupusAlreadyChecked =
            HasAnyFeature(
                features,
                "AUTOIMMUNE_POSITIVE",
                "ana_pos",
                "ana_lowpos",
                "anti_ds_pos",
                "c3_low"
            );

        bool thyroidAlreadyChecked =
            HasAnyFeature(
                features,
                "THYROID_HIGH",
                "HYPERTHYROID_PATTERN",
                "ft3_high",
                "ft4_high",
                "tsh_low"
            );

        // Hastalıklara göre öneri sistemi.
        if (topDiseases.Contains("COVID19") &&
            !covidAlreadyChecked &&
            HasAnyFeature(
                features,
                "HAS_FEVER",
                "HAS_COUGH",
                "HAS_FATIGUE"))
        {
            suggestions.Add(
                "Hızlı COVID testi ayırıcı tanı için faydalı olabilir."
            );
        }

        if (topDiseases.Contains("GRIP") &&
            !fluAlreadyChecked &&
            HasAnyFeature(
                features,
                "HAS_FEVER",
                "HAS_MYALGIA",
                "chills",
                "chills_yes"))
        {
            suggestions.Add(
                "Grip testi veya sistemik belirti sorgusu ayırıcı tanıyı destekleyebilir."
            );
        }

        if (topDiseases.Contains("PNEUMONIA") &&
            !xrayAlreadyChecked)
        {
            suggestions.Add(
                "Akciğer grafisi ve solunum muayenesi değerlendirmeyi güçlendirebilir."
            );
        }

        if (topDiseases.Contains("BRONCHITIS") &&
            !HasAnyFeature(
                features,
                "RHONCHI",
                "WHEEZING",
                "mild_rhonchi",
                "diffuse_rhonchi",
                "mild_wheezing"))
        {
            suggestions.Add(
                "Akciğer oskültasyonu bronşit ayrımı için yardımcı olabilir."
            );
        }

        if (topDiseases.Contains("PHARYNGITIS") &&
            !HasAnyFeature(
                features,
                "HAS_THROAT_PAIN",
                "throat_exam",
                "throat_exam_done",
                "oropharyngeal_exam",
                "tonsil_exam"))
        {
            suggestions.Add(
                "Boğaz muayenesi ve yutma ağrısı sorgusu değerlendirilebilir."
            );
        }

        if (ContainsAnyDisease(
            topDiseases,
            "GERD",
            "GASTRITIS",
            "PEPTIC_ULCER") &&
            !hPyloriAlreadyChecked)
        {
            suggestions.Add(
                "H. pylori testi veya üst gastrointestinal değerlendirme düşünülebilir."
            );
        }

        if (topDiseases.Contains("GALLBLADDER") &&
            !gallbladderAlreadyChecked)
        {
            suggestions.Add(
                "Sağ üst karın muayenesi ve ultrason değerlendirmesi faydalı olabilir."
            );
        }

        if (topDiseases.Contains("PANCREATITIS") &&
            !pancreasAlreadyChecked)
        {
            suggestions.Add(
                "Lipaz/amilaz sonucu pankreatit ayrımını güçlendirebilir."
            );
        }

        if (topDiseases.Contains("CROHN") &&
            !crohnAlreadyChecked)
        {
            suggestions.Add(
                "Kalprotektin veya kolonoskopi bulguları ayırıcı tanıda yardımcı olabilir."
            );
        }

        if (topDiseases.Contains("LYME") &&
            !lymeAlreadyChecked)
        {
            suggestions.Add(
                "Lyme testi ve kene teması öyküsü değerlendirilebilir."
            );
        }

        if (topDiseases.Contains("LUPUS") &&
            !lupusAlreadyChecked)
        {
            suggestions.Add(
                "Otoimmün belirteçler ve sistemik bulgular birlikte değerlendirilmelidir."
            );
        }

        if (topDiseases.Contains("HYPERTHYROIDISM") &&
            !thyroidAlreadyChecked)
        {
            suggestions.Add(
                "Tiroid hormon paneli değerlendirmeyi güçlendirebilir."
            );
        }

        // Öneri yoksa varsayılan mesaj döndürülür.
        if (suggestions.Count == 0)
        {
            return
                "Mevcut bulgular ayırıcı tanılarla birlikte değerlendirilebilir.";
        }

        return "• " + string.Join("\n• ", suggestions);
    }

    // =====================================================
    // FEATURE KONTROLLERİ
    // =====================================================

    private bool HasAnyFeature(
        HashSet<string> features,
        params string[] targets)
    {
        foreach (string target in targets)
        {
            if (features.Contains(target))
                return true;
        }

        return false;
    }

    // =====================================================
    // HASTALIK KONTROLLERİ
    // =====================================================

    private bool ContainsAnyDisease(
        List<string> diseases,
        params string[] targets)
    {
        foreach (string target in targets)
        {
            if (diseases.Contains(target))
                return true;
        }

        return false;
    }

    // =====================================================
    // SONUÇ YAKINLIK KONTROLÜ
    // =====================================================

    private bool AreTopResultsClose(
        DiagnosisPrediction first,
        DiagnosisPrediction second)
    {
        if (first == null || second == null)
            return false;

        return
            Math.Abs(first.probability - second.probability) < 0.15;
    }

    // =====================================================
    // SIRALAMA ETİKETLERİ
    // =====================================================

    private string GetRankLabel(int index)
    {
        if (index == 0)
            return "Öne çıkıyor";

        if (index == 1)
            return "Benzer bulgular var";

        return "Ayırıcı tanıda";
    }

    // =====================================================
    // HASTALIK İSİM ÇEVİRİLERİ
    // =====================================================

    private string GetDisplayName(string disease)
    {
        switch (disease)
        {
            case "GRIP": return "Grip";
            case "COVID19": return "COVID-19";
            case "COLD": return "Soğuk Algınlığı";
            case "PHARYNGITIS": return "Farenjit";
            case "BRONCHITIS": return "Bronşit";
            case "PNEUMONIA": return "Zatürre";

            case "GERD": return "Reflü";
            case "GASTRITIS": return "Gastrit";
            case "PEPTIC_ULCER": return "Peptik Ülser";
            case "GALLBLADDER": return "Safra Kesesi Hastalığı";
            case "PANCREATITIS": return "Pankreatit";
            case "CROHN": return "Crohn Hastalığı";

            case "LUPUS": return "Lupus";
            case "LYME": return "Lyme Hastalığı";
            case "HYPERTHYROIDISM": return "Hipertiroidi";

            default: return disease;
        }
    }
}