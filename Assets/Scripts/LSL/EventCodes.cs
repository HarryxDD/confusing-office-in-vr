public enum LSLEventCode
{
    // Experiment boundaries (10-19)
    ExperimentStart = 10,
    ExperimentEnd = 11,
    
    // Session boundaries (20-29)
    SessionStart = 20,
    SessionEnd = 21,
    
    // Block boundaries (30-39)
    BlockStart = 30,
    BlockEnd = 31,
    BlockMapping = 32,  // Color mapping for block
    
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
    FeedbackShowCorrect = 61,
    FeedbackShowIncorrect = 62,
    FeedbackEnd = 63,
    LatencyCubeAppear = 64,    
    LatencyCubeDisappear = 65,
    
    // Condition markers (70-79)
    ConditionControl = 70,
    ConditionConfusion = 71,
    
    // Color markers (80-89)
    ColorRed = 80,
    ColorGreen = 81,
    ColorBlue = 82,
    ColorYellow = 83,
    ColorUnknown = 84
}