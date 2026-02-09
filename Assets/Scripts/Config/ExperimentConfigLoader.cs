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

            Debug.Log("ExperimentConfigLoader: Successfully loaded experiment configuration.");
            return config;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ExperimentConfigLoader: Failed to load config from {path}. Exception: {e.Message}");
            return null;
        }
    }
}
