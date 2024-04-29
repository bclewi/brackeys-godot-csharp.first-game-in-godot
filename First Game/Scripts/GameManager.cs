using Godot;

namespace FirstGame.Scripts;

public sealed partial class GameManager : Node
{
    private int _score;
    private Label _scoreLabel = default!;
    
    public override void _Ready()
    {
        _score = 0;
        _scoreLabel = this.GetNodeOrThrow<Label>("ScoreLabel");
    }

    public void AddPoint()
    {
        _score++;
        _scoreLabel.Text = $"You collected {_score} coins.";
    }
}
