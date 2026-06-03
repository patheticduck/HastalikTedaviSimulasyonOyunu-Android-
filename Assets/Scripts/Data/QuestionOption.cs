using UnityEngine;

[System.Serializable]
public class QuestionOption
{
    // =====================================================
    // SORU VE HASTA CEVABI
    // =====================================================

    public string questionText;
    public string patientResponse;

    // =====================================================
    // BİLGİ VE ASİSTAN TETİKLEYİCİLERİ
    // =====================================================

    public string infoTag;

    public bool triggersAssistant;

    [TextArea]
    public string assistantText;

    // =====================================================
    // DİYALOG AKIŞI
    // =====================================================

    public bool endsDialogue;
}