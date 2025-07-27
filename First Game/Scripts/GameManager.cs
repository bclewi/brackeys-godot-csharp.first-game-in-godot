using FirstGame.Scripts.Multiplayer;
using Godot;

namespace FirstGame.Scripts;

public sealed partial class GameManager : Node
{
    private int _score;
    private Label _scoreLabel = default!;
    private Control _multiplayerHUD = default!;
    private Button _hostGameButton = default!;
    private Button _joinAsPlayer2Button = default!;

    public override void _Ready()
    {
        _score = 0;
        _scoreLabel = this.GetNodeOrThrow<Label>("ScoreLabel");
        _multiplayerHUD = this.GetNodeOrThrow<Control>("%MultiplayerHUD");
        _hostGameButton = this.GetNodeOrThrow<Button>("%HostGame");
        _joinAsPlayer2Button = this.GetNodeOrThrow<Button>("%JoinAsPlayer2");

        _hostGameButton.Pressed += BecomeHost;
        _joinAsPlayer2Button.Pressed += JoinAsPlayer2;
    }

    public void AddPoint()
    {
        _score++;
        _scoreLabel.Text = $"You collected {_score} coins.";
    }

    public void BecomeHost()
    {
        GD.Print("Become host pressed");
        _multiplayerHUD.Hide();
        MultiplayerManager.Instance.BecomeHost();
    }

    public void JoinAsPlayer2()
    {
        GD.Print("Join as player 2");
        _multiplayerHUD.Hide();
        MultiplayerManager.Instance.JoinAsPlayer2();
    }
}
