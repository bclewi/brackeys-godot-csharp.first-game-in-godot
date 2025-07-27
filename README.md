# Brackeys First Game in C# with Basic Multiplayer

This is a fork of [the C# version of Brackeys first Godot project](https://github.com/brackeys-godot-csharp/first-game-in-godot) with an added basic multiplayer implementation ported from https://github.com/BatteryAcid/first-game-in-godot/.

Uses MultiplayerSynchronizer, MultiplayerSpawner, and RPCs to sync player position, animation, and the moving platform in the game.

- Multiplayer tutorial (GDScript): https://youtu.be/V4a_J38XdHk
- Brackeys tutorial (GDScript): https://www.youtube.com/watch?v=LOhfqjmasi0

## C# Porting Progress
> This example is in progress and does not currently work as intended. See below for details on the remaining issue to be fixed.

### Repro Steps

> If debugging from VS Code/Codium, change the `launch.json` as needed for your IDE, making sure it points to the local install of Godot 4.4.1. I only added VS Codium setup and use the GODOT environment variable to point to my local install.

- Clone and open the project for editing in Godot Mono 4.4.1.
- In the editor, go to Debug > "Customize Run Instances..."
  - Enable Multiple Instances should be checked.
  - Change the number of instances to 2 (or higher if desired).
  - Click the "OK" button.
- Run the project.
- From one of the instances, click "Host New Game".
- From another instance, click "Join as Player 2".
- BUG: It isn't working as expected. Observe the following:
  - The Player 2 instance has no players spawned in the "Players" node. There may or may not be errors in the debug log as shown below.
    - ![Player2Instance.png](https://github.com/bclewi/brackeys-godot-csharp.first-game-in-godot/blob/main/screenshots/Player2Instance.png?raw=true)
  - The Player 2 Game Window has no player with an active camera and shows the level zoomed out.
    - ![Player2InstanceGameWindow.png](https://github.com/bclewi/brackeys-godot-csharp.first-game-in-godot/blob/main/screenshots/Player2InstanceGameWindow.png?raw=true)
  - The Host instance is unable to synchronize with other instances and throws many errors in the debug log as shown below.
    - ![HostInstance1.png](https://github.com/bclewi/brackeys-godot-csharp.first-game-in-godot/blob/main/screenshots/HostInstance1.png?raw=true)
     - ![HostInstance2.png](https://github.com/bclewi/brackeys-godot-csharp.first-game-in-godot/blob/main/screenshots/HostInstance2.png?raw=true)
  - Both the Host an the Player 2 move with the same inputs in the Host Game Window.
    - ![HostInstanceGameWindow.png](https://github.com/bclewi/brackeys-godot-csharp.first-game-in-godot/blob/main/screenshots/HostInstanceGameWindow.png?raw=true)

