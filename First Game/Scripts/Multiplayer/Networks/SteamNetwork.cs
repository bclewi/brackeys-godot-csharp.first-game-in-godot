using Godot;
using SteamMultiplayerPeerCSharp;
using System;
using GodotSteam;

namespace FirstGame.Scripts.Multiplayer.Networks;

public partial class SteamNetwork : Node, INetwork
{
    public static SteamNetwork Instance { get; internal set; } = default!;

    private PackedScene _multiplayerScene = GD.Load<PackedScene>("res://scenes/multiplayer_player.tscn");
    private SteamMultiplayerPeer _multiplayerPeer = default!;
    public Node2D PlayersSpawnNode { get; set; } = default!;
    private ulong _hostedLobbyId;

    private const string LobbyName = "BAD_CSHARP";
    private const string LobbyMode = "CoOP";

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;

        _multiplayerPeer = new();

        // https://github.com/expressobits/steam-multiplayer-peer/issues/15
        // Each property to synchronize via MultiplayerSynchronizer should have the Replicate option set to "On Change"
        // to minimize the amount of data needs to be sent over the wire. However, updating the config of MultiplayerPeer
        // may be needed in an actual game depending on how much data is being syncrhonized, as is shown here.
        _multiplayerPeer.SetConfig(SteamNetworkingConfig.SendBufferSize, 524288);
        _multiplayerPeer.SetConfig(SteamNetworkingConfig.RecvBufferSize, 524288);
        _multiplayerPeer.SetConfig(SteamNetworkingConfig.SendRateMax, 524288);
    }

    public override void _Ready()
    {
        base._Ready();

        Steam.LobbyCreated += OnLobbyCreated;
    }

    public override void _ExitTree()
    {
        Steam.LobbyCreated -= OnLobbyCreated;

        base._ExitTree();
    }

    public void BecomeHost()
    {
        GD.Print("Starting host!");

        Multiplayer.PeerConnected += AddPlayerToGame;
        Multiplayer.PeerDisconnected += DelPlayer;

        Steam.LobbyJoined += OnLobbyJoined;
        Steam.CreateLobby(Steam.LobbyType.Public, SteamManager.Instance.LobbyMaxMembers);
    }

    public void JoinAsClient(ulong lobbyId = 0)
    {
        GD.Print($"Joining lobby {lobbyId}");
        Steam.LobbyJoined += OnLobbyJoined;
        Steam.JoinLobby(lobbyId);
    }

    private void OnLobbyCreated(long connect, ulong lobbyId)
    {
        GD.Print("On Lobby created");
        if (connect == 1)
        {
            _hostedLobbyId = lobbyId;
            GD.Print($"Created lobby: {_hostedLobbyId}");

            Steam.SetLobbyJoinable(_hostedLobbyId, true);

            Steam.SetLobbyData(_hostedLobbyId, "name", LobbyName);
            Steam.SetLobbyData(_hostedLobbyId, "mode", LobbyMode);

            CreateHost();
        }
    }

    private void CreateHost()
    {
        GD.Print("Create Host");
        var error = _multiplayerPeer.CreateHost(0);
        if (error == Error.Ok)
        {
            Multiplayer.MultiplayerPeer = _multiplayerPeer.MultiplayerPeer;

            if (!OS.HasFeature("dedicated_server"))
            {
                AddPlayerToGame(1);
            }
        }
        else
        {
            GD.Print($"error creating host: {error}");
        }
    }

    private void OnLobbyJoined(ulong lobby, long permissions, bool locked, long response)
    {
        GD.Print($"On lobby joined: {response}");

        if (response == 1)
        {
            var id = Steam.GetLobbyOwner(lobby);
            if (id != Steam.GetSteamID())
            {
                GD.Print("Connecting client to socket...");
                ConnectSocket(id);
            }
        }
        else
        {
            var failReason = response switch
            {
                2 => "This lobby no longer exists.",
                3 => "You don't have permission to join this lobby.",
                4 => "The lobby is not full.",
                5 => "Uh... something unexpected happened!",
                6 => "You are banned from this lobby.",
                7 => "You cannot join due to having a limited account.",
                8 => "This lobby is locked or disabled.",
                9 => "This lobby is community locked.",
                10 => "A user in the lobby has blocked you from joining.",
                11 => "A user you have blocked is in the lobby.",
                _ => throw new NotImplementedException()
            };
            GD.Print(failReason);
        }
    }

    private void ConnectSocket(ulong steamId)
    {
        var error = _multiplayerPeer.CreateClient(steamId, 0);
        if (error == Error.Ok)
        {
            GD.Print("Connecting peer to host...");
            Multiplayer.MultiplayerPeer = _multiplayerPeer.MultiplayerPeer;
        }
        else
        {
            GD.Print($"Error creating client: {error}");
        }
    }

    public void ListLobbies()
    {
        Steam.AddRequestLobbyListDistanceFilter(Steam.LobbyDistanceFilter.Worldwide);
        // NOTE: If you are using the test app id, you will need to apply a filter on your game name
        // Otherwise, it may not show up in the lobby list of your clients
        Steam.AddRequestLobbyListStringFilter("name", LobbyName, Steam.LobbyComparison.LobbyComparisonEqual);
        Steam.RequestLobbyList();
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
