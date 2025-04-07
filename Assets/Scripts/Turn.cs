using System;
using Unity.Netcode;
using XRMultiplayer;

public class Turn : NetworkBehaviour
{
    private NetworkVariable<ulong> net_currentPlayerTurn = new ();
    public event EventHandler TurnChanged;

    public ulong CurrentPlayerId => net_currentPlayerTurn.Value;
    
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            net_currentPlayerTurn.Value = 0;
        }

        net_currentPlayerTurn.OnValueChanged += (_, __) => TurnChanged?.Invoke(null, null);
    }

    public override void OnNetworkDespawn()
    {
        net_currentPlayerTurn.OnValueChanged -= (_, __) => TurnChanged?.Invoke(null, null);
    }


    public bool IsClientTurn()
    {
        return IsPlayerTurn(NetworkManager.Singleton.LocalClient.ClientId);
    }
    
    public bool IsPlayerTurn(ulong player)
    {
        return net_currentPlayerTurn.Value == player && GameManager.Instance.Game.AreAllItemBoxesUsed();
    }

    [Rpc(SendTo.Server)]
    public void NextTurnRpc()
    {
        var game = GameManager.Instance.Game;
        
        if (game.ShouldEndGame()) {
            game.PlayerWinGameRpc(net_currentPlayerTurn.Value);
            return;
        }
        
        var nextPlayerId = game.GetNextPlayer(net_currentPlayerTurn.Value);
        var nextPlayer = PlayerManager.Instance.ById(nextPlayerId);
        
        net_currentPlayerTurn.Value = nextPlayerId;
        
        if (nextPlayer.IsHandcuffed)
        {
            NextTurnRpc();
            nextPlayer.UncuffRpc();
        }
    }
}
