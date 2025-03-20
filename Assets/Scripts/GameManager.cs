using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkSingleton<GameManager>
{
    [SerializeField]
    private NetworkVariable<GameState> gameState = new (GameState.PREPARE);
    public GameState GameState => gameState.Value;
    public event EventHandler<GameStateChangedArgs> GameStateChanged;
    public class GameStateChangedArgs : EventArgs
    {
        public GameState State;
    }
    
    [SerializeField] 
    private InputActionReference moveAction;
    
    public NetworkVariable<List<ulong>> playersIds = new (new List<ulong>());
    public List<Player> players = new List<Player>();
    
    [SerializeField]
    private int m_maxPlayers = 4;
    
    [SerializeField]
    private int m_minPlayers = 2;

    public Game game;
    public Round round;
    public Turn turn;
    
    public override void OnNetworkSpawn()
    {
        playersIds.OnValueChanged += OnPlayersIdsValueChanged;
        gameState.OnValueChanged += OnGameStateChanged;
    }
    
    private void OnPlayersIdsValueChanged(List<ulong> _, List<ulong> newPlayerList)
    {
        if (gameState.Value != GameState.PREPARE)
        {
            return;
        }

        players.Clear();
        
        foreach (var id in newPlayerList)
        {
            players.Add(PlayerManager.Instance.Player[id]);
        }
        
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (players.Count < m_minPlayers)
        {
            return;
        }

        if (players.Count >= m_maxPlayers || players.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGame();
        }
    }
    
    private void StartGame()
    {
        gameState.Value = GameState.IN_PROGRESS;
    }
    
    private void OnGameStateChanged(GameState _, GameState value)
    {
        GameStateChanged?.Invoke(this, new GameStateChangedArgs { State =  value });
        
        switch (value)
        {
            case GameState.PREPARE: 
                moveAction.action.Enable();
                break;
            
            case GameState.IN_PROGRESS:
                moveAction.action.Disable();
                break;
            
            case GameState.FINISHED:
                moveAction.action.Enable();
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void AddPlayerRpc(ulong player)
    {
        playersIds.Value.Add(player);
        playersIds.CheckDirtyState();
    }
    
    [Rpc(SendTo.Server)]
    public void EndGameRpc()
    {
        gameState.Value = GameState.FINISHED;
    }
}
