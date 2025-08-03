using Godot;

namespace FirstGame.Scripts.Multiplayer;

public partial class MultiplayerController : CharacterBody2D
{
    private const float Speed = 130f;
    private const float JumpVelocity = -300f;

    private AnimatedSprite2D _animatedSprite = default!;

    private float _gravity = default!;

    private MultiplayerInput _inputSynchronizer = default!;
    private Camera2D _camera2D = default!;
    private CollisionShape2D _collisionShape2D = default!;
    private Timer _respawnTimer = default!;

    public float Direction { get; private set; } = 1;
    public bool DoJump { get; set; }
    public bool OnFloor { get; private set; } = true;
    public bool Alive { get; private set; } = true;

    private int _playerId = 1;
    [Export] public int PlayerId
    {
        get => _playerId;
        set
        {
            _playerId = value;

            if (_inputSynchronizer is null)
            {
                _inputSynchronizer = this.GetNodeOrThrow<MultiplayerInput>("%InputSynchronizer");
            }
            _inputSynchronizer.SetMultiplayerAuthority(value);
        }
    }

    public override void _Ready()
    {
        base._Ready();

        _animatedSprite = this.GetNodeOrThrow<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

        _inputSynchronizer ??= this.GetNodeOrThrow<MultiplayerInput>("%InputSynchronizer");
        _camera2D = this.GetNodeOrThrow<Camera2D>(nameof(Camera2D));
        _collisionShape2D = this.GetNodeOrThrow<CollisionShape2D>(nameof(CollisionShape2D));
        _respawnTimer = this.GetNodeOrThrow<Timer>("RespawnTimer");

        if (Multiplayer.GetUniqueId() == PlayerId)
        {
            _camera2D.MakeCurrent();
        }
        else
        {
            _camera2D.Enabled = false;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (Multiplayer.IsServer())
        {
            if (!Alive && IsOnFloor())
            {
                SetAlive();
            }

            OnFloor = IsOnFloor();
            ApplyMovementFromInput(delta);
        }

        if (!Multiplayer.IsServer() || MultiplayerManager.Instance.HostModeEnabled)
        {
            ApplyAnimations();
        }
    }

    public void MarkDead()
    {
        GD.Print("Mark player dead!");
        Alive = false;
        _collisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _respawnTimer.Start();
    }

    public void Respawn()
    {
        GD.Print("Respawned!");
        Position = MultiplayerManager.Instance.RespawnPoint;
        _collisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
    }

    private void ApplyAnimations()
    {
        if (Direction > 0)
        {
            _animatedSprite.FlipH = false;
        }
        else if (Direction < 0)
        {
            _animatedSprite.FlipH = true;
        }

        if (OnFloor)
        {
            if (Direction == 0)
            {
                _animatedSprite.Play("idle");
            }
            else
            {
                _animatedSprite.Play("run");
            }
        }
        else
        {
            _animatedSprite.Play("jump");
        }
    }

    private void ApplyMovementFromInput(double delta)
    {
        // Add the gravity.
        if (!IsOnFloor())
        {
            Velocity = Velocity with
            {
                Y = Velocity.Y + (_gravity * (float)delta)
            };
        }

        // Handle jump.
        if (DoJump && IsOnFloor())
        {
            Velocity = Velocity with
            {
                Y = JumpVelocity
            };
            DoJump = false;
        }

        Direction = _inputSynchronizer.InputDirection;

        if (Direction != 0f)
        {
            Velocity = Velocity with
            {
                X = Direction * Speed
            };
        }
        else
        {
            Velocity = Velocity with
            {
                X = Mathf.MoveToward(Velocity.X, 0, Speed)
            };
        }

        MoveAndSlide();
    }

    private void SetAlive()
    {
        GD.Print("alive again!");
        Alive = true;
        Engine.TimeScale = 1.0;
    }
}
