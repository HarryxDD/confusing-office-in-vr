using TMPro;
using UnityEngine;
using System.Collections;

public class FeedbackDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas feedbackCanvas;
    [SerializeField] private GameObject correctIcon;
    [SerializeField] private GameObject incorrectIcon;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI scanningText;

    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;

    [Header("Settings")]
    [SerializeField] public float heightAboveTray = 0f;
    [SerializeField] public float scanDurationMin = 2f;
    [SerializeField] public float scanDurationMax = 2.5f;

    private AudioSource audioSource;
    private Camera mainCamera;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        mainCamera = Camera.main;

        if (feedbackCanvas != null)
        {
            feedbackCanvas.renderMode = RenderMode.WorldSpace;
            feedbackCanvas.enabled = false;
        }

        if (correctIcon != null) correctIcon.SetActive(false);
        if (incorrectIcon != null) incorrectIcon.SetActive(false);
        if (scanningText != null) scanningText.gameObject.SetActive(false);
    }

    public IEnumerator ShowFeedback(bool isCorrect, Vector3 trayPosition, float stillnessDuration = 0f)
    {
        // Move the Canvas GameObject itself, not this script's GameObject
        Transform canvasTransform = feedbackCanvas.transform;
        canvasTransform.position = trayPosition + Vector3.up * heightAboveTray;

        if (mainCamera != null)
        {
            // Make canvas face the camera (looking back at the camera position)
            canvasTransform.LookAt(mainCamera.transform.position, Vector3.up);
        }

        correctIcon.SetActive(false);
        incorrectIcon.SetActive(false);
        instructionText.text = "";

        feedbackCanvas.enabled = true;

        if (scanningText != null)
        {
            scanningText.gameObject.SetActive(true);
        }

        float scanDelay = Random.Range(scanDurationMin, scanDurationMax);
        yield return new WaitForSeconds(scanDelay);

        if (scanningText != null)
        {
            scanningText.gameObject.SetActive(false);
        }

        // Show appropriate icon
        correctIcon.SetActive(isCorrect);
        incorrectIcon.SetActive(!isCorrect);
        instructionText.text = "";

        // Play sound
        AudioClip clip = isCorrect ? correctSound : incorrectSound;
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }

        if (stillnessDuration > 0f)
        {
            // Show stillness instruction
            instructionText.text = "Please remain still";
            yield return new WaitForSeconds(stillnessDuration);
        }

        // Hide
        feedbackCanvas.enabled = false;
        correctIcon.SetActive(false);
        incorrectIcon.SetActive(false);
        instructionText.text = "";
    }
}