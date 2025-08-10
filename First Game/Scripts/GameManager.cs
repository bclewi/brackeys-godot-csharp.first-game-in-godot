using FirstGame.Scripts.Multiplayer;
using FirstGame.Scripts.Multiplayer.Networks;
using Godot;
using GodotSteam;

namespace FirstGame.Scripts;

public sealed partial class GameManager : Node
{
    private readonly StringName Font = "font";
    private readonly StringName FontSize = "font_size";

    private NetworkManager _networkManager = default!;
    private Control _multiplayerHUD = default!;
    private Button _hostGameButton = default!;
    private Button _joinAsClientButton = default!;
    private Button _useSteamButton = default!;
    private Control _steamHUD = default!;
    private Button _listLobbiesButton = default!;
    private Button _hostP2PGame = default!;
    private VBoxContainer _lobbiesVBoxContainer = default!;

    private int _score;

    private Label _scoreLabel = default!;

    public override void _Ready()
    {
        _networkManager = this.GetNodeOrThrow<NetworkManager>("%NetworkManager");
        _multiplayerHUD = this.GetNodeOrThrow<Control>("%MultiplayerHUD");
        _hostGameButton = this.GetNodeOrThrow<Button>("%HostGame");
        _joinAsClientButton = this.GetNodeOrThrow<Button>("%JoinAsClient");
        _useSteamButton = this.GetNodeOrThrow<Button>("%UseSteam");
        _steamHUD = this.GetNodeOrThrow<Control>("%SteamHUD");
        _hostP2PGame = this.GetNodeOrThrow<Button>("%HostP2PGame");
        _listLobbiesButton = this.GetNodeOrThrow<Button>("%ListLobbies");
        _lobbiesVBoxContainer = this.GetNodeOrThrow<VBoxContainer>("%VBoxContainer");

        _score = 0;

        _scoreLabel = this.GetNodeOrThrow<Label>("ScoreLabel");

        _hostGameButton.Pressed += BecomeHost;
        _joinAsClientButton.Pressed += JoinAsClient;
        _useSteamButton.Pressed += UseSteam;
        _hostP2PGame.Pressed += BecomeHost;
        _listLobbiesButton.Pressed += ListSteamLobbies;

        if (OS.HasFeature("dedicated_server"))
        {
            GD.Print("Starting dedicated server...");
            RemoveSinglePlayer();
            _networkManager.BecomeHost(true);
        }
    }

    public void AddPoint()
    {
        _score++;
        _scoreLabel.Text = $"You collected {_score} coins.";
    }

    public void BecomeHost()
    {
        GD.Print("Become host pressed");
        RemoveSinglePlayer();
        _multiplayerHUD.Hide();
        _steamHUD.Hide();
        _networkManager.BecomeHost();
    }

    public void JoinAsClient()
    {
        GD.Print("Join as player 2");
        JoinLobby();
    }

    public void UseSteam()
    {
        GD.Print("Using Steam!");
        _multiplayerHUD.Hide();
        _steamHUD.Show();
        SteamManager.Instance.InitializeSteam();
        Steam.LobbyMatchList += OnLobbyMatchList;
        _networkManager.ActiveNetworkType = MultiplayerNetworkType.Steam;
    }

    public void ListSteamLobbies()
    {
        GD.Print("List Steam lobbies");
        _networkManager.ListLobbies();
    }

    private void JoinLobby(ulong lobbyId = 0)
    {
        GD.Print($"Joining lobby {lobbyId}");
        RemoveSinglePlayer();
        _multiplayerHUD.Hide();
        _steamHUD.Hide();
        _networkManager.JoinAsClient(lobbyId);
    }

    private void OnLobbyMatchList(Godot.Collections.Array lobbies)
    {
        GD.Print("On lobby match list");

        foreach (var lobbyChild in _lobbiesVBoxContainer.GetChildren())
        {
            lobbyChild.QueueFree();
        }

        foreach (var lobby in lobbies)
        {
            var lobbyId = lobby.As<ulong>();

            var lobbyName = Steam.GetLobbyData(lobbyId, "name");

            if (!string.IsNullOrWhiteSpace(lobbyName))
            {
                var lobbyMode = Steam.GetLobbyData(lobbyId, "mode");

                var lobbyButton = new Button();
                lobbyButton.Text = $"{lobbyName} | {lobbyMode}";
                lobbyButton.Size = new Vector2(100, 30);
                lobbyButton.AddThemeFontSizeOverride(FontSize, 8);

                var fv = new FontVariation();
                fv.BaseFont = GD.Load<Font>("res://assets/fonts/PixelOperator8.ttf");
                lobbyButton.AddThemeFontOverride(Font, fv);
                lobbyButton.Name = $"lobby_{lobbyId}";
                lobbyButton.Alignment = HorizontalAlignment.Left;
                lobbyButton.Pressed += () => JoinLobby(lobbyId);

                _lobbiesVBoxContainer.AddChild(lobbyButton);
            }
        }
    }

    private void RemoveSinglePlayer()
    {
        GD.Print("Removed single player");

        var playerToRemove = GetTree().CurrentScene.GetNode("Player");
        playerToRemove.QueueFree();
    }
}
