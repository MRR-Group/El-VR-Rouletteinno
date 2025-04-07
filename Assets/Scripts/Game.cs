using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;
using Random = UnityEngine.Random;

public class Game : NetworkBehaviour
{
    public event EventHandler Win;
    private NetworkVariable<Dictionary<ulong, uint>> net_wins = new(new Dictionary<ulong, uint>());
    private NetworkVariable<List<ulong>> net_alivePlayers = new(new List<ulong>());

    [SerializeField]
    private int m_minAlivePlayers = 2;

    [SerializeField]
    private bool m_disableAliveChecker = false;
    
    [SerializeField]
    private int m_maxWins;
    public int MaxWins => m_maxWins;

    public ulong[] AlivePlayers => net_alivePlayers.Value.ToArray();

    public bool AreAllItemBoxesUsed()
    {
        return PlayerManager.Instance.ByIds(net_alivePlayers.Value.ToArray())
            .All(player => !(player.Inventory?.HasUnusedItemBox ?? false));
    }

    public Player GetRandomPlayer(ulong[] excludedPlayers)
    {
        var players = net_alivePlayers.Value.FindAll((id) => !excludedPlayers.Contains(id));
        var id = players[Random.Range(0, players.Count)];
        
        return PlayerManager.Instance.ById(id);
    }
    
    
    public ulong GetNextPlayer(ulong currentPlayer)
    {
        var players = net_alivePlayers.Value;
        var index = players.IndexOf(currentPlayer);
        var next = index + 1 >= players.Count ? 0 : index + 1;
        
        return players[next];
    }

    public bool ShouldEndGame()
    {
        return net_alivePlayers.Value.Count <= m_minAlivePlayers && !m_disableAliveChecker;
    }

    public override void OnNetworkSpawn()
    {
        net_wins.OnValueChanged += OnWinsChanged;

        if (NetworkManager.Singleton.IsServer)
        {
            net_alivePlayers.Value.Clear();
            net_alivePlayers.CheckDirtyState();
            
            net_wins.Value.Clear();
            net_wins.CheckDirtyState();
            
            GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        net_wins.OnValueChanged -= OnWinsChanged;
        
        if (NetworkManager.Singleton.IsServer)
        {
            net_alivePlayers.Value.Clear();
            net_alivePlayers.CheckDirtyState();
            
            net_wins.Value.Clear();
            net_wins.CheckDirtyState();
            
            GameManager.Instance.GameStateChanged -= GameManager_OnGameStateChanged;
        }
    }

    private void OnWinsChanged(Dictionary<ulong, uint> _, Dictionary<ulong, uint> __)
    {
        Win?.Invoke(this, EventArgs.Empty);
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State == GameState.IN_PROGRESS)
        {
            Debug.Log("Should start da gaem1!!!");
            StartGameRpc();
        }
    }

    public void RemovePlayer(ulong player)
    {
        net_alivePlayers.Value.Remove(player);
        net_wins.Value.Remove(player);

        if (GameManager.Instance.Turn.IsPlayerTurn(player))
        {
            GameManager.Instance.Turn.NextTurnRpc();
        }
        else if (ShouldEndGame()) {
            PlayerWinGameRpc(GameManager.Instance.Turn.CurrentPlayerId);
            return;
        }
        
        GameManager.Instance.Turn.NextTurnRpc();
    }
    
    [Rpc(SendTo.Server)]
    private void StartGameRpc()
    {
        ResetPlayerQueue(GameManager.Instance.InGamePlayers);
        GameManager.Instance.Round.StartRoundRpc();
    }
    
    private void ResetPlayerQueue(ulong[] participants)
    {
        net_alivePlayers.Value = new List<ulong>(participants);
        net_alivePlayers.CheckDirtyState();
        
        foreach (var player in PlayerManager.Instance.ByIds(net_alivePlayers.Value.ToArray()))
        {
            player.ResetHealthRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerWinGameRpc(ulong player)
    {
        if (net_wins.Value.TryAdd(player, 1))
        {
            net_wins.Value[player] += 1;
            ShowWinnerNotificationRpc(player, "the round!");
        }

        net_wins.CheckDirtyState();
        
        InventoryManager.Instance.ByClientId(player).Chair.DisplayWinParticlesRpc();

        StartGameRpc();

        if (IsGameOver())
        {
            ShowWinnerNotificationRpc(player, "the game!");
            GameManager.Instance.EndGameRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ShowWinnerNotificationRpc(ulong playerId, string message)
    {
        var player = PlayerManager.Instance.ById(playerId);
        PlayerHudNotification.Instance.ShowText($"{player.Name} won {message}");
    }

    private bool IsGameOver()
    {
        return net_wins.Value.Any(win => win.Value >= MaxWins);
    }
    
    [Rpc(SendTo.Server)]
    public void RemoveDeadPlayerRpc(ulong player)
    {
        net_alivePlayers.Value.Remove(player);
        net_alivePlayers.CheckDirtyState();
    }

    public uint GetPlayerWins(ulong player)
    {
        return net_wins.Value.TryGetValue(player, out var value) ? value : 0;
    }
}
