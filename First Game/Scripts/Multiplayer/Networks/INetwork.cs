using Godot;

namespace FirstGame.Scripts.Multiplayer.Networks;

public interface INetwork
{
    Node2D PlayersSpawnNode { get; set; }

    void BecomeHost();
    void JoinAsClient(ulong lobbyId = 0);
    void ListLobbies();
}
