using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Inventory : NetworkBehaviour
{
    private List<Slot> _slots;
    

    private void Awake()
    {
        _slots = GetComponentsInChildren<Slot>().ToList();
    }

    public List<Slot> GetSlots()
    {
        return _slots;
    }

    public Slot GetSlot(int  slotNumber)
    {
        return _slots[slotNumber];
    }

    private Slot GetFreeSlot()
    {
        var freeSlots = from _slot in _slots where _slot.IsFree select _slot ;
        return freeSlots.FirstOrDefault();
    }
    
    [Rpc(SendTo.Server)]
    private void AddItemRpc(int prefabIndex)
    {
        var slot = GetFreeSlot();
        
        if (!slot)
        {
            return;
        }
        
        var itemPrefab = GameManager.Instance.AvailableItems[prefabIndex];
        
        var instance = Instantiate(NetworkManager.GetNetworkPrefabOverride(itemPrefab.gameObject), slot.SpawnPoint.position + new Vector3(0, 0.1f, 0),  Quaternion.identity);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        var item = instance.GetComponent<NetworkItem>();
        
        slot.OccupyRpc();
        item.SetSpawnPointRpc(slot.SpawnPoint.position);
    }

    [Rpc(SendTo.Server)]
    public void SpawnRandomItemsRpc()
    {
        var itemsCount = GameManager.Instance.Round.CurrentItemCount;
        var itemsList = GetRandomItemIds(itemsCount);
        
        foreach (var item in itemsList)
        {
            AddItemRpc(item);
        }
    }

    private int GetRandomItemIndex()
    {
        var items = GameManager.Instance.AvailableItems;
        var randomIndex = Random.Range(0, items.Length);
        
        return randomIndex;
    }

    private int[] GetRandomItemIds(int count)
    {
        var randomItems = new List<int>();
        
        for (var i = 0; i < count; i++)
        {
            randomItems.Add(GetRandomItemIndex());
        }
        
        return randomItems.ToArray();
    }
}
