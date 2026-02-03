# Office VR Experience

A virtual reality office environment built with Unity and XR Interaction Toolkit.

## Project Info

- **Unity Version:** 6000.2.7f2 or newer
- **Platform:** Meta Quest, PC VR
- **VR Framework:** XR Interaction Toolkit
- **Render Pipeline:** Universal Render Pipeline (URP)

## Getting Started

### Prerequisites

- Unity Hub installed
- Unity 6000.2.7f2 (or compatible version)
- Git with Git LFS installed
- VR headset (or use XR Device Simulator)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/HarryxDD/vr-confused-office.git
   cd vr-confused-office
   ```

2. **Open in Unity Hub:**
   - Add project from disk
   - Select the cloned folder
   - Wait for Unity to import all assets

3. **Install XR packages** (if prompted):
   - Window → Package Manager
   - Install XR Interaction Toolkit
   - Install XR Plugin Management

### Building for Meta Quest

1. File → Build Settings
2. Switch Platform to Android
3. Player Settings → XR Plug-in Management → Android → Enable OpenXR
4. Connect Quest via USB
5. Build and Run

## Development Notes

- Always use uniform scale (1,1,1) for parent objects
- Keep colliders on all walls/floors
- Bake lighting for better performance
- Test in actual VR headset regularly