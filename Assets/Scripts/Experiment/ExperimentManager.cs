using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ExperimentManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string configFilename = "ExperimentConfig.json";
    [SerializeField] private string participantID = "P001";

    [Header("Height Adjustment")]
    [SerializeField] private Transform xrOriginRoot;
    [SerializeField] private Transform headCamera;
    [SerializeField] private float keyboardHeightStep = 0.05f;

    [Header("References")]
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private RestScreenController restScreen;
    [SerializeField] private LSLExperimentLogger lslLogger;

    private ExperimentConfig config;
    private ExperimentState currentState;

    public enum ExperimentState
    {
        NotStarted,
        InitialRest,
        RunningSession1,
        InterSessionRest,
        RunningSession2,
        Completed
    }

    void Start()
    {
        config = ExperimentConfigLoader.LoadConfig(configFilename);

        if (config == null)
        {
            Debug.LogError("Experiment configuration file not found");
            enabled = false;
            return;
        }

        lslLogger.Initialize(config, participantID);
        StartCoroutine(RunExperiment());
    }

    void Update()
    {
        HandleKeyboardHeightAdjustment();
        HandleKeyboardRecenterInput();
    }

    private void HandleKeyboardHeightAdjustment()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.jKey.wasPressedThisFrame)
            AdjustHeight(-keyboardHeightStep);

        if (keyboard.kKey.wasPressedThisFrame)
            AdjustHeight(keyboardHeightStep);
    }

    private void AdjustHeight(float deltaY)
    {
        if (xrOriginRoot == null && Camera.main != null)
            xrOriginRoot = Camera.main.transform.root;

        if (xrOriginRoot == null)
            return;

        Vector3 pos = xrOriginRoot.position;
        pos.y += deltaY;
        xrOriginRoot.position = pos;
    }

    private void HandleKeyboardRecenterInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.rKey.wasPressedThisFrame)
            return;

        RecenterToCurrentHeading();
    }

    private void RecenterToCurrentHeading()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (xrOriginRoot == null && headCamera != null)
            xrOriginRoot = headCamera.root;

        if (xrOriginRoot == null || headCamera == null)
            return;

        Vector3 flatForward = Vector3.ProjectOnPlane(headCamera.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f)
            return;

        float currentYaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;
        float yawDelta = -currentYaw + 90f;
        xrOriginRoot.RotateAround(headCamera.position, Vector3.up, yawDelta);
    }

    IEnumerator RunExperiment()
    {
        lslLogger.LogEvent(LSLEventCode.ExperimentStart, $"Participant:{participantID}");

        // Initial Rest
        currentState = ExperimentState.InitialRest;
        yield return StartCoroutine(restScreen.ShowRest(config.timing.initialRestDuration));

        // Session 1
        currentState = ExperimentState.RunningSession1;
        yield return StartCoroutine(RunSession(config.session1));

        // Inter-session Rest
        currentState = ExperimentState.InterSessionRest;
        yield return StartCoroutine(restScreen.ShowRest(config.timing.interSessionRestDuration));

        // Session 2
        currentState = ExperimentState.RunningSession2;
        yield return StartCoroutine(RunSession(config.session2));

        // Experiment Completed
        currentState = ExperimentState.Completed;
        lslLogger.LogEvent(LSLEventCode.ExperimentEnd, $"Participant:{participantID}");
        yield return StartCoroutine(restScreen.ShowCompletionMessage());
    }

    IEnumerator RunSession(SessionConfig sessionConfig)
    {
        int sessionNumber = sessionConfig.sessionNumber;
        lslLogger.LogEvent(LSLEventCode.SessionStart, $"S{sessionNumber}|{sessionConfig.name}");

        int globalTrialNumber = 1;
        
        for (int blockNum = 0; blockNum < sessionConfig.blocks.Length; blockNum++)
        {
            string block = sessionConfig.blocks[blockNum];
            TrialCondition condition = (block == "C") ? TrialCondition.Control : TrialCondition.Confusion;

            // Run block
            yield return StartCoroutine(blockManager.RunBlock(
                sessionNumber,
                blockNum + 1,
                globalTrialNumber,
                sessionConfig.trialsPerBlock,
                condition,
                config
            ));

            globalTrialNumber += sessionConfig.trialsPerBlock;

            if (blockNum < sessionConfig.blocks.Length - 1)
            {
                yield return StartCoroutine(restScreen.ShowRest(config.timing.blockRestDuration));
            }
        }

        lslLogger.LogEvent(LSLEventCode.SessionEnd, $"S{sessionNumber}|{sessionConfig.name}");
    }
}