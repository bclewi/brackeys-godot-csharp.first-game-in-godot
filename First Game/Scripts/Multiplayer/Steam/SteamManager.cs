using Godot;
using GodotSteam;
using System.Text.Json;

namespace FirstGame.Scripts.Multiplayer;

public partial class SteamManager : Node
{
    public static SteamManager Instance { get; private set; } = default!;

    private bool _isOwned;
    private const int SteamAppId = 480; // Test game app id
    private ulong _steamId = 0;
    public string SteamUsername { get; private set; } = string.Empty;

    private int _lobbyId;
    public long LobbyMaxMembers { get; private set; } = 4;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void Init()
    {
        GD.Print("Init Steam");
        OS.SetEnvironment("SteamAppId", SteamAppId.ToString());
        OS.SetEnvironment("SteamGameId", SteamAppId.ToString());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Steam.RunCallbacks();
    }

    public void InitializeSteam()
    {
        var initializeResponse = Steam.SteamInitEx(retrieveStats: false, SteamAppId);
        GD.Print($"Did Steam Initialize?: {JsonSerializer.Serialize(initializeResponse)}");

        if (initializeResponse.Status != SteamInitExStatus.SteamworksActive)
        {
            GD.Print($"Failed to init Steam! Shutting down. {JsonSerializer.Serialize(initializeResponse)}");
            GetTree().Quit();
        }

        _isOwned = Steam.IsSubscribed();
        _steamId = Steam.GetSteamID();
        SteamUsername = Steam.GetPersonaName();

        GD.Print($"{nameof(_steamId)} {_steamId}");

        if (!_isOwned)
        {
            GD.Print("User does not own game!");
            GetTree().Quit();
        }
    }
}
