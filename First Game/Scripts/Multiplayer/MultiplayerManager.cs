using Godot;

namespace FirstGame.Scripts.Multiplayer;

public sealed partial class MultiplayerManager : Node
{
    public static MultiplayerManager Instance { get; internal set; } = default!;

    public bool HostModeEnabled { get; set; }
    public bool MultiplayerModeEnabled { get; set; }
    public Vector2 RespawnPoint { get; init; } = new(30, 20);

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;
    }
}
