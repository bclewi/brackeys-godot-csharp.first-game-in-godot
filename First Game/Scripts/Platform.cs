using Godot;

namespace FirstGame.Scripts;

public sealed partial class Platform : AnimatableBody2D
{
    [Export] private AnimationPlayer AnimationPlayer { get; set; }

    public override void _Ready()
    {
        base._Ready();

        if (AnimationPlayer is not null)
        {
            Multiplayer.PeerConnected += OnPlayerConnected;
        }
    }

    private void OnPlayerConnected(long id)
    {
        if (!Multiplayer.IsServer())
        {
            AnimationPlayer?.Stop();
            AnimationPlayer?.SetActive(false);
        }
    }
}