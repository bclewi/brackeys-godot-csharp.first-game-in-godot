# Brackeys First Game in C# with Steam Multiplayer

This is a fork of [the C# version of Brackeys first Godot project](https://github.com/brackeys-godot-csharp/first-game-in-godot) with an added Steam P2P multiplayer implementation ported from https://github.com/BatteryAcid/godot-steam-multiplayer-peer-extension.

> This example project does not currently work as intended, either because I set up the C# Bindings inccorectly, or because the C# Bindings are not working anymore. See https://github.com/LauraWebdev/GodotSteam_CSharpBindings/issues/42 for the current status of this issue.

Uses MultiplayerSynchronizer, MultiplayerSpawner, and RPCs to sync player position, animation, and the moving platform in the game.

- GodotSteam GDExtension + Steam Multiplayer Peer tutotial (GDScript): https://youtu.be/xugYYCz0VHU
- Basic multiplayer tutorial (GDScript): https://youtu.be/V4a_J38XdHk
- Brackeys tutorial (GDScript): https://www.youtube.com/watch?v=LOhfqjmasi0
- GodotSteam GDExtesion 4.4: https://godotengine.org/asset-library/asset/2445
- GodotSteam C# Bindings: https://github.com/LauraWebdev/GodotSteam_CSharpBindings/tree/refresh-jan-2025/addons/godotsteam_csharpbindings
- Steam Multiplayer Peer: https://godotengine.org/asset-library/asset/2258
- Steam Multiplayer Peer C# Bindings: https://github.com/expressobits/steam-multiplayer-peer/tree/csharp/steam-multiplayer-peer-csharp-bindings

### Local Testing

- In the editor, go to Debug > "Customize Run Instances..."
  - Enable Multiple Instances should be checked.
  - Change the number of instances to 2.
  - Click the "OK" button.
- Run the project.
- From one of the instances, click "Host New Game".
- From another instance, click "Join as Client".
