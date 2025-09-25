# VR Street Crossing Training App

A Unity VR application designed to help users practice street crossing safety in a virtual environment. This immersive training app allows users to experience realistic traffic scenarios and learn safe crossing behaviors without real-world risks.

## Features

### Core Functionality
- **VR Player Movement**: Step-by-step movement using physical buttons (forward/backward)
- **Head Tracking**: Natural head movement to look left and right for traffic
- **Interactive Traffic Lights**: Realistic traffic light system with timed cycles
- **AI-Driven Traffic**: Vehicles that obey traffic laws and stop at intersections
- **Safety Training**: Dedicated crossing zones and safe areas
- **Immersive Environment**: Realistic street intersection with sidewalks and buildings

### VR Interaction System
- **Physical Button Interface**: Press buttons to move forward or backward
- **Visual Feedback**: Buttons change color when hovered over or pressed
- **Haptic Feedback**: Controller vibration when interacting with buttons
- **Audio Cues**: Sound feedback for button presses and traffic lights

### Traffic System
- **Smart Vehicle AI**: Cars stop at red lights and intersections
- **Traffic Law Compliance**: Vehicles follow realistic traffic patterns
- **Pedestrian Detection**: Cars respond to pedestrians in crossing zones
- **Multiple Vehicle Types**: Various vehicles with different behaviors

## Project Structure

```
Assets/
├── Scenes/
│   └── StreetCrossingVR.unity          # Main VR scene
├── Scripts/
│   ├── VRPlayerController.cs           # Player movement and interaction
│   ├── TrafficLightController.cs       # Traffic light system
│   ├── VehicleAI.cs                   # Vehicle behavior and traffic laws
│   ├── VRMovementButton.cs            # VR button interactions
│   ├── StreetEnvironmentManager.cs    # Street layout generation
│   └── VRSceneManager.cs              # Overall scene management
├── Prefabs/                           # VR components and vehicles
├── Materials/                         # Road, sidewalk, and UI materials
└── Audio/                            # Sound effects and ambient audio

ProjectSettings/
├── ProjectSettings.asset              # VR-enabled project settings
├── XRSettings.asset                  # OpenXR configuration
└── TagManager.asset                  # Layers and tags setup
```

## Requirements

### Unity Version
- Unity 2022.3.21f1 or newer
- Universal Render Pipeline (URP) recommended for VR performance

### VR Hardware
- **Oculus/Meta Headsets**: Quest 2, Quest 3, Quest Pro
- **OpenXR Compatible Headsets**: HTC Vive, Valve Index, Pico
- **Minimum Play Area**: 2m x 2m standing area recommended

### Dependencies
The project includes these Unity packages:
- XR Interaction Toolkit (2.4.3+)
- XR Management (4.4.0+)
- OpenXR Plugin (1.8.2+)
- NavMesh Components (for vehicle AI)

## Setup Instructions

### 1. Open in Unity
1. Open Unity Hub
2. Click "Add project from disk"
3. Navigate to this project folder
4. Open the project in Unity 2022.3.21f1 or newer

### 2. VR Configuration
1. Go to **Edit → Project Settings → XR Plug-in Management**
2. Enable **OpenXR** for your target platform
3. Under **OpenXR Feature Groups**, enable:
   - **Oculus** (for Meta headsets)
   - **Microsoft HoloLens** (if using WMR)
   - **HTC Vive** (for Steam VR headsets)

### 3. Scene Setup
1. Open `Assets/Scenes/StreetCrossingVR.unity`
2. The scene should automatically generate the street environment
3. VR components will be set up automatically by the VRSceneManager

### 4. Build Settings
1. Go to **File → Build Settings**
2. Select your target platform (Android for Quest, Windows for PC VR)
3. Click **Player Settings** and configure:
   - **XR Settings**: Ensure VR SDKs are properly configured
   - **Android Settings**: Set minimum API level to 23+ for Quest

## How to Use

### VR Controls
1. **Put on VR Headset**: Start the application
2. **Look Around**: Use natural head movement to observe traffic
3. **Movement Buttons**: 
   - **Green Button**: Move forward one step
   - **Red Button**: Move backward one step
4. **Traffic Light**: Wait for green light before crossing
5. **Safety**: Stay in designated crossing areas

### Training Scenarios
- **Basic Crossing**: Wait for green light, look both ways, cross safely
- **Traffic Awareness**: Observe vehicles stopping at intersections
- **Timing Practice**: Learn appropriate crossing timing
- **Emergency Situations**: Practice stopping mid-crossing if needed

## Customization

### Adjusting Movement
Edit `VRPlayerController.cs`:
```csharp
[SerializeField] private float stepSize = 0.5f;        // Distance per step
[SerializeField] private float movementSpeed = 2f;     // Movement speed
```

### Traffic Light Timing
Edit `TrafficLightController.cs`:
```csharp
[SerializeField] private float stopDuration = 30f;    // Red light duration
[SerializeField] private float goDuration = 15f;      // Green light duration
```

### Vehicle Behavior
Edit `VehicleAI.cs`:
```csharp
[SerializeField] private float normalSpeed = 5f;      // Normal driving speed
[SerializeField] private float stoppingDistance = 5f; // Distance to stop at lights
```

## Development Notes

### Performance Optimization
- Uses object pooling for vehicles
- LOD system for distant objects
- Optimized materials for VR rendering
- Efficient collision detection

### Accessibility Features
- Audio cues for traffic lights
- High contrast button colors
- Adjustable movement speed
- Clear visual indicators

### Safety Features
- Boundary detection prevents falling off sidewalks
- Emergency stop functionality
- Safe zone indicators
- Collision prevention

## Contributing

To contribute to this project:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test in VR thoroughly
5. Submit a pull request

### Testing Checklist
- [ ] VR headset tracking works properly
- [ ] Movement buttons respond correctly
- [ ] Traffic lights cycle as expected
- [ ] Vehicles stop at red lights
- [ ] Audio feedback functions properly
- [ ] No motion sickness issues
- [ ] Performance maintains 72+ FPS

## License

This project is created for educational and training purposes. Please ensure proper attribution when using or modifying this code.

## Support

For issues or questions:
1. Check the Unity Console for error messages
2. Verify VR headset is properly connected
3. Ensure all required packages are installed
4. Test on a simple scene first to isolate issues

## Version History

- **v1.0.0**: Initial release with basic VR street crossing functionality
- Core features: Player movement, traffic lights, vehicle AI, VR interactions