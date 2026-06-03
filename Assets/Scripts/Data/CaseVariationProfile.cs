using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CaseVariationProfile", menuName = "Diagnosis Game/Case Variation Profile")]
public class CaseVariationProfile : ScriptableObject
{
    // =====================================================
    // SAYISAL VARYASYON DEĞERLERİ
    // =====================================================

    [Header("Sayısal Değerler")]
    public List<FloatVariableRange> floatVariables = new List<FloatVariableRange>();

    // =====================================================
    // METİN VARYASYON SEÇENEKLERİ
    // =====================================================

    [Header("Metin Seçenekleri")]
    public List<StringVariableOptions> stringVariables = new List<StringVariableOptions>();
}

[System.Serializable]
public class FloatVariableRange
{
    // =====================================================
    // SAYISAL DEĞİŞKEN AYARLARI
    // =====================================================

    public string key;
    public float minValue;
    public float maxValue;
    public int decimalPlaces = 1;
}

[System.Serializable]
public class StringVariableOptions
{
    // =====================================================
    // METİN DEĞİŞKEN SEÇENEKLERİ
    // =====================================================

    public string key;
    public List<string> options = new List<string>();
}