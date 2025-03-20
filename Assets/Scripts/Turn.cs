using System;
using Unity.Netcode;

public class Turn : NetworkBehaviour
{
    private NetworkVariable<ulong> net_currentPlayerTurn = new ();
    public event EventHandler TurnChanged;
    
    public override void OnNetworkSpawn()
    {
        net_currentPlayerTurn.OnValueChanged += (_, __) => TurnChanged?.Invoke(null, null);
    }
    
    public bool IsClientTurn()
    {
        return IsPlayerTurn(NetworkManager.Singleton.LocalClient.ClientId);
    }
    
    public bool IsPlayerTurn(ulong player)
    {
        return net_currentPlayerTurn.Value == player;
    }

    [Rpc(SendTo.Server)]
    public void NextTurnRpc()
    {
        var game = GameManager.Instance.Game;
        
        if (game.ShouldEndGame()) {
            game.PlayerWinGameRpc(net_currentPlayerTurn.Value);
            return;
        }
        
        net_currentPlayerTurn.Value = game.GetNextPlayer(net_currentPlayerTurn.Value);
    }
}
