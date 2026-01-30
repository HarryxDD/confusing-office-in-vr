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
    
    [Header("Audio")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip incorrectSound;
    
    [Header("Settings")]
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] public float heightAboveTray = 0.3f; 
    
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        feedbackCanvas.enabled = false;
    } 

    public IEnumerator ShowFeedback(bool isCorrect, Vector3 trayPosition, float stillnessDuration = 0f)
    {
        transform.position = trayPosition + Vector3.up * heightAboveTray;

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

        // Display
        feedbackCanvas.enabled = true;

        // wait
        yield return new WaitForSeconds(displayDuration);

        // Show stillness instruction
        instructionText.text = "Please remain still until the next trial begins.";

        // Hide
        feedbackCanvas.enabled = false;
        correctIcon.SetActive(false);
        incorrectIcon.SetActive(false);
        instructionText.text = "";
    }
}