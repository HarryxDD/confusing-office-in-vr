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
        TrialCondition[] conditions,
        ExperimentConfig config
    )
    {
        lslLogger.LogEvent($"BlockStart|S{sessionNumber}|B{blockNumber}|Trials:{conditions.Length}");

        for (int i = 0; i < conditions.Length; i++)
        {
            int globalTrialNumber = startingTrialNumber + i;

            yield return StartCoroutine(trialController.RunTrial(
                sessionNumber,
                blockNumber,
                globalTrialNumber,
                conditions[i],
                config
            ));
        }

        lslLogger.LogEvent($"BlockEnd|S{sessionNumber}|B{blockNumber}");
    }
}