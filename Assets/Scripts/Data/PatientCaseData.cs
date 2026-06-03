using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Game/Patient Case")]
public class PatientCaseData : ScriptableObject
{
    // =====================================================
    // VAKA KİMLİĞİ
    // =====================================================

    [Header("Case Identity")]
    public string caseId;
    public string trueDiseaseId;

    // =====================================================
    // HASTA GÖRSELİ
    // =====================================================

    [Header("Patient Visual")]
    public int patientVisualIndex;

    // =====================================================
    // BAŞLANGIÇ AYARLARI
    // =====================================================

    [Header("Start")]
    public string rootNodeId;

    [TextArea]
    public string startText;

    [TextArea]
    public string startAssistantText;

    // =====================================================
    // DİYALOG NODE VERİLERİ
    // =====================================================

    [Header("Dialogue")]
    public List<DialogueNodeData> allNodes = new List<DialogueNodeData>();

    // =====================================================
    // TANI SEÇENEKLERİ
    // =====================================================

    [Header("Diagnosis")]
    public List<DiagnosisChoiceData> diagnosisChoices = new List<DiagnosisChoiceData>();

    // =====================================================
    // ASİSTAN İPUÇLARI
    // =====================================================

    [Header("Assistant Hints")]
    public List<AssistantHintRuleData> assistantHints = new List<AssistantHintRuleData>();

    // =====================================================
    // RASTGELE VAKA VARYASYONLARI
    // =====================================================

    [Header("Random Variation")]
    public CaseVariationProfile variationProfile;
}