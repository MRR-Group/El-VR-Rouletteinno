using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField]
    private Dictionary<ulong, Player> _players = new (new Dictionary<ulong, Player>());
    
    public Player Client()
    {
        return _players[NetworkManager.Singleton.LocalClientId];
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
        return _players[id];
    }

    public void RegisterPlayer(ulong id, Player instance)
    {
        _players.Add(id, instance);
    }
}