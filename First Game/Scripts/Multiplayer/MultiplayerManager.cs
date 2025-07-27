using System;
using Godot;

namespace FirstGame.Scripts.Multiplayer;

public sealed partial class MultiplayerManager : Node
{
    public static MultiplayerManager Instance { get; internal set; } = default!;

    private const int ServerPort = 8080;
    private const string ServerIP = "127.0.0.1";
    private const int MaxConnections = 4;

    private PackedScene _multiplayerScene = GD.Load<PackedScene>("res://scenes/multiplayer_player.tscn");

    private Node2D _playersSpawnNode = default!;

    public bool HostModeEnabled { get; private set; }
    public bool MultiplayerModeEnabled { get; private set; }
    public Vector2 RespawnPoint { get; init; } = new(30, 20);

    public class PlayerConnectedEventArgs : EventArgs
    {
        public required int PeerId { get; init; }
    }
    public event EventHandler<PlayerConnectedEventArgs>? PlayerConnected;

    public event EventHandler? ServerDisconnected;

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;
    }

    public override void _Ready()
    {
        _playersSpawnNode = GetTree().CurrentScene.GetNodeOrThrow<Node2D>("Players");
    }

    public void BecomeHost()
    {
        GD.Print("Starting host!");

        MultiplayerModeEnabled = true;
        HostModeEnabled = true;

        var serverPeer = new ENetMultiplayerPeer();
        var error = serverPeer.CreateServer(ServerPort, MaxConnections);
        if (error != Error.Ok)
        {
            GD.PushError(new InvalidOperationException("Unable to create Host."
                + $" {nameof(serverPeer.CreateServer)}({nameof(ServerPort)}: {ServerPort}, {nameof(MaxConnections)}: {MaxConnections})"
                + $" returned {nameof(Error)} code: {error}"));
        }

        Multiplayer.MultiplayerPeer = serverPeer;

        Multiplayer.PeerConnected += HandlePeerConnected;
        Multiplayer.PeerDisconnected += HandlePeerDisconnected;
        Multiplayer.ConnectedToServer += HandleConnectedToServer;
        Multiplayer.ConnectionFailed += HandleConnectionFailed;
        Multiplayer.ServerDisconnected += HandleServerDisconnected;

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
        if (id > int.MaxValue)
        {
            GD.PushError(new InvalidOperationException($"Unable to assign the ID {id} to the player"
                + $" because only values up to {int.MaxValue} are supported by MultiplayerSynchronizer."));
        }

        var playerToAdd = _multiplayerScene.Instantiate<MultiplayerController>();
        playerToAdd.PlayerId = (int)id;
        playerToAdd.Name = $"{id}";

        _playersSpawnNode.AddChild(playerToAdd, true);

        GD.Print($"Player {id} joined the game!");
    }

    private void DeletePlayer(long id)
    {
        if (!_playersSpawnNode.HasNode(id.ToString()))
        {
            return;
        }
        _playersSpawnNode.GetNode(id.ToString()).QueueFree();

        GD.Print($"Player {id} left the game!");
    }

    private void RemoveSinglePlayer()
    {
        var playerToRemove = GetTree().CurrentScene.GetNode("Player");
        playerToRemove.QueueFree();

        GD.Print("Removed single player");
    }

    private void HandlePeerConnected(long id)
    {
        AddPlayerToGame(id);
        GD.Print($"Peer {id} Connected.");
    }

    private void HandlePeerDisconnected(long id)
    {
        DeletePlayer(id);
        GD.Print($"Peer {id} Disconnected.");
    }

    private void HandleConnectedToServer()
    {
        PlayerConnected?.Invoke(this, new PlayerConnectedEventArgs()
        {
            PeerId = Multiplayer.GetUniqueId()
        });
        GD.Print($"Peer {Multiplayer.GetUniqueId()} Connected to the Server.");
    }

    private void HandleConnectionFailed()
    {
        Multiplayer.MultiplayerPeer = null;
        GD.PushError(new InvalidOperationException($"Peer {Multiplayer.GetUniqueId()} Connection Failed"));
    }

    private void HandleServerDisconnected()
    {
        Multiplayer.MultiplayerPeer = null;
        ServerDisconnected?.Invoke(this, EventArgs.Empty);
        GD.Print($"Server Disconnected from Peer {Multiplayer.GetUniqueId()}");
    }
}
