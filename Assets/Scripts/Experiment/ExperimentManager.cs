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
    [SerializeField] private Transform recenterTarget;
    [SerializeField] private float keyboardHeightStep = 0.05f;

    [Header("References")]
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private RestScreenController restScreen;
    [SerializeField] private LSLExperimentLogger lslLogger;
    [SerializeField] private TrialController trialController;

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

        if (trialController == null)
            trialController = FindFirstObjectByType<TrialController>();

        StartCoroutine(RunExperiment());
    }

    void Update()
    {
        HandleKeyboardHeightAdjustment();
        HandleKeyboardRecenterInput();
        HandleKeyboardPaperResetInput();
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

        RecenterToTarget();
    }

    private void HandleKeyboardPaperResetInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.pKey.wasPressedThisFrame)
            return;

        RequestPaperReset();
    }

    private void RecenterToTarget()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (xrOriginRoot == null && headCamera != null)
            xrOriginRoot = headCamera.root;

        if (xrOriginRoot == null || headCamera == null || recenterTarget == null)
            return;

        Vector3 offset = headCamera.position - xrOriginRoot.position;
        offset.y = 0f;
        xrOriginRoot.position = recenterTarget.position - offset;

        Vector3 targetForward = recenterTarget.forward;
        targetForward.y = 0f;

        Vector3 cameraForward = headCamera.forward;
        cameraForward.y = 0f;

        if (targetForward.sqrMagnitude < 0.0001f || cameraForward.sqrMagnitude < 0.0001f)
            return;

        float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);
        xrOriginRoot.RotateAround(headCamera.position, Vector3.up, angle);
    }

    private void RequestPaperReset()
    {
        if (trialController == null)
            trialController = FindFirstObjectByType<TrialController>();

        if (trialController == null)
            return;

        trialController.ResetCurrentPaperToSpawn();
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