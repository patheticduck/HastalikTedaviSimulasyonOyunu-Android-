using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueChoiceData
{
    // =====================================================
    // TEMEL SEÇENEK BİLGİLERİ
    // =====================================================

    [Header("Buton Metni")]
    public string choiceText;

    [Header("Tür")]
    public ChoiceType choiceType = ChoiceType.Question;

    // =====================================================
    // DİYALOG AKIŞI
    // =====================================================

    [Header("Konuşma Akışı")]
    [TextArea]
    public string doctorLine;

    [TextArea]
    public string patientReply;

    [TextArea]
    public string assistantReply;

    // =====================================================
    // NODE GEÇİŞ AYARLARI
    // =====================================================

    [Header("Geçiş")]
    public string nextNodeId;
    public bool returnToHub;
    public bool opensDiagnosisPanel;

    // =====================================================
    // SEÇENEK DAVRANIŞI
    // =====================================================

    [Header("Davranış")]
    public bool reusable = true;
    public bool hideAfterUse = false;

    // =====================================================
    // TAG VE KOŞUL SİSTEMİ
    // =====================================================

    [Header("Tag Sistemi")]
    public List<string> gainedTags = new List<string>();
    public List<string> requiredTags = new List<string>();
    public List<string> blockedTags = new List<string>();

    // =====================================================
    // BELİRTİ VE TEST KAYITLARI
    // =====================================================

    [Header("Belirti / Test Kayıtları")]
    public List<string> discoveredSymptoms = new List<string>();
    public List<string> performedTests = new List<string>();

    // =====================================================
    // NOTEBOOK KAYITLARI
    // =====================================================

    [Header("Notebook Kayıtları (Türkçe)")]
    public List<string> notebookSymptoms = new List<string>();
    public List<string> notebookTests = new List<string>();
    public List<string> notebookNotes = new List<string>();

    // =====================================================
    // HASTALIK PUANLAMA SİSTEMİ
    // =====================================================

    [Header("Disease Scoring")]
    public List<DiseaseScoreEffect> diseaseEffects = new List<DiseaseScoreEffect>();

    // =====================================================
    // TEST BAŞARI ŞANSI SİSTEMİ
    // =====================================================

    [Header("Test Başarı Şansı")]
    public bool useTestChance = false;

    [Range(0, 100)]
    public int baseSuccessChance = 70;

    public DoctorSpecialization bonusSpecialization = DoctorSpecialization.None;

    [Range(0, 100)]
    public int specializationBonus = 15;

    [Range(0, 100)]
    public int generalistBonus = 5;

    [TextArea]
    public string failedTestText = "Testten net sonuç alınamadı. Klinik bulgularla devam etmelisin.";
}