using UnityEngine;
using LSL;

public class LSLExperimentLogger : MonoBehaviour
{
    [Header("Stream Configuration")]
    [SerializeField] private string streamName = "ConfusingOffice.Experiment";
    [SerializeField] private string streamType = "Markers";
    
    [Header("Head Tracking")]
    [SerializeField] private bool enableHeadTracking = false;
    [SerializeField] private Transform headTransform;
    [SerializeField] private float headTrackingRate = 60f;  // Hz
    
    private StreamOutlet eventOutlet;
    private StreamOutlet headTrackingOutlet;
    private int[] eventSample = { 0 };
    private float[] headSample = new float[7];  // x,y,z,qx,qy,qz,qw
    
    private float headTimer = 0f;

    public void Initialize(ExperimentConfig config, string participantID)
    {
        // Event stream
        var hash = new Hash128();
        hash.Append(streamName);
        hash.Append(System.DateTime.Now.ToString());

        StreamInfo eventInfo = new StreamInfo(
            streamName,
            streamType,
            1,
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_string,
            hash.ToString()
        );

        eventInfo.desc().append_child_value("ExperimentName", config.experimentInfo.name);
        eventInfo.desc().append_child_value("Version", config.experimentInfo.version);
        eventInfo.desc().append_child_value("ParticipantID", participantID);

        eventOutlet = new StreamOutlet(eventInfo);
        Debug.Log($"[LSL] Event stream: {streamName}");

        // Head tracking stream
        if (enableHeadTracking && headTransform != null)
        {
            StreamInfo headInfo = new StreamInfo(
                streamName + ".HeadTracking",
                "Position",
                7,
                headTrackingRate,
                channel_format_t.cf_float32,
                hash.ToString() + "_head"
            );
            
            headTrackingOutlet = new StreamOutlet(headInfo);
            Debug.Log($"[LSL] Head tracking: {headTrackingRate}Hz");
        }
    }

    void Update()
    {
        if (headTrackingOutlet != null && headTransform != null)
        {
            headTimer += Time.deltaTime;
            
            if (headTimer >= 1f / headTrackingRate)
            {
                headSample[0] = headTransform.position.x;
                headSample[1] = headTransform.position.y;
                headSample[2] = headTransform.position.z;
                headSample[3] = headTransform.rotation.x;
                headSample[4] = headTransform.rotation.y;
                headSample[5] = headTransform.rotation.z;
                headSample[6] = headTransform.rotation.w;
                
                headTrackingOutlet.push_sample(headSample);
                headTimer = 0f;
            }
        }
    }

    public void LogEvent(LSLEventCode eventCode, string metadata = "")
    {
        if (eventOutlet != null)
        {
            int code = (int)eventCode;
            string data = string.IsNullOrEmpty(metadata) 
                ? $"{code}" 
                : $"{code}|{metadata}";

            eventSample[0] = code;
            eventOutlet.push_sample(eventSample);
            // Debug.Log($"[{code}] | {metadata}");
            Debug.Log("Event code: " + eventSample[0]);
        }
    }

    // Helper to convert color string to code
    public LSLEventCode GetColorCode(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red": return LSLEventCode.ColorRed;
            case "green": return LSLEventCode.ColorGreen;
            case "blue": return LSLEventCode.ColorBlue;
            case "yellow": return LSLEventCode.ColorYellow;
            default: return LSLEventCode.ColorUnknown; // fallback
        }
    }
    
    // Helper to get condition code
    public LSLEventCode GetConditionCode(TrialCondition condition)
    {
        return condition == TrialCondition.Control 
            ? LSLEventCode.ConditionControl 
            : LSLEventCode.ConditionConfusion;
    }
}