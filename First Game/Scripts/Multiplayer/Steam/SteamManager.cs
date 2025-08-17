using Godot;
using Steamworks;

namespace FirstGame.Scripts.Multiplayer;

public partial class SteamManager : Node
{
    public static SteamManager Instance { get; private set; } = default!;

    private bool _isOwned;
    private const int SteamAppId = 480; // Test game app id
    private ulong _steamId = 0;
    public string SteamUsername { get; private set; } = string.Empty;

    private int _lobbyId;
    public int LobbyMaxMembers { get; private set; } = 4;

    private bool _isSteamInitialized;

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

        if (_isSteamInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    public void InitializeSteam()
    {
        var steamApiInitResult = SteamAPI.InitEx(out var steamErrMsg);
        GD.Print($"Did Steam Initialize? Result: {steamApiInitResult}, Error Message: {steamErrMsg}");

        if (steamApiInitResult != ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
        {
            GD.Print($"Failed to init Steam! Shutting down.");
            GetTree().Quit();
        }

        _isOwned = SteamApps.BIsSubscribed();
        _steamId = SteamUser.GetSteamID().GetAccountID().m_AccountID;
        SteamUsername = SteamFriends.GetPersonaName();

        GD.Print($"{nameof(_steamId)} {_steamId}");

        if (!_isOwned)
        {
            GD.Print("User does not own game!");
            GetTree().Quit();
        }

        _isSteamInitialized = true;
    }
}
