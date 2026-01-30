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
    private string[] eventSample = { "" };
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

    public void LogEvent(string eventData)
    {
        if (eventOutlet != null)
        {
            eventSample[0] = $"{Time.time:F3}|{eventData}";
            eventOutlet.push_sample(eventSample);
            Debug.Log($"[LSL] {eventSample[0]}");
        }
    }
}