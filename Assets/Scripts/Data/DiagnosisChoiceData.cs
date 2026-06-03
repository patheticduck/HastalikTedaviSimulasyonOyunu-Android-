using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DiagnosisChoiceData
{
    // =====================================================
    // TANI KİMLİĞİ
    // =====================================================

    [Header("Kimlik")]
    public string diseaseId;

    // =====================================================
    // TANI PANELİ UI VERİLERİ
    // =====================================================

    [Header("UI")]
    public string diagnosisName;

    [TextArea]
    public string resultText;

    public bool isCorrectDiagnosis;

    // =====================================================
    // OPSİYONEL TANI KOŞULLARI
    // =====================================================

    [Header("Opsiyonel Koşullar")]
    public List<string> recommendedTags = new List<string>();
}