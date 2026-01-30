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
    private List<GameObject> currentPapers;

    public IEnumerator RunTrial(
        int sessionNumber,
        int blockNumber,
        int globalTrialNumber,
        TrialCondition condition,
        ExperimentConfig config
    )
    {
        lslLogger.LogEvent($"TrialStart|S{sessionNumber}|B{blockNumber}|T{globalTrialNumber}|{condition}");

        SetupColorMapping(condition, config.taskSettings.colors);
        currentPapers = new List<GameObject>();

        // Randomize spawn order
        List<string> shuffledColors = new List<string>(config.taskSettings.colors);
        ShuffleList(shuffledColors);

        // Run 4 papers per trial
        for (int paperNum = 0; paperNum < config.taskSettings.papersPerTrial; paperNum++)
        {
            yield return StartCoroutine(RunSinglePaper(sessionNumber, globalTrialNumber, paperNum + 1, config));
        }

        // Now all papers are placed
        int correctCount = 0;
        foreach (GameObject paper in currentPapers)
        {
            PaperGrabbable grabbable = paper.GetComponent<PaperGrabbable>();
            string paperColor = grabbable.PaperColor;
            string placedTray = grabbable.PlacedTrayName;
            string correctTray = currentColorMapping[paperColor];

            if (placedTray.Contains(correctTray))
            {
                correctCount++;
            }
        }

        bool isTrialCorrect = correctCount == config.taskSettings.papersPerTrial;

        lslLogger.LogEvent($"TrialEnd|S{sessionNumber}|T{globalTrialNumber}|Result:{(isTrialCorrect ? "Correct" : "Incorrect")}|Score:{correctCount}/{config.taskSettings.papersPerTrial}");

        yield return new WaitForSeconds(0.3f);
    }

    void SetupColorMapping(TrialCondition condition, List<string> colors)
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
            // Confusion: Random mapping
            List<string> availableTargets = new List<string>(colors);

            foreach (string color in colors)
            {
                int randomIndex = Random.Range(0, availableTargets.Count);
                currentColorMapping[color] = availableTargets[randomIndex];
                availableTargets.RemoveAt(randomIndex);
            }
        }

        // Log mapping
        string mapping = "";
        foreach (var kvp in currentColorMapping)
        {
            mapping += $"{kvp.Key}->{kvp.Value} ";
        }
        lslLogger.LogEvent($"ColorMapping|{mapping}");
    }

    void ShuffleList(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    IEnumerator RunSinglePaper(int sessionNumber, int trialNumber, int paperNumber, ExperimentConfig config)
    {
        // Random color
        string paperColor = config.taskSettings.colors[Random.Range(0, config.taskSettings.colors.Count)];

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
        string correctTray = currentColorMapping[paperColor];
        bool isCorrect = placedTray.Contains(correctTray);

        lslLogger.LogEvent($"PaperPlaced|S{sessionNumber}|T{trialNumber}|P{paperNumber}|PlacedIn:{placedTray}|Correct:{correctTray}|Result:{isCorrect}");

        // Clean up paper
        Destroy(paper, 0.5f);

        yield return new WaitForSeconds(0.5f);
    }
}