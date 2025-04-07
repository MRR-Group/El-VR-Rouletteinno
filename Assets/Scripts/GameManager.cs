using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using XRMultiplayer;

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
    
    [SerializedDictionary]
    [SerializeField]
    private SerializedDictionary<ItemGroup, NetworkItem[]> m_itemsDropChance = new ();
    
    private NetworkItem[] _availableItems;
    
    public Dictionary<ItemGroup, NetworkItem[]> ItemsDropChance => m_itemsDropChance;
    public NetworkItem[] AvailableItems => _availableItems;

    void Start()
    {
        _availableItems = m_itemsDropChance.Values.SelectMany(value => value).ToArray();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            net_gameState.Value = GameState.PREPARE;
            
            net_playerInGame.Value.Clear();
            net_playerInGame.CheckDirtyState();
            XRINetworkGameManager.Instance.playerStateChanged += XRINetworkGameManager_OnPlayerConnectionChanged;
        }

        net_gameState.OnValueChanged += OnGameStateChanged;
        GameStateChanged?.Invoke(this, new GameStateChangedArgs { State = net_gameState.Value});
    }

    public override void OnNetworkDespawn()
    {
        net_gameState.OnValueChanged -= OnGameStateChanged;
        
        if (NetworkManager.Singleton.IsServer)
        {
            XRINetworkGameManager.Instance.playerStateChanged -= XRINetworkGameManager_OnPlayerConnectionChanged;
        }
    }
    
    private void XRINetworkGameManager_OnPlayerConnectionChanged(ulong playerId, bool isConnected)
    {
        if (!isConnected && NetworkManager.Singleton.IsServer)
        {
            net_playerInGame.Value.Remove(playerId);
            net_playerInGame.CheckDirtyState();
            
            Game.RemovePlayer(playerId);
        }
    }
    
    private void OnGameStateChanged(GameState _, GameState value)
    {
        GameStateChanged?.Invoke(this, new GameStateChangedArgs { State =  value });
        Debug.Log("GameStateChanged: " + value.ToString());
        
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
                StartCoroutine(nameof(ForceDisconnect));
                
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    private IEnumerator ForceDisconnect()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            yield return new WaitForSeconds(1.0f);
            
            foreach (var player in PlayerManager.Instance.ByIds(new ulong[] { NetworkManager.Singleton.LocalClientId }))
            {
                NetworkManager.Singleton.DisconnectClient(player.PlayerId);
            }
            
            yield return new WaitForSeconds(1.0f);
            
            XRINetworkGameManager.Instance.Disconnect();

            yield return null;
        }
    }

    [Rpc(SendTo.Server)]
    public void AddPlayerRpc(ulong player)
    {
        net_playerInGame.Value.Add(player);
        net_playerInGame.CheckDirtyState();
        
        Debug.Log("net_playerInGame: PLAYER: " + player + " added!!!");
    }

    public void TryStartGame()
    {
        if (net_playerInGame.Value.Count >= m_minPlayers)
        {
            net_gameState.Value = GameState.IN_PROGRESS;
        }
    }
    
    [Rpc(SendTo.Server)]
    public void EndGameRpc()
    {
        net_gameState.Value = GameState.FINISHED;
    }
}
