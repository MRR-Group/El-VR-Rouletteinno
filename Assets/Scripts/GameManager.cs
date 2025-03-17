using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<GameState> gameState = new (GameState.PREPARE);
    
    private NetworkVariable<int> participated_players = new (0);
    
    private const int MAX_PLAYERS = 4;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Init even");
        this.participated_players.OnValueChanged += OnPlayerValueChanged;
    }

    private void OnPlayerValueChanged(int _, int players)
    {
        Debug.Log("OnPlayerValueChanged " + players);
        
        if (gameState.Value != GameState.PREPARE)
        {
            return;
        }

        if (players == MAX_PLAYERS)
        {
            return;
        }
        
        Debug.Log("IN_PROGRESS ");
        gameState.Value = GameState.IN_PROGRESS;
    }
    
    public void AddPlayer(ulong player)
    {
        Debug.Log("Added player " + player);
        participated_players.Value += 1;
    }
}
