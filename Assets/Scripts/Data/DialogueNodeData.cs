using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNodeData
{
    // =====================================================
    // NODE KİMLİĞİ VE AYARLARI
    // =====================================================

    [Header("Node Kimliği")]
    public string nodeId;

    [Header("Node Ayarları")]
    public bool isHubNode; // Ana menü gibi davranır

    // =====================================================
    // NODE GİRİŞ METİNLERİ
    // =====================================================

    [TextArea]
    public string nodeIntroText;

    [TextArea]
    public string nodeAssistantText;

    // =====================================================
    // NODE SEÇENEKLERİ
    // =====================================================

    [Header("Seçenekler")]
    public List<DialogueChoiceData> choices = new List<DialogueChoiceData>();
}