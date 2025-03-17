using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<GameState> gameState = new (GameState.PREPARE);

    [SerializeField] private InputActionReference moveAction;
    
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

        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (players >= MAX_PLAYERS || players == NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGame();
        }
    }
    
    public void AddPlayer(ulong player)
    {
        Debug.Log("Added player " + player);
        participated_players.Value += 1;
    }

    private void StartGame()
    {
        Debug.Log("Starting game");
        gameState.Value = GameState.IN_PROGRESS;
        moveAction.action.Disable();
    }
}
