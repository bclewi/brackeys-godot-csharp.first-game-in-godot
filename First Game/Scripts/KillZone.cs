using FirstGame.Scripts.Multiplayer;
using Godot;

namespace FirstGame.Scripts;

public sealed partial class KillZone : Area2D
{
    private const double NormalTimeScale = 1D;
    private const double SlowTimeScale = NormalTimeScale / 2D;
    private Timer _timer = default!;

    public override void _Ready()
    {
        _timer = this.GetNodeOrThrow<Timer>("Timer");

        _timer.Timeout += OnTimerTimeout;
        BodyEntered += OnBodyEntered;
    }

    private void OnTimerTimeout()
    {
        Engine.TimeScale = NormalTimeScale;
        GetTree().ReloadCurrentScene();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!MultiplayerManager.Instance.MultiplayerModeEnabled)
        {
            GD.Print("You died!");
            Engine.TimeScale = SlowTimeScale;
            body.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).QueueFree();
            _timer.Start();
        }
        else if (body is MultiplayerController player)
        {
            MultiplayerDead(player);
        }
    }

    private void MultiplayerDead(MultiplayerController body)
    {
        if (Multiplayer.IsServer() && body.Alive)
        {
            Engine.TimeScale = SlowTimeScale;
            body.MarkDead();
        }
    }
}
