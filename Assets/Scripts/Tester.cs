using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Test script to trigger feedback and cube without VR
/// Press SPACE to simulate feedback
/// </summary>
public class FeedbackTester : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private FeedbackDisplay feedbackDisplay;
    
    [Header("Test Settings")]
    [SerializeField] private Vector3 testTrayPosition = new Vector3(0, 1, 2);
    [SerializeField] private float testStillnessDuration = 2f;

    void Update()
    {
        // Press SPACE to test correct feedback
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE pressed - Testing CORRECT feedback with cube");
            StartCoroutine(feedbackDisplay.ShowFeedback(true, testTrayPosition, testStillnessDuration));
        }
        
        // Press X to test incorrect feedback
        if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
        {
            Debug.Log("X pressed - Testing INCORRECT feedback with cube");
            StartCoroutine(feedbackDisplay.ShowFeedback(false, testTrayPosition, testStillnessDuration));
        }
    }
}