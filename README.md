# Unity Multiplayer Movement System

## About This Project

This is a functional movement system with multiplayer support using Photon Fusion 2 in shared mode. The project demonstrates networking capabilities, responsive character controls, and customization options in a multiplayer environment.

## Features

- **Smooth Character Controller**: A physics-based movement system with interpolated transitions for natural movement
- **Input Handling**: Fully implemented using Unity's new Input System
- **Controls**:
    - WASD/Arrow Keys for movement
    - Mouse for rotation/camera control
    - Spacebar for jumping
    - Shift for sprinting
- **Multiplayer Integration**: Photon Fusion 2 shared mode implementation
- **Character Customization**: Change player names and colors through the in-game UI
- **Network Synchronization**: Real-time movement updates across all connected clients

## Setup Instructions

### Requirements
- Unity `v2022.3.34f1`
- Photon Fusion 2 (included in the project)

### Installation
1. Clone this repository or download the zip file
2. Open the project in Unity
3. Open the `Scenes/TestScene` scene
4. Press Play to test in single-player mode, or use ParrelSync to test multiplayer

### Multiplayer Testing
For local multiplayer testing:
1. Use ParrelSync to create a clone of the project (Window > ParrelSync > Clones Manager)
2. Open the original project and the clone
3. Press Play in both instances
4. Enter the same Room ID in both instances to connect to the same session

## Directories

- `Assets/Scripts/`: Contains all script files for the project
    - `Gameplay/`: Scripts related to gameplay mechanics
    - `Network/`: Scripts related to networking and multiplayer functionality
    - `Camera/`: Scripts related to camera control and movement
    - `UI/`: Scripts related to user interface elements and interactions
- `Assets/Prefabs/`: Pre-built game objects and UI elements ready for use
- `Assets/InputSystem/`: Input configuration and controls
- `Assets/Scenes/`: Scenes for the game, including the TestScene for testing
- `Assets/Photon/`: Photon Fusion 2 plugin for networking and multiplayer

## Implementation Details

### Movement System
The player controller uses a character controller component with physics-based interpolation for smooth movement. Weight factors affect speed, jump height, and gravity for a more realistic feel.

### Networking
The project uses Photon Fusion 2's shared mode for networking. Movement data is networked through the `NetworkInputData` struct and synchronized across clients.

### Customization
Players can customize their displayed name and character color through the debug UI, which are networked to be visible to all players.

## Tools and Libraries Used

- **Unity 2022.3.34f1**: Game engine
- **Photon Fusion 2**: Networking solution
- **TextMeshPro**: Enhanced text rendering
- **Cinemachine**: Camera system
- **Unity Input System**: Input handling
- **FlexibleColorPicker**: For color customization
- **ParrelSync**: Local multiplayer testing

## Building and Deploying

The project can be built for various platforms through Unity's standard build process. Note that the included settings are optimized for desktop platforms.