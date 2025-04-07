using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using XRMultiplayer;

public class InventoryManager : Singleton<InventoryManager>
{
    public SerializedDictionary<ulong, Inventory> _inventories = new ();

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

        if (_inventories.ContainsKey(playerId))
        {
            _inventories[playerId].ClearSlots();
            
            _inventories.Remove(playerId);
        }
    }

    protected void HandleDisconnect(bool status)
    {
        if (!status)
        {
            _inventories = new ();
        }
    }

    public Inventory ByClientId(ulong client)
    {
        return HasPlayerUi(client) ? _inventories[client] : null;
    }

    public ulong GetPlayerId(Inventory inventory)
    {
        var query = from pair in _inventories 
            where pair.Value == inventory 
            select pair.Key;

        return query.FirstOrDefault();
    }

    public void LoadInventories()
    {
        var inventories = FindObjectsByType<Inventory>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (var inventory in inventories)
        {
            if (!inventory.Chair.IsFree)
            {
                RegisterInventory(inventory.Chair.Player.PlayerId, inventory);
            }
        }
    }

    public void RegisterInventory(ulong client, Inventory instance)
    {
        if (!_inventories.ContainsValue(instance))
        {
            _inventories.Add(client, instance);
        }
    }
    
    public bool HasPlayerUi(ulong playerId)
    {
        return _inventories.ContainsKey(playerId);
    }
}