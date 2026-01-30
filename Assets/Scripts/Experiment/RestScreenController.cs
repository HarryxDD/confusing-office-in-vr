using UnityEngine;
using System.Collections;
using TMPro;

public class RestScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas restCanvas;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private UnityEngine.UI.Image blackScreen;
    
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 2f;

    void Awake()
    {
        restCanvas.enabled = false;
    }

    public IEnumerator ShowRest(float duration)
    {
        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Show rest UI
        restCanvas.enabled = true;
        messageText.text = "Rest Period";

        // Countdown
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            timerText.text = FormatTime(remainingTime);
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        // Fade from black
        messageText.text = "Get Ready...";
        timerText.text = "";
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(FadeFromBlack());

        restCanvas.enabled = false;
    }

    IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        Color startColor = blackScreen.color;
        startColor.a = 0f;
        Color endColor = Color.black;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            blackScreen.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        blackScreen.color = Color.black;
    }

    IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        Color startColor = Color.black;
        Color endColor = Color.black;
        endColor.a = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            blackScreen.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        blackScreen.color = endColor;
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public IEnumerator ShowCompletionMessage()
    {
        restCanvas.enabled = true;
        blackScreen.color = Color.black;
        messageText.text = "Experiment Complete";
        timerText.text = "Thank You!";
        yield return new WaitForSeconds(5f);
    }
}