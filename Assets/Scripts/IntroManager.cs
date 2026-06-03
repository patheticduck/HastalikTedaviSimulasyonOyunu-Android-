using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("References")]
    public TMP_Text loadingText;
    public Image fadePanel;

    [Header("Settings")]
    public float introDuration = 4f;
    public string nextSceneName = "MainScene";

    private void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        float elapsed = 0f;

        Color panelColor = fadePanel.color;

        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / introDuration;

            panelColor.a = Mathf.Lerp(1f, 0f, t);
            fadePanel.color = panelColor;

            int dots =
                Mathf.FloorToInt(Time.time * 2f) % 4;

            loadingText.text =
                "Patient Loading" +
                new string('.', dots);

            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}