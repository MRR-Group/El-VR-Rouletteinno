using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
{
    public event EventHandler Win;
    private NetworkVariable<Dictionary<ulong, uint>> wins = new(new Dictionary<ulong, uint>());
    public NetworkVariable<List<ulong>> players = new(new List<ulong>());

    public override void OnNetworkSpawn()
    {
        wins.OnValueChanged += OnWinsChanged;
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
    }

    private void OnWinsChanged(Dictionary<ulong, uint> _, Dictionary<ulong, uint> __)
    {
        Win?.Invoke(this, EventArgs.Empty);
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State == GameState.IN_PROGRESS)
        {
            StartGameRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void StartGameRpc()
    {
        ResetPlayerQueue();
        
        GameManager.Instance.round.StartRoundRpc();
    }
    
    private void ResetPlayerQueue()
    {
        players.Value = new List<ulong>(GameManager.Instance.playersIds.Value);
        players.CheckDirtyState();
        
        foreach (var player in GameManager.Instance.players)
        {
            player.ResetHealthRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerWinGameRpc(ulong player)
    {
        if (!wins.Value.TryAdd(player, 1))
        {
            wins.Value[player] += 1;
        }

        wins.CheckDirtyState();

        StartGameRpc();

        if (IsGameOver())
        {
            GameManager.Instance.EndGameRpc();
        }
    }
    
    private bool IsGameOver()
    {
        return wins.Value.Any(win => win.Value >= 2);
    }
    
    [Rpc(SendTo.Server)]
    public void RemoveDeadPlayerRpc(ulong player)
    {
        players.Value.Remove(player);
        players.CheckDirtyState();
    }

    public uint GetPlayerWins(ulong player)
    {
        return wins.Value.TryGetValue(player, out var value) ? value : 0;
    }
}
