using System;
using Unity.Netcode;
using UnityEngine;

public class Turn : NetworkBehaviour
{
    private NetworkVariable<ulong> currentPlayerTurn = new ();
    public event EventHandler TurnChanged;
    
    [SerializeField]
    private int m_minAlivePlayers = 1;

    [SerializeField]
    private bool m_disableAliveChecker = false;
    
    public override void OnNetworkSpawn()
    {
        currentPlayerTurn.OnValueChanged += (_, __) => TurnChanged?.Invoke(null, null);
    }
    
    public bool IsClientTurn()
    {
        return IsPlayerTurn(NetworkManager.Singleton.LocalClient.ClientId);
    }
    
    public bool IsPlayerTurn(ulong player)
    {
        return currentPlayerTurn.Value == player;
    }

    [Rpc(SendTo.Server)]
    public void NextTurnRpc()
    {
        var players = GameManager.Instance.game.players.Value;
        
        if (ShouldEndGame(players.Count)) {
            GameManager.Instance.game.PlayerWinGameRpc(currentPlayerTurn.Value);
            
            return;
        }

        var index = players.IndexOf(currentPlayerTurn.Value);
        var next = index + 1 >= players.Count ? 0 : index + 1;
        
        currentPlayerTurn.Value = players[next];
    }

    private bool ShouldEndGame(int alivePlayers)
    {
        return alivePlayers <= m_minAlivePlayers && !m_disableAliveChecker;
    }
}
