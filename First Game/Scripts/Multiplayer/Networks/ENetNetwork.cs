using Godot;
using System;

namespace FirstGame.Scripts.Multiplayer.Networks;

public partial class ENetNetwork : Node, INetwork
{
    private const int ServerPort = 8080;
    private const string ServerIP = "127.0.0.1";

    public static ENetNetwork Instance { get; internal set; } = default!;

    private PackedScene _multiplayerScene = default!;
    private ENetMultiplayerPeer _multiplayerPeer = default!;
    public Node2D PlayersSpawnNode { get; set; } = default!;

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;
    }

    public override void _Ready()
    {
        base._Ready();

        _multiplayerScene = GD.Load<PackedScene>("res://scenes/multiplayer_player.tscn");
        _multiplayerPeer = new();
        PlayersSpawnNode = GetTree().CurrentScene.GetNodeOrThrow<Node2D>("Players");
    }

    public void BecomeHost()
    {
        GD.Print("Starting host!");

        var error = _multiplayerPeer.CreateServer(ServerPort);
        if (error != Error.Ok)
        {
            GD.PushError(new InvalidOperationException("Unable to create Host."
                + $" {nameof(_multiplayerPeer.CreateServer)}({nameof(ServerPort)}: {ServerPort})"
                + $" returned {nameof(Error)} code: {error}"));
        }
        Multiplayer.MultiplayerPeer = _multiplayerPeer;

        Multiplayer.PeerConnected += AddPlayerToGame;
        Multiplayer.PeerDisconnected += DelPlayer;

        if (!OS.HasFeature("dedicated_server"))
        {
            AddPlayerToGame(1);
        }
    }

    public void JoinAsClient(ulong lobbyId = 0)
    {
        GD.Print("Player 2 joining");

        var error = _multiplayerPeer.CreateClient(ServerIP, ServerPort);
        if (error != Error.Ok)
        {
            GD.PushError(new InvalidOperationException("Unable to join as Player 2."
                + $" {nameof(_multiplayerPeer.CreateClient)}({nameof(ServerIP)}: {ServerIP}, {nameof(ServerPort)}: {ServerPort})"
                + $" returned {nameof(Error)} code: {error}"));
        }
        Multiplayer.MultiplayerPeer = _multiplayerPeer;
    }

    public void ListLobbies()
    {
        throw new NotImplementedException();
    }

    private void AddPlayerToGame(long id)
    {
        if (id > int.MaxValue)
        {
            GD.PushError(new InvalidOperationException($"Unable to assign the ID {id} to the player"
                + $" because only values up to {int.MaxValue} are supported by MultiplayerSynchronizer."));
        }

        GD.Print($"Player {id} joined the game!");

        var playerToAdd = _multiplayerScene.Instantiate<MultiplayerController>();
        playerToAdd.PlayerId = (int)id;
        playerToAdd.Name = id.ToString();

        PlayersSpawnNode.AddChild(playerToAdd, true);
    }

    private void DelPlayer(long id)
    {
        GD.Print($"Player {id} left the game!");

        if (!PlayersSpawnNode.HasNode(id.ToString()))
        {
            return;
        }
        PlayersSpawnNode.GetNode(id.ToString()).QueueFree();
    }
}
