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
    }
    
    private void HandleTimerTimeout()
    {
        Engine.TimeScale = NormalTimeScale;
        GetTree().ReloadCurrentScene();
    }
    
    private void HandleBodyEntered(Node2D body)
    {
        GD.Print("You died!");
        Engine.TimeScale = SlowTimeScale;
        body.GetNode<CollisionShape2D>(nameof(CollisionShape2D)).QueueFree();
        _timer.Start();
    }
}
