using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiagnosisAI : MonoBehaviour
{
    [Header("Model JSON")]
    public TextAsset modelJson;

    [Header("Probability Calibration")]
    [SerializeField] private double softmaxTemperature = 4.0;

    private AIModelData modelData;

    // =====================================================
    // MODEL YÜKLEME
    // =====================================================

    private void Awake()
    {
        LoadModel();
    }

    private void LoadModel()
    {
        // Model JSON atanmadıysa hata verilir.
        if (modelJson == null)
        {
            Debug.LogError("DiagnosisAI: modelJson atanmadı!");
            return;
        }

        // JSON verisi deserialize edilir.
        modelData = JsonUtility.FromJson<AIModelData>(modelJson.text);

        // Model verisi eksik veya bozuksa hata verilir.
        if (modelData == null ||
            modelData.classes == null ||
            modelData.features == null ||
            modelData.class_log_prior == null ||
            modelData.feature_log_prob == null)
        {
            Debug.LogError("DiagnosisAI: model JSON okunamadı veya eksik!");
            return;
        }
    }

    // =====================================================
    // HASTALIK TAHMİNİ ÜRETME
    // =====================================================

    public List<DiagnosisPrediction> Predict(List<string> rawFeatures)
    {
        List<DiagnosisPrediction> emptyResults =
            new List<DiagnosisPrediction>();

        // Model yüklenmediyse boş sonuç döndürülür.
        if (modelData == null)
        {
            Debug.LogError(
                "DiagnosisAI: modelData null. Model yüklenmemiş."
            );

            return emptyResults;
        }

        // Feature yoksa tahmin yapılmaz.
        if (rawFeatures == null || rawFeatures.Count == 0)
        {
            Debug.LogWarning(
                "DiagnosisAI: Predict için feature gelmedi."
            );

            return emptyResults;
        }

        // Gelen feature'lar normalize edilir.
        List<string> normalizedFeatures =
            rawFeatures
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => AIFeatureNormalizer.Normalize(f))
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .ToList();

        List<DiagnosisPrediction> results =
            new List<DiagnosisPrediction>();

        // Her hastalık sınıfı için skor hesaplanır.
        for (int classIndex = 0;
             classIndex < modelData.classes.Length;
             classIndex++)
        {
            double score =
                modelData.class_log_prior[classIndex];

            // Eşleşen feature log probability değerleri eklenir.
            for (int featureIndex = 0;
                 featureIndex < modelData.features.Length;
                 featureIndex++)
            {
                string feature =
                    modelData.features[featureIndex];

                if (normalizedFeatures.Contains(feature))
                {
                    score +=
                        modelData.feature_log_prob[classIndex][featureIndex];
                }
            }

            results.Add(new DiagnosisPrediction
            {
                disease = modelData.classes[classIndex],
                score = score
            });
        }

        if (results.Count == 0)
            return results;

        // =====================================================
        // SOFTMAX OLASILIK HESAPLAMA
        // =====================================================

        double maxScore = results.Max(r => r.score);
        double sumExp = 0;

        // Temperature değeri minimum sınırda tutulur.
        double temperature =
            Math.Max(softmaxTemperature, 0.1);

        // Exp skorları hesaplanır.
        foreach (var r in results)
        {
            r.probability =
                Math.Exp((r.score - maxScore) / temperature);

            sumExp += r.probability;
        }

        // Geçersiz sonuç kontrolü yapılır.
        if (sumExp <= 0)
        {
            Debug.LogWarning(
                "DiagnosisAI: Softmax sumExp 0 veya negatif."
            );

            return results;
        }

        // Normalize edilmiş probability değerleri hesaplanır.
        foreach (var r in results)
        {
            r.probability /= sumExp;
        }

        // Sonuçlar probability değerine göre sıralanır.
        return results
            .OrderByDescending(r => r.probability)
            .ToList();
    }
}

// =====================================================
// AI TAHMİN SONUÇ VERİSİ
// =====================================================

[Serializable]
public class DiagnosisPrediction
{
    public string disease;
    public double score;
    public double probability;
}

// =====================================================
// MODEL JSON VERİ YAPISI
// =====================================================

[Serializable]
public class AIModelData
{
    public string[] classes;
    public string[] features;
    public double[] class_log_prior;
    public DoubleArrayWrapper[] feature_log_prob;
}

// =====================================================
// DOUBLE ARRAY WRAPPER
// =====================================================

[Serializable]
public class DoubleArrayWrapper
{
    public double[] values;

    public double this[int index]
    {
        get => values[index];
    }
}