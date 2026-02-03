using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrialController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ColorPaperSpawner paperSpawner;
    [SerializeField] private FeedbackDisplay feedbackDisplay;
    [SerializeField] private LSLExperimentLogger lslLogger;

    private Dictionary<string, string> currentColorMapping;

    public void SetupBlockColorMapping(TrialCondition condition, List<string> colors)
    {
        currentColorMapping = new Dictionary<string, string>();

        if (condition == TrialCondition.Control)
        {
            // Control: Direct matching
            foreach (string color in colors)
            {
                currentColorMapping[color] = color;
            }
        }
        else
        {
            // Confusion: Random mapping for entire block
            List<string> avaiableTargets = new List<string>(colors);

            foreach (string color in colors)
            {
                int randomIndex = Random.Range(0, avaiableTargets.Count);
                currentColorMapping[color] = avaiableTargets[randomIndex];
                avaiableTargets.RemoveAt(randomIndex);
            }
        }

        string mapping = "";
        foreach (var kvp in currentColorMapping)
        {
            mapping += $"{kvp.Key}->{kvp.Value} ";
        }
        lslLogger.LogEvent($"BlockColorMapping|{mapping.Trim()}");
    }

    public IEnumerator RunTrial(
        int sessionNumber,
        int blockNumber,
        int globalTrialNumber,
        TrialCondition condition,
        ExperimentConfig config
    )
    {
        lslLogger.LogEvent($"TrialStart|S{sessionNumber}|B{blockNumber}|T{globalTrialNumber}|{condition}");

        string paperColor = config.taskSettings.colors[Random.Range(0, config.taskSettings.colors.Count)];

        // string correctTray;
        // if (condition == TrialCondition.Control)
        // {
        //     correctTray = paperColor;
        // }
        // else
        // {
        //     List<string> availableTrays = new List<string>(config.taskSettings.colors);
        //     availableTrays.Remove(paperColor);
        //     correctTray = availableTrays[Random.Range(0, availableTrays.Count)];
        // }

        string correctTray = currentColorMapping[paperColor];

        lslLogger.LogEvent($"ColorMapping|Paper:{paperColor}->Tray:{correctTray}");

        yield return StartCoroutine(RunSinglePaper(sessionNumber, globalTrialNumber, 1, paperColor, correctTray, config));


        lslLogger.LogEvent($"TrialEnd|S{sessionNumber}|T{globalTrialNumber}");

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator RunSinglePaper(int sessionNumber, int trialNumber, int paperNumber, string paperColor, string correctTray, ExperimentConfig config)
    {
        // Spawn
        GameObject paper = paperSpawner.SpawnPaper(paperColor);
        PaperGrabbable grabbable = paper.GetComponent<PaperGrabbable>();

        lslLogger.LogEvent($"PaperSpawned|S{sessionNumber}|T{trialNumber}|P{paperNumber}|Color:{paperColor}");
    
        // Wait for grab
        yield return new WaitUntil(() => grabbable.IsGrabbed);
        lslLogger.LogEvent($"PaperGrabbed|S{sessionNumber}|T{trialNumber}|P{paperNumber}");

        // Wait for placement
        yield return new WaitUntil(() => grabbable.IsPlaced);

        // Check correctness
        string placedTray = grabbable.PlacedTrayName;
        bool isCorrect = placedTray.Contains(correctTray);

        lslLogger.LogEvent($"PaperPlaced|S{sessionNumber}|T{trialNumber}|P{paperNumber}|PlacedIn:{placedTray}|Correct:{correctTray}|Result:{isCorrect}");

        Vector3 feedbackPosition = paper.transform.position;
        yield return StartCoroutine(feedbackDisplay.ShowFeedback(isCorrect, feedbackPosition, config.timing.headStillnessDuration));

        // Clean up paper
        Destroy(paper);

        yield return new WaitForSeconds(0.3f);
    }
}