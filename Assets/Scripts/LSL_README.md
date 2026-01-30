# LSL Data Logging for ConfusingOffice VR

This folder contains scripts for logging and analyzing VR interaction data using Lab Streaming Layer (LSL).

## Setup Instructions

### 1. Unity Setup

#### Add LSL Scripts to Scene

1. **Main Interaction Logger:**
   - Create an empty GameObject in the scene (e.g., "LSL_Manager")
   - Attach `LSLGrabPlaceLogger.cs` to it
   - Configure settings in Inspector:
     - **Auto Find Interactables**: Enabled (automatically finds all XRGrabInteractable objects)
     - **Log To Console**: Enabled (for debugging)

2. **Placement Zone Loggers:**
   - For each shelf/tray/placement area:
     - Select the GameObject with a Collider
     - Add component `LSLPlacementZone.cs`
     - Set "Is Trigger" on the Collider
     - Configure:
       - **Zone Name**: e.g., "Shelf_Left", "PaperTray_01"
       - **Zone Type**: e.g., "Shelf", "Tray", "Desk"
       - **Use Shared Outlet**: Enabled (one stream for all zones)

3. **Tag Objects:**
   - Papers/Documents: Tag as "Document"
   - Shelves: Tag as "Shelf"
   - Trays: Tag as "Tray"

### 2. Recording Data

#### Install LabRecorder

1. Download from: https://github.com/labstreaminglayer/App-LabRecorder/releases
2. Extract and run `LabRecorder.exe`

#### Recording Steps

1. Start Unity and enter Play mode
2. Open LabRecorder
3. Click "Update" to see available streams:
   - `ConfusingOffice.Interactions` (grab/release events)
   - `ConfusingOffice.PlacementZones` (zone enter/exit)
4. Check both streams
5. Set filename (e.g., `P01_Session1.xdf`)
6. Click "Start" to begin recording
7. Perform VR task
8. Click "Stop" when done
9. File saved as `.xdf` in LabRecorder directory

### 3. Data Analysis

#### Install Python Dependencies

```bash
pip install pyxdf pandas numpy matplotlib
```

#### Run Analysis

```bash
python analyze_office_vr.py recording.xdf
```

#### Output Files

The script creates a `processed_data/` folder with:
- `interactions.csv` - All grab/release events
- `placements.csv` - Zone entry/exit events
- `hold_times.csv` - Duration each object was held
- `timeline.png` - Visual timeline of interactions

## Data Format

### Interaction Events

Format: `EventType|ObjectName|Hand|PosX|PosY|PosZ|PlacementZone|Timestamp`

**Grab Event Example:**
```
Grab|Paper_Document_03|RightHand|1.234|0.856|-2.345|1.234
```

**Release Event Example:**
```
Release|Paper_Document_03|RightHand|1.256|0.920|-2.312|Shelf_Left|2.456
```

### Placement Zone Events

Format: `EventType|ZoneName|ZoneType|ObjectName|PosX|PosY|PosZ|DwellTime|Timestamp`

**Enter Event Example:**
```
Enter|Shelf_Left|Shelf|Paper_Document_03|1.250|0.910|-2.300|3.456
```

**Exit Event Example:**
```
Exit|Shelf_Left|Shelf|Paper_Document_03|1.260|0.905|-2.295|0.523|4.123
```

## 🔧 Customization

### Adding Custom Metadata

Edit `LSLGrabPlaceLogger.cs`, in `InitializeLSL()`:

```csharp
streamInfo.desc().append_child_value("participant_id", "P01");
streamInfo.desc().append_child_value("condition", "baseline");
streamInfo.desc().append_child_value("session", "1");
```

### Filtering Objects

In `LSLGrabPlaceLogger.cs`, modify `RegisterInteractables()` to filter specific objects:

```csharp
foreach (var interactable in trackedObjects)
{
    // Only track objects with "Document" or "Paper" in name
    if (interactable.gameObject.name.Contains("Document") || 
        interactable.gameObject.name.Contains("Paper"))
    {
        objectNames[interactable] = interactable.gameObject.name;
        interactable.selectEntered.AddListener(OnObjectGrabbed);
        interactable.selectExited.AddListener(OnObjectReleased);
    }
}
```

### Custom Placement Detection

In `LSLGrabPlaceLogger.cs`, modify `DetectPlacementZone()`:

```csharp
string DetectPlacementZone(GameObject obj)
{
    // Check if object is on a specific layer
    if (obj.transform.position.y > 1.5f)
        return "HighShelf";
    else if (obj.transform.position.y < 0.8f)
        return "LowTray";
    
    // Or use distance to specific locations
    Vector3 shelfPos = new Vector3(1, 1, -2);
    if (Vector3.Distance(obj.transform.position, shelfPos) < 0.5f)
        return "MainShelf";
    
    return "None";
}
```

## Advanced Analysis Examples

### Calculate Task Completion Time

```python
analyzer = OfficeVRAnalyzer('recording.xdf')

# Time from first grab to last placement
first_event = analyzer.interaction_stream['timestamp'].min()
last_event = analyzer.interaction_stream['timestamp'].max()
task_duration = last_event - first_event

print(f"Task completed in {task_duration:.2f} seconds")
```

### Analyze Hand Preference

```python
grabs = analyzer.interaction_stream[analyzer.interaction_stream['event_type'] == 'Grab']
hand_counts = grabs['hand'].value_counts()

print(f"Left hand: {hand_counts.get('LeftHand', 0)}")
print(f"Right hand: {hand_counts.get('RightHand', 0)}")
```

### Error Detection (Re-grabs)

```python
hold_times = analyzer.calculate_interaction_times()

# Find objects grabbed multiple times
object_grab_counts = hold_times['object_name'].value_counts()
re_grabbed = object_grab_counts[object_grab_counts > 1]

print("Objects re-grabbed:")
for obj, count in re_grabbed.items():
    print(f"  {obj}: {count} times")
```
## Resources

- **LSL Documentation:** https://labstreaminglayer.readthedocs.io/
- **LSL4Unity:** https://github.com/labstreaminglayer/LSL4Unity
- **LabRecorder:** https://github.com/labstreaminglayer/App-LabRecorder
- **PyXDF:** https://github.com/xdf-modules/pyxdf