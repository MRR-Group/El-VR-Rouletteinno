using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    public SerializedDictionary<ulong, Inventory> _inventories = new ();
  
    public Inventory ByClientId(ulong client)
    {
        return _inventories[client];
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
}