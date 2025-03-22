using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : NetworkSingleton<GameManager>
{
    private NetworkVariable<GameState> net_gameState = new ();
    public GameState GameState => net_gameState.Value;
    
    public event EventHandler<GameStateChangedArgs> GameStateChanged;
    public class GameStateChangedArgs : EventArgs
    {
        public GameState State;
    }
    
    [SerializeField] 
    private InputActionReference m_moveAction;
    
    private NetworkVariable<List<ulong>> net_playerInGame = new (new List<ulong>());
    
    public ulong[] InGamePlayers => net_playerInGame.Value.ToArray();
    
    [SerializeField]
    private int m_maxPlayers = 4;
    
    [SerializeField]
    private int m_minPlayers = 2;

    [SerializeField]
    protected XRInteractionManager m_interactionManager;
    public XRInteractionManager InteractionManager => m_interactionManager;

    public Game Game;

    public Round Round;

    public Turn Turn;
    
    [SerializeField] 
    private List<NetworkItem> m_availableItems;
    public NetworkItem[] AvailableItems => m_availableItems.ToArray();

    public override void OnNetworkSpawn()
    {
        net_playerInGame.OnValueChanged += OnPlayersIdsValueChanged;
        net_gameState.OnValueChanged += OnGameStateChanged;
        GameStateChanged?.Invoke(this, new GameStateChangedArgs { State = net_gameState.Value});
    }
    
    private void OnPlayersIdsValueChanged(List<ulong> _, List<ulong> newPlayerList)
    {
        if (net_gameState.Value != GameState.PREPARE)
        {
            return;
        }
        
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (net_playerInGame.Value.Count < m_minPlayers)
        {
            return;
        }

        if (net_playerInGame.Value.Count >= m_maxPlayers || net_playerInGame.Value.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGame();
        }
    }
    
    private void StartGame()
    {
        net_gameState.Value = GameState.IN_PROGRESS;
    }
    
    private void OnGameStateChanged(GameState _, GameState value)
    {
        GameStateChanged?.Invoke(this, new GameStateChangedArgs { State =  value });
        
        switch (value)
        {
            case GameState.PREPARE: 
                m_moveAction.action.Enable();
                break;
            
            case GameState.IN_PROGRESS:
                m_moveAction.action.Disable();
                break;
            
            case GameState.FINISHED:
                m_moveAction.action.Enable();
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
    
    [Rpc(SendTo.Server)]
    public void AddPlayerRpc(ulong player)
    {
        net_playerInGame.Value.Add(player);
        net_playerInGame.CheckDirtyState();
    }
    
    [Rpc(SendTo.Server)]
    public void EndGameRpc()
    {
        net_gameState.Value = GameState.FINISHED;
    }
}
