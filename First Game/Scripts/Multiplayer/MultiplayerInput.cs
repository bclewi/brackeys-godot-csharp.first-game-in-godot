using Godot;

namespace FirstGame.Scripts.Multiplayer;

public partial class MultiplayerInput : MultiplayerSynchronizer
{
    private static class InputActionName
    {
        public static StringName MoveLeft { get; } = "move_left";
        public static StringName MoveRight { get; } = "move_right";
        public static StringName Jump { get; } = "jump";
    }

    private MultiplayerController _player = default!;

    public float InputDirection { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        _player = GetParent<MultiplayerController>();

        if (GetMultiplayerAuthority() != Multiplayer.GetUniqueId())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
        }

        InputDirection = Input.GetAxis(InputActionName.MoveLeft, InputActionName.MoveRight);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        InputDirection = Input.GetAxis(InputActionName.MoveLeft, InputActionName.MoveRight);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed(InputActionName.Jump))
        {
            Rpc(MethodName.Jump);
        }
    }

    [Rpc(CallLocal = true)]
    private void Jump()
    {
        if (Multiplayer.IsServer())
        {
            _player.DoJump = true;
        }
    }
}
