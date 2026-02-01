using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class PaperGrabbable : MonoBehaviour
{
    public string PaperColor { get; set; }
    public bool IsGrabbed { get; private set; }
    public bool IsPlaced { get; private set; }
    public string PlacedTrayName { get; private set; }
    
    private XRGrabInteractable grabbable;
    
    void Awake()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        grabbable.selectEntered.AddListener(OnGrabbed);
        grabbable.selectExited.AddListener(OnReleased);
    }
    
    void OnGrabbed(SelectEnterEventArgs args)
    {
        IsGrabbed = true;
    }
    
    void OnReleased(SelectExitEventArgs args)
    {
        // Check if released into a socket (tray)
        CheckSocketPlacement();
    }
    
    void CheckSocketPlacement()
    {
        XRSocketInteractor socket = GetComponentInParent<XRSocketInteractor>();
        if (socket != null)
        {
            IsPlaced = true;
            PlacedTrayName = socket.gameObject.name;
        }
    }
    
    void OnDestroy()
    {
        if (grabbable != null)
        {
            grabbable.selectEntered.RemoveListener(OnGrabbed);
            grabbable.selectExited.RemoveListener(OnReleased);
        }
    }
}