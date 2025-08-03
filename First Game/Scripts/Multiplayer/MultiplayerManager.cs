using System;
using Godot;

namespace FirstGame.Scripts.Multiplayer;

public sealed partial class MultiplayerManager : Node
{
    private const int ServerPort = 8080;
    private const string ServerIP = "127.0.0.1";

    public static MultiplayerManager Instance { get; internal set; } = default!;

    private PackedScene _multiplayerScene = GD.Load<PackedScene>("res://scenes/multiplayer_player.tscn");

    private Node2D _playersSpawnNode = default!;

    public bool HostModeEnabled { get; private set; }
    public bool MultiplayerModeEnabled { get; private set; }
    public Vector2 RespawnPoint { get; init; } = new(30, 20);

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;
    }

    public void BecomeHost()
    {
        GD.Print("Starting host!");

        _playersSpawnNode = GetTree().CurrentScene.GetNodeOrThrow<Node2D>("Players");

        MultiplayerModeEnabled = true;
        HostModeEnabled = true;

        var serverPeer = new ENetMultiplayerPeer();
        var error = serverPeer.CreateServer(ServerPort);
        if (error != Error.Ok)
        {
            GD.PushError(new InvalidOperationException("Unable to create Host."
                + $" {nameof(serverPeer.CreateServer)}({nameof(ServerPort)}: {ServerPort})"
                + $" returned {nameof(Error)} code: {error}"));
        }

        Multiplayer.MultiplayerPeer = serverPeer;

        Multiplayer.PeerConnected += (id) => AddPlayerToGame(id);
        Multiplayer.PeerDisconnected += (id) => DeletePlayer(id);

        RemoveSinglePlayer();

        if (!OS.HasFeature("dedicated_server"))
        {
            AddPlayerToGame(1);
        }
    }

    public void JoinAsPlayer2()
    {
        GD.Print("Player 2 joining");

        MultiplayerModeEnabled = true;

        var clientPeer = new ENetMultiplayerPeer();
        var error = clientPeer.CreateClient(ServerIP, ServerPort);
        if (error != Error.Ok)
        {
            GD.PushError(new InvalidOperationException("Unable to join as Player 2."
                + $" {nameof(clientPeer.CreateClient)}({nameof(ServerIP)}: {ServerIP}, {nameof(ServerPort)}: {ServerPort})"
                + $" returned {nameof(Error)} code: {error}"));
        }

        Multiplayer.MultiplayerPeer = clientPeer;

        RemoveSinglePlayer();
    }

    private void AddPlayerToGame(long id)
    {
        GD.Print($"Player {id} joined the game!");

        if (id > int.MaxValue)
        {
            GD.PushError(new InvalidOperationException($"Unable to assign the ID {id} to the player"
                + $" because only values up to {int.MaxValue} are supported by MultiplayerSynchronizer."));
        }

        var playerToAdd = _multiplayerScene.Instantiate<MultiplayerController>();
        playerToAdd.PlayerId = (int)id;
        playerToAdd.Name = id.ToString();

        _playersSpawnNode.AddChild(playerToAdd, true);
    }

    private void DeletePlayer(long id)
    {
        GD.Print($"Player {id} left the game!");

        if (!_playersSpawnNode.HasNode(id.ToString()))
        {
            return;
        }
        _playersSpawnNode.GetNode(id.ToString()).QueueFree();
    }

    private void RemoveSinglePlayer()
    {
        GD.Print("Removed single player");

        var playerToRemove = GetTree().CurrentScene.GetNode("Player");
        playerToRemove.QueueFree();
    }
}
