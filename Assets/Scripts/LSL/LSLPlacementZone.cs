using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

/// <summary>
/// LSL logger for placement zones (shelves, trays, etc.)
/// Attach this to objects where items can be placed
/// Requires a Collider with "Is Trigger" enabled
/// </summary>
[RequireComponent(typeof(Collider))]
public class LSLPlacementZone : MonoBehaviour
{
    [Header("Zone Identification")]
    [SerializeField] private string zoneName = "Shelf01";
    [SerializeField] private string zoneType = "Shelf"; // Shelf, Tray, Desk, etc.
    
    [Header("LSL Configuration")]
    [SerializeField] private string streamName = "ConfusingOffice.PlacementZones";
    [SerializeField] private string streamType = "Markers";
    [SerializeField] private bool useSharedOutlet = true; // Share one outlet across all zones
    
    [Header("Tracking")]
    [SerializeField] private bool trackEnterExit = true;
    [SerializeField] private bool trackDwellTime = true;
    
    [Header("Debug")]
    [SerializeField] private bool logToConsole = true;
    
    private static StreamOutlet sharedOutlet;
    private static int instanceCount = 0;
    
    private StreamOutlet outlet;
    private string[] sample = { "" };
    private Dictionary<GameObject, float> objectEnterTimes = new Dictionary<GameObject, float>();
    
    void Start()
    {
        // Ensure we have a trigger collider
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[LSLPlacementZone] {gameObject.name} collider is not set as trigger. Setting it now.");
            col.isTrigger = true;
        }
        
        InitializeLSL();
    }
    
    void InitializeLSL()
    {
        if (useSharedOutlet)
        {
            // Use a single shared outlet for all placement zones
            if (sharedOutlet == null)
            {
                var hash = new Hash128();
                hash.Append(streamName);
                hash.Append(streamType);
                hash.Append("SharedOutlet");
                
                StreamInfo streamInfo = new StreamInfo(
                    streamName, 
                    streamType, 
                    1, 
                    LSL.LSL.IRREGULAR_RATE,
                    channel_format_t.cf_string, 
                    hash.ToString()
                );
                
                // Add metadata
                XMLElement channels = streamInfo.desc().append_child("channels");
                XMLElement channel = channels.append_child("channel");
                channel.append_child_value("label", "PlacementZoneEvent");
                channel.append_child_value("type", "Marker");
                
                streamInfo.desc().append_child_value("manufacturer", "ConfusingOffice");
                streamInfo.desc().append_child_value("task", "VR_Office_Placement");
                
                sharedOutlet = new StreamOutlet(streamInfo);
                LogMessage("Shared LSL Outlet initialized: " + streamName);
            }
            outlet = sharedOutlet;
            instanceCount++;
        }
        else
        {
            // Create individual outlet for this zone
            var hash = new Hash128();
            hash.Append(streamName + "_" + zoneName);
            hash.Append(streamType);
            hash.Append(gameObject.GetInstanceID());
            
            StreamInfo streamInfo = new StreamInfo(
                streamName + "_" + zoneName, 
                streamType, 
                1, 
                LSL.LSL.IRREGULAR_RATE,
                channel_format_t.cf_string, 
                hash.ToString()
            );
            
            outlet = new StreamOutlet(streamInfo);
            LogMessage($"LSL Outlet initialized: {streamName}_{zoneName}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!trackEnterExit) return;
        
        // Filter out hands, controllers, or other non-interactable objects
        if (IsTrackedObject(other.gameObject))
        {
            float timestamp = Time.time;
            Vector3 position = other.transform.position;
            
            // Store enter time for dwell calculation
            if (trackDwellTime)
            {
                objectEnterTimes[other.gameObject] = timestamp;
            }
            
            // Format: EventType|ZoneName|ZoneType|ObjectName|PositionX|PositionY|PositionZ|Timestamp
            string eventData = $"Enter|{zoneName}|{zoneType}|{other.gameObject.name}|{position.x:F3}|{position.y:F3}|{position.z:F3}|{timestamp:F3}";
            
            SendLSLMarker(eventData);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!trackEnterExit) return;
        
        if (IsTrackedObject(other.gameObject))
        {
            float timestamp = Time.time;
            Vector3 position = other.transform.position;
            float dwellTime = 0f;
            
            // Calculate dwell time
            if (trackDwellTime && objectEnterTimes.ContainsKey(other.gameObject))
            {
                dwellTime = timestamp - objectEnterTimes[other.gameObject];
                objectEnterTimes.Remove(other.gameObject);
            }
            
            // Format: EventType|ZoneName|ZoneType|ObjectName|PositionX|PositionY|PositionZ|DwellTime|Timestamp
            string eventData = $"Exit|{zoneName}|{zoneType}|{other.gameObject.name}|{position.x:F3}|{position.y:F3}|{position.z:F3}|{dwellTime:F3}|{timestamp:F3}";
            
            SendLSLMarker(eventData);
        }
    }
    
    bool IsTrackedObject(GameObject obj)
    {
        // Exclude hands, controllers, players, etc. by name
        string objName = obj.name.ToLower();
        if (objName.Contains("hand") ||
            objName.Contains("controller") ||
            objName.Contains("player") ||
            objName.Contains("interactor") ||
            objName.Contains("ray"))
        {
            return false;
        }
        
        // Check if it has a Rigidbody or XRGrabInteractable (typical for interactive objects)
        return obj.GetComponent<Rigidbody>() != null || 
               obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null;
    }
    
    void SendLSLMarker(string eventData)
    {
        if (outlet != null)
        {
            sample[0] = eventData;
            outlet.push_sample(sample);
            
            LogMessage(eventData);
        }
    }
    
    void LogMessage(string message)
    {
        if (logToConsole)
        {
            Debug.Log($"[LSL PlacementZone] {message}");
        }
    }
    
    void OnDestroy()
    {
        if (useSharedOutlet)
        {
            instanceCount--;
            // Only destroy shared outlet when last instance is destroyed
            if (instanceCount <= 0 && sharedOutlet != null)
            {
                sharedOutlet = null;
                instanceCount = 0;
            }
        }
    }
    
    // Public method to manually log placement events
    public void LogPlacement(GameObject obj)
    {
        float timestamp = Time.time;
        Vector3 position = obj.transform.position;
        
        string eventData = $"Placed|{zoneName}|{zoneType}|{obj.name}|{position.x:F3}|{position.y:F3}|{position.z:F3}|{timestamp:F3}";
        SendLSLMarker(eventData);
    }
}
