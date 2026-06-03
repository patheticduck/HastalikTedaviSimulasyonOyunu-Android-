using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DatasetExtractor : EditorWindow
{
    // =====================================================
    // TRAINING DATASET OLUŞTURMA
    // =====================================================

    [MenuItem("AI/Generate Training Dataset")]
    public static void GenerateDataset()
    {
        // Projedeki tüm PatientCaseData assetleri bulunur.
        string[] guids =
            AssetDatabase.FindAssets("t:PatientCaseData");

        List<TrainingEntry> dataset =
            new List<TrainingEntry>();

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            PatientCaseData patient =
                AssetDatabase.LoadAssetAtPath<PatientCaseData>(path);

            if (patient == null)
                continue;

            HashSet<string> features =
                new HashSet<string>();

            // Tüm node ve choice verileri taranır.
            foreach (var node in patient.allNodes)
            {
                foreach (var choice in node.choices)
                {
                    AddNormalizedFeatures(
                        features,
                        choice.gainedTags
                    );

                    AddNormalizedFeatures(
                        features,
                        choice.discoveredSymptoms
                    );

                    AddNormalizedFeatures(
                        features,
                        choice.performedTests
                    );
                }
            }

            // Hasta için eğitim verisi oluşturulur.
            TrainingEntry entry = new TrainingEntry
            {
                disease = patient.trueDiseaseId.Trim(),
                features = features
                    .OrderBy(f => f)
                    .ToList()
            };

            dataset.Add(entry);
        }

        // Dataset JSON formatına çevrilir.
        string json =
            JsonUtility.ToJson(
                new TrainingDatasetWrapper(dataset),
                true
            );

        string outputPath =
            "Assets/AI/Generated/training_dataset.json";

        // JSON dosyası oluşturulur.
        File.WriteAllText(outputPath, json);

        AssetDatabase.Refresh();
    }

    // =====================================================
    // FEATURE NORMALIZATION VE EKLEME
    // =====================================================

    private static void AddNormalizedFeatures(
        HashSet<string> features,
        List<string> rawValues)
    {
        foreach (string rawValue in rawValues)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                continue;

            // Feature normalize edilir.
            string normalized =
                AIFeatureNormalizer.Normalize(rawValue.Trim());

            // Geçerli feature ise listeye eklenir.
            if (IsValidFeature(normalized))
                features.Add(normalized);
        }
    }

    // =====================================================
    // FEATURE GEÇERLİLİK KONTROLÜ
    // =====================================================

    private static bool IsValidFeature(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim();

        // Türkçe açıklama veya notebook cümleleri filtrelenir.
        if (value.Contains(" "))
            return false;

        // Çok kısa anlamsız değerler filtrelenir.
        if (value.Length < 3)
            return false;

        return true;
    }
}

// =====================================================
// TRAINING ENTRY VERİ YAPISI
// =====================================================

[System.Serializable]
public class TrainingEntry
{
    public string disease;
    public List<string> features;
}

// =====================================================
// DATASET WRAPPER VERİ YAPISI
// =====================================================

[System.Serializable]
public class TrainingDatasetWrapper
{
    public List<TrainingEntry> entries;

    public TrainingDatasetWrapper(List<TrainingEntry> entries)
    {
        this.entries = entries;
    }
}