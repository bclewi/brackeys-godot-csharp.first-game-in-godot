using Godot;

namespace FirstGame.Scripts.Multiplayer.Networks;

public partial class NetworkManager : Node
{
    [Export] public Node2D PlayersSpawnNode { get; set; } = default!;

    public MultiplayerNetworkType ActiveNetworkType { get; set; } = MultiplayerNetworkType.ENet;
    private PackedScene _enetNetworkScene = GD.Load<PackedScene>("res://scenes/multiplayer/networks/enet_network.tscn");
    private PackedScene _steamNetworkScene = GD.Load<PackedScene>("res://scenes/multiplayer/networks/steam_network.tscn");
    private INetwork _activeNetwork = default!;

    public void BuildMultiplayerNetwork()
    {
        if (_activeNetwork is null)
        {
            GD.Print($"Setting {nameof(_activeNetwork)}");

            MultiplayerManager.Instance.MultiplayerModeEnabled = true;

            switch (ActiveNetworkType)
            {
                case MultiplayerNetworkType.ENet:
                    GD.Print("Setting network type to Enet");
                    SetActiveNetwork(_enetNetworkScene);
                    break;
                case MultiplayerNetworkType.Steam:
                    GD.Print("Setting network type to Steam");
                    SetActiveNetwork(_steamNetworkScene);
                    break;
                default:
                    GD.Print("No match for network type!");
                    break;
            }
        }
    }

    private void SetActiveNetwork(PackedScene activeNetworkScene)
    {
        var networkSceneInitialized = activeNetworkScene.Instantiate<INetwork>();
        _activeNetwork = networkSceneInitialized;
        _activeNetwork.PlayersSpawnNode = PlayersSpawnNode;
        AddChild((Node)_activeNetwork);
    }

    public void BecomeHost(bool isDedicatedServer = false)
    {
        BuildMultiplayerNetwork();
        MultiplayerManager.Instance.HostModeEnabled = !isDedicatedServer;
        _activeNetwork.BecomeHost();
    }

    public void JoinAsClient(ulong lobbyId = 0)
    {
        BuildMultiplayerNetwork();
        _activeNetwork.JoinAsClient(lobbyId);
    }

    public void ListLobbies()
    {
        BuildMultiplayerNetwork();
        _activeNetwork.ListLobbies();
    }
}
