using Godot;
using SteamMultiplayerPeerCSharp;
using Steamworks;
using System;
using System.Text.Json;

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

    private CallResult<LobbyCreated_t> _lobbyCreatedResult;
    private CallResult<LobbyEnter_t> _lobbyJoinedResult;
    private CallResult<LobbyMatchList_t> _requestLobbyListResult;

    public class RequestLobbyListEventArgs : EventArgs
    {
        public LobbyMatchList_t PCallback;
        public bool BIOFailure;
    }
    public event EventHandler<RequestLobbyListEventArgs> RequestLobbyListCompleted;

    public override void _EnterTree()
    {
        base._EnterTree();

        Instance = this;

        _multiplayerPeer = new();

        // https://github.com/expressobits/steam-multiplayer-peer/issues/15
        // Each property to synchronize via MultiplayerSynchronizer should have the Replicate option set to "On Change"
        // to minimize the amount of data needs to be sent over the wire. However, updating the config of MultiplayerPeer
        // may be needed in an actual game depending on how much data is being syncrhonized, as is shown here.
        //_multiplayerPeer.SetConfig(SteamNetworkingConfig.SendBufferSize, 524288);
        //_multiplayerPeer.SetConfig(SteamNetworkingConfig.RecvBufferSize, 524288);
        //_multiplayerPeer.SetConfig(SteamNetworkingConfig.SendRateMax, 524288);
    }

    public void BecomeHost()
    {
        GD.Print("Starting host!");

        Multiplayer.PeerConnected += AddPlayerToGame;
        Multiplayer.PeerDisconnected += DelPlayer;

        _lobbyCreatedResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        var handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, SteamManager.Instance.LobbyMaxMembers);
        _lobbyCreatedResult.Set(handle);
    }

    public void JoinAsClient(ulong lobbyId = 0)
    {
        GD.Print($"Joining lobby {lobbyId}");

        var steamIDLobby = new CSteamID(lobbyId);

        _lobbyJoinedResult = CallResult<LobbyEnter_t>.Create(OnLobbyJoined);
        var handle = SteamMatchmaking.JoinLobby(steamIDLobby);
        _lobbyJoinedResult.Set(handle);
    }

    private void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)//long connect, ulong lobbyId)
    {
        GD.Print("On Lobby created");
        if (pCallback.m_eResult != EResult.k_EResultOK || bIOFailure)
        {
            GD.Print($"Error creating lobby. IOFailure: {bIOFailure}, Result: {JsonSerializer.Serialize(pCallback)}");
        }

        _hostedLobbyId = pCallback.m_ulSteamIDLobby;

        var steamHostedLobbyId = new CSteamID(_hostedLobbyId);
        SteamMatchmaking.SetLobbyJoinable(steamHostedLobbyId, bLobbyJoinable: true);
        SteamMatchmaking.SetLobbyData(steamHostedLobbyId, "name", LobbyName);
        SteamMatchmaking.SetLobbyData(steamHostedLobbyId, "mode", LobbyMode);

        CreateHost();

        GD.Print($"Created lobby: {_hostedLobbyId}");
    }

    private void CreateHost()
    {
        GD.Print("Create Host");
        var error = _multiplayerPeer.CreateHost(0);
        if (error == Godot.Error.Ok)
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

    private void OnLobbyJoined(LobbyEnter_t pCallback, bool bIOFailure)
    {
        GD.Print($"On lobby joined: {pCallback.m_EChatRoomEnterResponse}");

        if (pCallback.m_EChatRoomEnterResponse == 1)
        {
            var lobbyOwnerSteamID = SteamMatchmaking.GetLobbyOwner(new CSteamID(pCallback.m_ulSteamIDLobby));
            if (lobbyOwnerSteamID != SteamUser.GetSteamID())
            {
                GD.Print("Connecting client to socket...");
                ConnectSocket(lobbyOwnerSteamID);
            }
        }
        else
        {
            var failReason = pCallback.m_EChatRoomEnterResponse switch
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

    private void ConnectSocket(CSteamID steamId)
    {
        var error = _multiplayerPeer.CreateClient(steamId.GetAccountID().m_AccountID, 0);
        if (error == Godot.Error.Ok)
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
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        // NOTE: If you are using the test app id, you will need to apply a filter on your game name
        // Otherwise, it may not show up in the lobby list of your clients
        SteamMatchmaking.AddRequestLobbyListStringFilter("name", LobbyName, ELobbyComparison.k_ELobbyComparisonEqual);
        _requestLobbyListResult = CallResult<LobbyMatchList_t>.Create(OnRequestLobbyListCompleted);
        var handle = SteamMatchmaking.RequestLobbyList();
        _requestLobbyListResult.Set(handle);
    }

    private void OnRequestLobbyListCompleted(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        RequestLobbyListCompleted?.Invoke(this, new()
        {
            PCallback = pCallback,
            BIOFailure = bIOFailure
        });
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
