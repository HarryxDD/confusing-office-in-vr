using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrialController trialController;
    [SerializeField] private LSLExperimentLogger lslLogger;

    public IEnumerator RunBlock(
        int sessionNumber,
        int blockNumber,
        int startingTrialNumber,
        int totalTrials,
        TrialCondition condition,
        ExperimentConfig config
    )
    {
        lslLogger.LogEvent(LSLEventCode.BlockStart, $"S{sessionNumber}|B{blockNumber}|Condition:{condition}");

        trialController.SetupBlockColorMapping(
            condition,
            config.taskSettings.colors
        );

        for (int i = 0; i < totalTrials; i++)
        {
            int globalTrialNumber = startingTrialNumber + i;
            
            yield return StartCoroutine(trialController.RunTrial(
                sessionNumber,
                blockNumber,
                globalTrialNumber,
                condition,
                config
            ));
        }

        lslLogger.LogEvent(LSLEventCode.BlockEnd, $"S{sessionNumber}|B{blockNumber}");
    }
}