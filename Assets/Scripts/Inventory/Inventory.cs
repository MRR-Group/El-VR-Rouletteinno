using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Inventory : MonoBehaviour
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
        var freeSlots = from _slot in _slots where _slot.Item == null select _slot ;
        return freeSlots.FirstOrDefault();
    }
    
    public void AddItem(NetworkItem itemPrefab)
    {
        var slot = GetFreeSlot();
        
        if (!slot)
        {
            return;
        }
        
        var instance = Instantiate(itemPrefab, slot.SpawnPoint.position + new Vector3(0, 0.1f, 0),  Quaternion.identity);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        var item = instance.GetComponent<NetworkItem>();
        
        slot.Item = item;
        item.SetSpawnPoint(slot.SpawnPoint);
    }

    [Rpc(SendTo.Server)]
    private void spawnItem()
    {
        
    }

    public void SpawnRandomItems()
    {
        var itemsCount = GameManager.Instance.Round.CurrentItemCount;
        var itemsList = GetRandomItems(itemsCount);
        
        foreach (var item in itemsList)
        {
            AddItem(item);
        }
    }

    private NetworkItem GetRandomItem()
    {
        var items = GameManager.Instance.AvailableItems;
        var randomIndex = Random.Range(0, items.Length);
        
        return items[randomIndex];
    }

    private NetworkItem[] GetRandomItems(int count)
    {
        var randomItems = new List<NetworkItem>();
        
        for (var i = 0; i < count; i++)
        {
            randomItems.Add(GetRandomItem());
        }
        
        return randomItems.ToArray();
    }
}
