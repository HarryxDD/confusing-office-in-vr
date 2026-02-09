using UnityEngine;
using System.Collections;

public class ExperimentManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string configFilename = "ExperimentConfig.json";
    [SerializeField] private string participantID = "P001";

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

        if (configFilename == null)
        {
            Debug.LogError("Experiment configuration file not found");
            enabled = false;
            return;
        }

        lslLogger.Initialize(config, participantID);
        StartCoroutine(RunExperiment());
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

            globalTrialNumber += block.Length;

            if (blockNum < sessionConfig.blocks.Length - 1)
            {
                yield return StartCoroutine(restScreen.ShowRest(config.timing.blockRestDuration));
            }
        }

        lslLogger.LogEvent(LSLEventCode.SessionEnd, $"S{sessionNumber}|{sessionConfig.name}");
    }
}