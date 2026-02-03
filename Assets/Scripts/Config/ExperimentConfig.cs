using System;
using System.Collections.Generic;

[Serializable]
public class ExperimentConfig
{
    public ExperimentInfo experimentInfo;
    public TimingSettings timing;
    public TaskSettings taskSettings;
    public SessionConfig session1;
    public SessionConfig session2;
}

[Serializable]
public class ExperimentInfo
{
    public string name;
    public string version;
    public string description;
}

[Serializable]
public class TimingSettings
{
    public float initialRestDuration;
    public float blockRestDuration;
    public float headStillnessDuration;
    public float interSessionRestDuration;
}

[Serializable]
public class TaskSettings
{
    public List<string> colors;
}

[Serializable]
public class SessionConfig
{
    public int sessionNumber;
    public string name;
    public int totalTrials;
    public BlockConfig[] blocks;
}

[Serializable]
public class BlockConfig
{
    public string[] trials;
    public string note;
}

public enum TrialCondition
{
    Control, // C - Normal color matching
    Confusion // X - Randomized matching
}