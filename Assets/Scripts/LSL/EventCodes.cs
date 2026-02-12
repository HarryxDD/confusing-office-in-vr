public enum LSLEventCode
{
    // Experiment boundaries (10-19)
    ExperimentStart = 10,
    ExperimentEnd = 11,
    
    // Session boundaries (20-29)
    SessionStart = 20,
    SessionEnd = 21,
    
    // Block boundaries (30-39)
    ControlBlockStart = 30,
    ConfusionBlockStart = 31,
    BlockEnd = 32,
    BlockMapping = 33,  // Color mapping for block
    
    // Trial boundaries (40-49)
    TrialStart = 40,
    TrialEnd = 41,
    TrialMapping = 42, // Color mapping for trial
    
    // Stimulus events (50-59)
    PaperSpawn = 50,
    PaperGrab = 51,
    PaperPlaceCorrect = 52,
    PaperPlaceIncorrect = 53,
    
    // Feedback events (60-69)
    FeedbackScanStart = 60,
    FeedbackShow = 61,
    FeedbackEnd = 62,
    LatencyCubeAppear = 63,    
    LatencyCubeDisappear = 64,
    
    // Color markers (80-89)
    PaperRed = 80,
    PaperGreen = 81,
    PaperBlue = 82,
    PaperYellow = 83,
    TrayRed = 84,
    TrayGreen = 85,
    TrayBlue = 86,
    TrayYellow = 87,
    PaperUnknown = 88,
    TrayUnknown = 89
}