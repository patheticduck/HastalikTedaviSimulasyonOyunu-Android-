using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssistantHintRuleData
{
    // =====================================================
    // HINT BİLGİLERİ
    // =====================================================

    public string hintId;

    [TextArea]
    public string hintText;

    // =====================================================
    // HINT AÇILMA KOŞULLARI
    // =====================================================

    public List<string> requiredTags = new List<string>();
    public List<string> blockedTags = new List<string>();

    public int minAskedQuestions = 0;

    // =====================================================
    // KULLANIM AYARI
    // =====================================================

    public bool showOnlyOnce = true;
}