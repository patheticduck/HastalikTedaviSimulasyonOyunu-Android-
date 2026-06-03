using System.Collections.Generic;

public static class AIFeatureNormalizer
{
    // =====================================================
    // FEATURE NORMALIZATION EŞLEŞME TABLOSU
    // =====================================================

    private static readonly Dictionary<string, string> map =
        new Dictionary<string, string>
    {
        // Fever
        { "fever", "HAS_FEVER" },
        { "fever_yes", "HAS_FEVER" },
        { "fever_mid", "HAS_FEVER" },
        { "fever_mild", "HAS_FEVER" },
        { "mild_fever", "HAS_FEVER" },
        { "low_fever", "HAS_FEVER" },
        { "low_grade_fever", "HAS_FEVER" },
        { "high_fever", "HAS_FEVER" },
        { "measured_high_fever", "HAS_FEVER" },
        { "fever_no", "NO_FEVER" },
        { "no_fever", "NO_FEVER" },

        // Cough / sputum
        { "cough", "HAS_COUGH" },
        { "cough_yes", "HAS_COUGH" },
        { "dry_cough", "HAS_COUGH" },
        { "dry_cough_yes", "HAS_COUGH" },
        { "cough_mild", "HAS_COUGH" },
        { "mild_cough", "HAS_COUGH" },
        { "productive_cough", "HAS_COUGH" },
        { "sputum", "HAS_SPUTUM" },
        { "morning_sputum", "HAS_SPUTUM" },
        { "mucus", "HAS_SPUTUM" },
        { "yellow_sputum", "HAS_SPUTUM" },

        // Upper respiratory
        { "throat_pain", "HAS_THROAT_PAIN" },
        { "mild_sore_throat", "HAS_THROAT_PAIN" },
        { "throat_irritation", "HAS_THROAT_PAIN" },
        { "mild_throat_irritation", "HAS_THROAT_PAIN" },
        { "odynophagia", "HAS_THROAT_PAIN" },
        { "runny_nose", "HAS_RUNNY_NOSE" },
        { "mild_runny_nose", "HAS_RUNNY_NOSE" },
        { "clear_nasal_discharge", "HAS_RUNNY_NOSE" },
        { "nasal_congestion", "HAS_NASAL_CONGESTION" },
        { "nasal_congestion_yes", "HAS_NASAL_CONGESTION" },

        // General
        { "fatigue", "HAS_FATIGUE" },
        { "fatigue_yes", "HAS_FATIGUE" },
        { "fatigue_present", "HAS_FATIGUE" },
        { "fatigue_mild", "HAS_FATIGUE" },
        { "mild_fatigue", "HAS_FATIGUE" },
        { "body_pain", "HAS_MYALGIA" },
        { "muscle_ache", "HAS_MYALGIA" },
        { "muscle_pain_yes", "HAS_MYALGIA" },
        { "mild_myalgia", "HAS_MYALGIA" },

        // Lab
        { "crp_high", "HIGH_CRP" },
        { "crp_mild_high", "HIGH_CRP" },
        { "crp_mildly_elevated", "HIGH_CRP" },
        { "crp_norm", "NORMAL_CRP" },
        { "crp_normal", "NORMAL_CRP" },

        // Viral tests
        { "covid_test_positive", "COVID_POSITIVE" },
        { "covid_positive", "COVID_POSITIVE" },
        { "covid_negative", "COVID_NEGATIVE" },
        { "flu_negative", "FLU_NEGATIVE" },

        // Xray / lung
        { "xray_infiltration", "XRAY_INFILTRATION" },
        { "infiltration", "XRAY_INFILTRATION" },
        { "xray_no_infiltration", "NO_XRAY_INFILTRATION" },
        { "no_infiltration", "NO_XRAY_INFILTRATION" },
        { "mild_wheezing", "WHEEZING" },
        { "wheezing", "WHEEZING" },
        { "mild_rhonchi", "RHONCHI" },
        { "diffuse_rhonchi", "RHONCHI" },
        { "fine_crackles", "CRACKLES" },
        { "rale", "CRACKLES" },
        { "right_lower_crackles", "CRACKLES" },

        // Gastro
        { "epi_pain", "EPIGASTRIC_PAIN" },
        { "epigastric_discomfort", "EPIGASTRIC_PAIN" },
        { "mid_upper", "EPIGASTRIC_PAIN" },
        { "acid_regurgitation", "REFLUX" },
        { "heartburn", "REFLUX" },
        { "mild_reflux", "REFLUX" },
        { "retrosternal_burning", "REFLUX" },
        { "nausea", "NAUSEA" },
        { "nausea_mild", "NAUSEA" },
        { "mild_nausea", "NAUSEA" },
        { "diarrhea", "DIARRHEA" },
        { "freq_stool", "DIARRHEA" },
        { "night_stool", "DIARRHEA" },
        { "bloating", "BLOATING" },

        // H. pylori / GI tests
        { "hp_pos", "HPYLORI_POSITIVE" },
        { "hpylori_positive", "HPYLORI_POSITIVE" },
        { "hp_neg", "HPYLORI_NEGATIVE" },
        { "hpylori_negative", "HPYLORI_NEGATIVE" },
        { "lipase_high", "LIPASE_HIGH" },
        { "amylase_high", "LIPASE_HIGH" },
        { "murphy_pos", "GALLBLADDER_SIGN" },
        { "fat_meal", "GALLBLADDER_SIGN" },
        { "fat_food", "GALLBLADDER_SIGN" },
        { "calpro_high", "CALPRO_HIGH" },
        { "stool_occult_blood_positive", "OCCULT_BLOOD_POSITIVE" },

        // Special cases
        { "ana_pos", "AUTOIMMUNE_POSITIVE" },
        { "ana_lowpos", "AUTOIMMUNE_POSITIVE" },
        { "anti_ds_pos", "AUTOIMMUNE_POSITIVE" },
        { "c3_low", "AUTOIMMUNE_POSITIVE" },
        { "lyme_pos", "LYME_POSITIVE" },
        { "ft3_high", "THYROID_HIGH" },
        { "ft4_high", "THYROID_HIGH" },
        { "tsh_low", "THYROID_HIGH" },
        { "tachycardia", "TACHYCARDIA" },
        { "ekg_sinus", "TACHYCARDIA" },
        { "fine_tremor", "TREMOR" },
        { "weight_loss", "WEIGHT_LOSS" },
        { "joint_pain", "JOINT_PAIN" },
        { "focal_joint", "JOINT_PAIN" },
        { "hand_wrist", "JOINT_PAIN" }
    };

    // =====================================================
    // FEATURE NORMALIZATION İŞLEMİ
    // =====================================================

    public static string Normalize(string feature)
    {
        // Boş veya geçersiz feature kontrolü.
        if (string.IsNullOrWhiteSpace(feature))
            return "";

        feature = feature.Trim();

        // Feature eşleşme tablosunda varsa normalize edilir.
        if (map.ContainsKey(feature))
            return map[feature];

        // Eşleşme yoksa orijinal değer döndürülür.
        return feature;
    }
}