using UnityEngine;
using System.IO;
using System;
using UnityEngine.Rendering.HighDefinition;

public static class ExperimentConfigLoader
{
    public static ExperimentConfig LoadConfig(string filename = "ExperimentConfig.json")
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        if (!File.Exists(path))
        {
            Debug.LogError($"ExperimentConfigLoader: Config file not found at {path}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            ExperimentConfig config = JsonUtility.FromJson<ExperimentConfig>(json);

            ValidateConfig(config);

            Debug.Log("ExperimentConfigLoader: Successfully loaded experiment configuration.");
            return config;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ExperimentConfigLoader: Failed to load config from {path}. Exception: {e.Message}");
            return null;
        }
    }

    private static void ValidateConfig(ExperimentConfig config)
    {
        Debug.Log("Config Validation");

        int session1Total = 0;
        int session1ControlCount = 0;

        foreach (var block in config.session1.blocks)
        {
            session1Total += block.trials.Length;
            foreach (var trial in block.trials)
            {
                if (trial == "C") session1ControlCount++;
            }
        }

        if (session1Total != config.session1.totalTrials)
        {
            Debug.LogWarning($"Session 1: Block trials ({session1Total}) don't match totalTrials ({config.session1.totalTrials}).");
        }
        else
        {
            Debug.Log($"Session 1: {session1Total} trials, {config.session1.blocks.Length} blocks, All Control.");
        }

        // Validate Session 2
        int session2Total = 0;
        int session2ControlCount = 0;
        int session2ConfusionCount = 0;
        
        foreach (var block in config.session2.blocks)
        {
            session2Total += block.trials.Length;
            foreach (var trial in block.trials)
            {
                if (trial == "C") session2ControlCount++;
                else if (trial == "X") session2ConfusionCount++;
            }
        }
        
        if (session2Total != config.session2.totalTrials)
        {
            Debug.LogWarning($"Session 2: Block trials ({session2Total}) don't match totalTrials ({config.session2.totalTrials})");
        }
        else
        {
            Debug.Log($"Session 2: {session2Total} trials, {config.session2.blocks.Length} blocks");
            Debug.Log($"{session2ControlCount} Control, {session2ConfusionCount} Confusion");
        }

        Debug.Log("End Validation");
    }
}
