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

    [Header("Latency Testing Cube")]
    [SerializeField] private GameObject latencyCube;  
    [SerializeField] private float cubeOffsetFromFeedback = 0.3f; // Distance from feedback canvas

    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;

    [Header("Settings")]
    [SerializeField] public float heightAboveTray = 0f;
    [SerializeField] public float scanDurationMin = 2f;
    [SerializeField] public float scanDurationMax = 2.5f;

    [SerializeField] private LSLExperimentLogger lslLogger;

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
        
        // Initialize cube as invisible
        if (latencyCube != null)
        {
            latencyCube.SetActive(false);
            Debug.Log("Latency testing cube initialized as invisible");
        }
    }

    public IEnumerator ShowFeedback(bool isCorrect, Vector3 trayPosition, float stillnessDuration = 0f)
    {
        lslLogger.LogEvent(LSLEventCode.FeedbackScanStart);
        
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

        lslLogger.LogEvent(isCorrect ? LSLEventCode.FeedbackShowCorrect : LSLEventCode.FeedbackShowIncorrect);
        // CRITICAL: Show feedback icon AND cube at EXACT same time
     
        // Show appropriate icon
        correctIcon.SetActive(isCorrect);
        incorrectIcon.SetActive(!isCorrect);
        instructionText.text = "";

        // NEW: Show latency cube at EXACT same moment as feedback
        if (latencyCube != null)
        {
            // Position cube near feedback (offset to the side so it's visible)
            Vector3 cubePosition = canvasTransform.position + canvasTransform.right * cubeOffsetFromFeedback;
            latencyCube.transform.position = cubePosition;
            
            // Make cube face camera
            latencyCube.transform.LookAt(mainCamera.transform.position, Vector3.up);
            
            // Show cube
            latencyCube.SetActive(true);
            
            // Send LSL marker for cube appearance
            lslLogger.LogEvent(LSLEventCode.LatencyCubeAppear);
            
            Debug.Log($"Latency cube shown at {Time.time}");
        }

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

          // Hide feedback AND cube together
    
        // Hide feedback
        feedbackCanvas.enabled = false;
        correctIcon.SetActive(false);
        incorrectIcon.SetActive(false);
        instructionText.text = "";
        
        // NEW: Hide cube
        if (latencyCube != null)
        {
            latencyCube.SetActive(false);
            lslLogger.LogEvent(LSLEventCode.LatencyCubeDisappear);
            Debug.Log($"Latency cube hidden at {Time.time}");
        }
        
        lslLogger.LogEvent(LSLEventCode.FeedbackEnd);
    }
}