using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    private Dictionary<ulong, Player> player = new (new Dictionary<ulong, Player>());
    
    public Dictionary<ulong, Player> Player { get => player; set => player = value; }

    public Player Client()
    {
        return player[NetworkManager.Singleton.LocalClientId];
    }
}