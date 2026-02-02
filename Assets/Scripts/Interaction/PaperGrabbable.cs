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
        // Check if released into a socket (tray) with a small delay
        // This allows the socket to grab the object first
        Invoke(nameof(CheckSocketPlacement), 0.1f);
    }
    
    void CheckSocketPlacement()
    {
        if (grabbable.interactorsSelecting.Count > 0)
        {
            foreach (var interactor in grabbable.interactorsSelecting)
            {
                XRSocketInteractor socket = interactor as XRSocketInteractor;
                if (socket != null)
                {
                    IsPlaced = true;
                    PlacedTrayName = socket.gameObject.name;
                    return;
                }
            }
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