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
        lslLogger.LogEvent("ExperimentStart");

        // Initial Rest
        currentState = ExperimentState.InitialRest;
        yield return StartCoroutine(restScreen.ShowRest(config.timing.initialRestDuration));

        // Session 1 (50 trials, all control)
        currentState = ExperimentState.RunningSession1;
        yield return StartCoroutine(RunSession(config.session1));

        // Inter-session Rest
        currentState = ExperimentState.InterSessionRest;
        yield return StartCoroutine(restScreen.ShowRest(config.timing.interSessionRestDuration));

        // Session 2 (80 trials, mixed conditions)
        currentState = ExperimentState.RunningSession2;
        yield return StartCoroutine(RunSession(config.session2));

        // Experiment Completed
        currentState = ExperimentState.Completed;
        lslLogger.LogEvent("ExperimentEnd");
        yield return StartCoroutine(restScreen.ShowCompletionMessage());
    }

    IEnumerator RunSession(SessionConfig sessionConfig)
    {
        int sessionNumber = sessionConfig.sessionNumber;
        lslLogger.LogEvent($"SessionStart|S{sessionNumber}|{sessionConfig.name}");

        int globalTrialNumber = 1;
        
        for (int blockNum = 0; blockNum < sessionConfig.blocks.Length; blockNum++)
        {
            BlockConfig block = sessionConfig.blocks[blockNum];

            // Convert trial strings to conditions
            TrialCondition[] conditions = new TrialCondition[block.trials.Length];
            for (int i = 0; i < block.trials.Length; i++)
            {
                conditions[i] = (block.trials[i] == "C") ? TrialCondition.Control : TrialCondition.Confusion;
            }

            // Run block
            yield return StartCoroutine(blockManager.RunBlock(
                sessionNumber,
                blockNum + 1,
                globalTrialNumber,
                conditions,
                config
            ));

            globalTrialNumber += block.trials.Length;

            if (blockNum < sessionConfig.blocks.Length - 1)
            {
                yield return StartCoroutine(restScreen.ShowRest(config.timing.blockRestDuration));
            }
        }

        lslLogger.LogEvent($"SessionEnd|S{sessionNumber}|{sessionConfig.name}");
    }
}