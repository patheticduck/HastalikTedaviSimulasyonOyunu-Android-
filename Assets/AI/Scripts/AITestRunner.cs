using System.Collections.Generic;
using UnityEngine;

public class AITestRunner : MonoBehaviour
{
    public DiagnosisAI diagnosisAI;

    // =====================================================
    // AI TEST ÇALIŞTIRMA
    // =====================================================

    [ContextMenu("Run AI Test")]
    public void RunTest()
    {
        // Test için örnek feature listesi oluşturulur.
        List<string> patientFeatures =
            new List<string>()
        {
            "HAS_FEVER",
            "HAS_COUGH",
            "HIGH_CRP",
            "XRAY_INFILTRATION",
            "HIGH_WBC"
        };

        // AI tahmin sonuçları alınır.
        List<DiagnosisPrediction> results =
            diagnosisAI.Predict(patientFeatures);

        Debug.Log("===== AI PREDICTION =====");

        // İlk 5 tahmin sonucu console'a yazdırılır.
        for (int i = 0;
             i < Mathf.Min(5, results.Count);
             i++)
        {
            var r = results[i];

            Debug.Log(
                $"{i + 1}. {r.disease} -> %{(r.probability * 100f):F2}"
            );
        }
    }
}