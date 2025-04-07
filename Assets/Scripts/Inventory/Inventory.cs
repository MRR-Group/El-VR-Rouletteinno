using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Inventory : NetworkBehaviour
{
    private List<Slot> _slots;

    public event EventHandler ItemBoxUsed;

    [SerializeField] 
    private Transform m_ItemBoxSpawnPoint;

    [SerializeField] 
    private ItemBox m_itemBoxPrefab;

    [SerializeField] 
    private StartGameButton m_startButtonPrefab;
    
    [SerializeField] 
    private GameChair m_chair;
    public GameChair Chair => m_chair;

    private NetworkVariable<bool> net_hasUnusedItemBox = new(false);

    public bool HasUnusedItemBox => net_hasUnusedItemBox.Value;

    private void Awake()
    {
        _slots = GetComponentsInChildren<Slot>().ToList();
    }
    
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            net_hasUnusedItemBox.Value = false;
        }
    }

    public List<Slot> GetSlots()
    {
        return _slots;
    }

    public Slot GetSlot(int slotNumber)
    {
        return _slots[slotNumber];
    }

    public void ClearSlots()
    {
        foreach (var slot in _slots)
        {
            slot.vacateRpc();
        }
    }

    private Slot GetFreeSlot()
    {
        var freeSlots = from _slot in _slots where _slot.IsFree select _slot;
        return freeSlots.FirstOrDefault();
    }

    [Rpc(SendTo.Server)]
    public void MarkItemBoxAsUsedRpc()
    {
        net_hasUnusedItemBox.Value = false;
        EmitItemBoxWasUsedEventRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void EmitItemBoxWasUsedEventRpc()
    {
        ItemBoxUsed?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnStartGameButton(ulong clientId)
    {
        Debug.Log("Spaw start button");
        
        if (m_chair.IsFree || m_chair.Player.IsDead() || !NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Maybe not! xd");

            return;
        }

        var instance = Instantiate(NetworkManager.GetNetworkPrefabOverride(m_startButtonPrefab.gameObject), m_ItemBoxSpawnPoint.position, Quaternion.identity);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        
        instanceNetworkObject.Spawn();
        
        var button = instance.GetComponent<StartGameButton>();

        button.SetSpawnPointRpc(m_ItemBoxSpawnPoint.position);
        button.SetOwnerRpc(clientId);
        
        Debug.Log("sPAWNED!! " + button.gameObject.name);
    }
    
    public void SpawnItemBox(ulong clientId)
    {
        if (m_chair.IsFree || m_chair.Player.IsDead())
        {
            return;
        }

        var instance = Instantiate(NetworkManager.GetNetworkPrefabOverride(m_itemBoxPrefab.gameObject),
            m_ItemBoxSpawnPoint.position, Quaternion.identity);
        
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        
        var box = instance.GetComponent<ItemBox>();

        box.SetSpawnPointRpc(m_ItemBoxSpawnPoint.position);
        box.SetOwnerRpc(clientId);
        
        net_hasUnusedItemBox.Value = true;
    }

    private void AddItem(int prefabIndex, ulong clientId)
    {
        var slot = GetFreeSlot();

        if (!slot)
        {
            return;
        }

        var itemPrefab = GameManager.Instance.AvailableItems[prefabIndex];
        var instance = Instantiate(
            NetworkManager.GetNetworkPrefabOverride(itemPrefab.gameObject),
            slot.SpawnPoint.position + new Vector3(0, 0.1f, 0), Quaternion.identity
        );
        
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn();
        
        var item = instance.GetComponent<NetworkItem>();

        slot.OccupyRpc();
        item.SetSpawnPointRpc(slot.SpawnPoint.position);
        item.SetOwnerRpc(clientId);
        item.SetInventorySlotIdRpc(_slots.IndexOf(slot));
    }

    [Rpc(SendTo.Server)]
    public void SpawnRandomItemsRpc(ulong clientId)
    {
        var itemsCount = GameManager.Instance.Round.CurrentItemCount;
        var itemsList = GetRandomItemIds(itemsCount);

        foreach (var item in itemsList)
        {
            AddItem(item, clientId);
        }
    }

    private int GetRandomItemIndex()
    {
        var itemsByDropChance = GameManager.Instance.ItemsDropChance;
        var dropChance = ItemGroupMethods.FromInt(Random.Range(0, 100));

        var items = itemsByDropChance[dropChance];        
        var randomItem = Random.Range(0, items.Length);
        
        return Array.IndexOf(GameManager.Instance.AvailableItems, items[randomItem]);
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

    [Rpc(SendTo.Everyone)]
    public void RegisterRpc(ulong player)
    {
        InventoryManager.Instance.RegisterInventory(player, this);
    }
}