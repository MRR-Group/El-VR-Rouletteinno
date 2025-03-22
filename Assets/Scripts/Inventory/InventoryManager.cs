using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializedDictionary("Client Id", "Inventory")]
    public SerializedDictionary<ulong, Inventory> _inventories = new ();
  
    public Inventory ByClientId(ulong client)
    {
        Debug.Log("Inventory for " + client);
        Debug.Log("all: ");
        foreach (var key in _inventories.Keys)
        {
            Debug.Log(" KEY: " + key);
        }

        return _inventories[client];
    }

    public ulong GetPlayerId(Inventory inventory)
    {
        var query = from pair in _inventories 
            where pair.Value == inventory 
            select pair.Key;

        return query.FirstOrDefault();
    }

    public void RegisterInventory(ulong client, Inventory instance)
    {
        if (_inventories.ContainsValue(instance))
        {
            Debug.LogError("Inventory already registered. Player: " + client + ", Inventory: " + instance.NetworkObjectId);
            return;
        }

        _inventories.Add(client, instance);
    }
}