using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiseaseBookPage", menuName = "Game/Disease Book Page")]
public class DiseaseBookPageData : ScriptableObject
{
    // =====================================================
    // HASTALIK KİMLİĞİ
    // =====================================================

    [Header("Kimlik")]
    public string diseaseId;
    public string diseaseName;

    // =====================================================
    // SOL SAYFA İÇERİĞİ
    // =====================================================

    [Header("Sol Sayfa")]
    [TextArea(3, 8)] public string generalInfo;
    [TextArea(3, 8)] public string symptomsInfo;
    [TextArea(3, 8)] public string diagnosisTips;

    // =====================================================
    // SAĞ SAYFA İÇERİĞİ
    // =====================================================

    [Header("Sağ Sayfa")]
    public Sprite diseaseImage;

    [TextArea(3, 8)]
    public string extraInfo;

    // =====================================================
    // NOTEBOOK EŞLEŞME KELİMELERİ
    // =====================================================

    [Header("Vurgulanacak Kelimeler")]
    public List<string> highlightKeywords = new List<string>();
}