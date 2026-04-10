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
        ResolveReferences();
        restCanvas.enabled = false;
    }

    void OnEnable()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (restCanvas == null)
            restCanvas = GetComponentInChildren<Canvas>(true);

        if (timerText == null)
        {
            Transform timer = transform.Find("CountdownText");
            if (timer == null)
                timer = transform.Find("TimerText");
            if (timer != null)
                timerText = timer.GetComponent<TextMeshProUGUI>();
        }

        if (messageText == null)
        {
            Transform message = transform.Find("MessageText");
            if (message != null)
                messageText = message.GetComponent<TextMeshProUGUI>();
        }

        if (blackScreen == null)
        {
            Transform black = transform.Find("BlackImage");
            if (black != null)
                blackScreen = black.GetComponent<UnityEngine.UI.Image>();
        }
    }

    public IEnumerator ShowRest(float duration)
    {
        if (restCanvas == null || timerText == null || messageText == null || blackScreen == null)
        {
            Debug.LogError("[RestScreenController] Missing UI references. Check RestScreenCanvas wiring.");
            yield break;
        }

        restCanvas.enabled = true;
        messageText.text = "";
        timerText.text = "";

        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Show rest UI
        messageText.text = "Please rest and keep still";
        timerText.text = $"Time Remaining: {FormatTime(duration)}";

        // Countdown
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            timerText.text = $"Time Remaining: {FormatTime(remainingTime)}";
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        timerText.text = "Time Remaining: 00:00";

        // Fade from black
        messageText.text = "";
        timerText.text = "";

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
        if (restCanvas == null || timerText == null || messageText == null || blackScreen == null)
        {
            Debug.LogError("[RestScreenController] Missing UI references. Check RestScreenCanvas wiring.");
            yield break;
        }

        restCanvas.enabled = true;
        blackScreen.color = Color.black;
        messageText.text = "Experiment Complete";
        timerText.text = "Thank You!";
        yield return new WaitForSeconds(5f);
    }

    public void ForceHide()
    {
        StopAllCoroutines();

        if (restCanvas != null)
            restCanvas.enabled = false;

        if (messageText != null)
            messageText.text = "";

        if (timerText != null)
            timerText.text = "";

        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
        }
    }
}