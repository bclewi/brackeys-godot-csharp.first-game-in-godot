using FirstGame.Scripts.Multiplayer;
using Godot;

namespace FirstGame.Scripts;

public sealed partial class FinishLine : Area2D
{
    public override void _Ready()
    {
        base._Ready();

        BodyEntered += OnBodyEntered;
    }

    public void OnBodyEntered(Node2D body)
    {
        if (MultiplayerManager.Instance.MultiplayerModeEnabled
            && body is MultiplayerController player
            && Multiplayer.GetUniqueId() == player.PlayerId)
        {
            GD.Print($"Player {Multiplayer.GetUniqueId()} WINS!");
        }
    }
}
