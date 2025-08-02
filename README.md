# Brackeys First Game in C# with Basic Multiplayer

This is a fork of [the C# version of Brackeys first Godot project](https://github.com/brackeys-godot-csharp/first-game-in-godot) with an added basic multiplayer implementation ported from https://github.com/BatteryAcid/first-game-in-godot/.

Uses MultiplayerSynchronizer, MultiplayerSpawner, and RPCs to sync player position, animation, and the moving platform in the game.

- Multiplayer tutorial (GDScript): https://youtu.be/V4a_J38XdHk
- Brackeys tutorial (GDScript): https://www.youtube.com/watch?v=LOhfqjmasi0

### Local Testing

- In the editor, go to Debug > "Customize Run Instances..."
  - Enable Multiple Instances should be checked.
  - Change the number of instances to 2.
  - Click the "OK" button.
- Run the project.
- From one of the instances, click "Host New Game".
- From another instance, click "Join as Player 2".

