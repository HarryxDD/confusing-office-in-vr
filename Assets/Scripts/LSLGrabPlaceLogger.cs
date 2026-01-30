using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using LSL;

/// <summary>
/// LSL outlet to log VR object interactions (grab, release, placement)
/// Attach this to your XR Origin or a persistent manager object
/// </summary>
public class LSLGrabPlaceLogger : MonoBehaviour
{
    [Header("LSL Configuration")]
    [SerializeField] private string streamName = "ConfusingOffice.Interactions";
    [SerializeField] private string streamType = "Markers";
    
    [Header("Auto-detect Interactables")]
    [SerializeField] private bool autoFindInteractables = true;
    
    [Header("Manual Assignment (if not auto-detecting)")]
    [SerializeField] private List<XRGrabInteractable> trackedObjects = new List<XRGrabInteractable>();
    
    [Header("Debug")]
    [SerializeField] private bool logToConsole = true;
    
    private StreamOutlet outlet;
    private string[] sample = { "" };
    private Dictionary<XRGrabInteractable, string> objectNames = new Dictionary<XRGrabInteractable, string>();
    
    void Start()
    {
        InitializeLSL();
        
        if (autoFindInteractables)
        {
            FindAllInteractables();
        }
        
        RegisterInteractables();
    }
    
    void InitializeLSL()
    {
        var hash = new Hash128();
        hash.Append(streamName);
        hash.Append(streamType);
        hash.Append(gameObject.GetInstanceID());
        
        StreamInfo streamInfo = new StreamInfo(
            streamName, 
            streamType, 
            1, 
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_string, 
            hash.ToString()
        );
        
        // Add metadata for better identification
        XMLElement channels = streamInfo.desc().append_child("channels");
        XMLElement channel = channels.append_child("channel");
        channel.append_child_value("label", "InteractionEvent");
        channel.append_child_value("type", "Marker");
        
        // Add description
        streamInfo.desc().append_child_value("manufacturer", "ConfusingOffice");
        streamInfo.desc().append_child_value("task", "VR_Office_Interaction");
        
        outlet = new StreamOutlet(streamInfo);
        
        LogMessage("LSL Outlet initialized: " + streamName);
    }
    
    void FindAllInteractables()
    {
        // Find all XRGrabInteractable objects in the scene
        XRGrabInteractable[] allInteractables = FindObjectsOfType<XRGrabInteractable>(true);
        
        trackedObjects.Clear();
        foreach (var interactable in allInteractables)
        {
            trackedObjects.Add(interactable);
        }
        
        LogMessage($"Found {trackedObjects.Count} interactable objects");
    }
    
    void RegisterInteractables()
    {
        foreach (var interactable in trackedObjects)
        {
            if (interactable == null) continue;
            
            // Store the object name for later use
            objectNames[interactable] = interactable.gameObject.name;
            
            // Subscribe to interaction events
            interactable.selectEntered.AddListener(OnObjectGrabbed);
            interactable.selectExited.AddListener(OnObjectReleased);
            
            LogMessage($"Registered: {interactable.gameObject.name}");
        }
    }
    
    void OnObjectGrabbed(SelectEnterEventArgs args)
    {
        var interactable = args.interactableObject as XRGrabInteractable;
        if (interactable == null) return;
        
        string objectName = objectNames.ContainsKey(interactable) ? objectNames[interactable] : interactable.gameObject.name;
        string handedness = GetHandedness(args.interactorObject);
        Vector3 position = interactable.transform.position;
        float timestamp = Time.time;
        
        // Format: EventType|ObjectName|Hand|PositionX|PositionY|PositionZ|Timestamp
        string eventData = $"Grab|{objectName}|{handedness}|{position.x:F3}|{position.y:F3}|{position.z:F3}|{timestamp:F3}";
        
        SendLSLMarker(eventData);
    }
    
    void OnObjectReleased(SelectExitEventArgs args)
    {
        var interactable = args.interactableObject as XRGrabInteractable;
        if (interactable == null) return;
        
        string objectName = objectNames.ContainsKey(interactable) ? objectNames[interactable] : interactable.gameObject.name;
        string handedness = GetHandedness(args.interactorObject);
        Vector3 position = interactable.transform.position;
        float timestamp = Time.time;
        
        // Check if placed in a target zone
        string placementZone = DetectPlacementZone(interactable.gameObject);
        
        // Format: EventType|ObjectName|Hand|PositionX|PositionY|PositionZ|PlacementZone|Timestamp
        string eventData = $"Release|{objectName}|{handedness}|{position.x:F3}|{position.y:F3}|{position.z:F3}|{placementZone}|{timestamp:F3}";
        
        SendLSLMarker(eventData);
    }
    
    string GetHandedness(IXRInteractor interactor)
    {
        // Try to determine which hand is being used
        string interactorName = (interactor as MonoBehaviour)?.gameObject.name ?? "Unknown";
        
        if (interactorName.Contains("Left") || interactorName.Contains("left"))
            return "LeftHand";
        else if (interactorName.Contains("Right") || interactorName.Contains("right"))
            return "RightHand";
        else
            return "UnknownHand";
    }
    
    string DetectPlacementZone(GameObject obj)
    {
        // Check for nearby placement zones
        Collider[] nearbyColliders = Physics.OverlapSphere(obj.transform.position, 0.3f);
        
        foreach (var collider in nearbyColliders)
        {
            // Skip the object itself
            if (collider.gameObject == obj) continue;
            
            string colliderName = collider.gameObject.name.ToLower();
            
            // Look for placement zones by name
            if (colliderName.Contains("shelf") || 
                colliderName.Contains("tray") ||
                colliderName.Contains("desk") ||
                colliderName.Contains("placement"))
            {
                return collider.gameObject.name;
            }
            
            // Check if it has the LSLPlacementZone component
            if (collider.GetComponent<LSLPlacementZone>() != null)
            {
                return collider.gameObject.name;
            }
        }
        
        return "None";
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
            Debug.Log($"[LSL] {message}");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from all events
        foreach (var interactable in trackedObjects)
        {
            if (interactable != null)
            {
                interactable.selectEntered.RemoveListener(OnObjectGrabbed);
                interactable.selectExited.RemoveListener(OnObjectReleased);
            }
        }
    }
    
    // Optional: Add objects at runtime
    public void AddTrackedObject(XRGrabInteractable interactable)
    {
        if (!trackedObjects.Contains(interactable))
        {
            trackedObjects.Add(interactable);
            objectNames[interactable] = interactable.gameObject.name;
            interactable.selectEntered.AddListener(OnObjectGrabbed);
            interactable.selectExited.AddListener(OnObjectReleased);
        }
    }
}
