using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] [SerializedDictionary("Client Id", "Player")]
    private SerializedDictionary<ulong, Player> _players = new();

    protected void Start()
    {
        XRINetworkGameManager.Connected.Subscribe(HandleDisconnect);
        XRINetworkGameManager.Instance.playerStateChanged += XRINetworkGameManager_OnPlayerConnectionChanged;
    }

    private void XRINetworkGameManager_OnPlayerConnectionChanged(ulong playerId, bool isConnected)
    {
        if (isConnected)
        {
            return;
        }

        if (_players.ContainsKey(playerId))
        {
            _players.Remove(playerId);
        }
    }

    protected void HandleDisconnect(bool status)
    {
        if (!status)
        {
            _players = new ();
        }
    }
    
    public Player Client()
    {
        return _players[NetworkManager.Singleton.LocalClientId];
    }

    public void LoadPlayers()
    {
        var players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var player in players)
        {
            RegisterPlayer(player.PlayerId, player);
        }
    }

    public void RegisterPlayer(ulong id, Player instance)
    {
        if (!_players.ContainsKey(id))
        {
            _players.Add(id, instance);
        }
    }

    public Player[] ByIds(ulong[] ids)
    {
        var query = from player in _players
            where ids.Contains(player.Key)
            select player.Value;

        return query.ToArray();
    }

    public Player ById(ulong id)
    {
        return IsPlayerConnected(id) ? _players[id] : null;
    }

    public bool IsPlayerConnected(ulong id)
    {
        return _players.ContainsKey(id);
    }
}