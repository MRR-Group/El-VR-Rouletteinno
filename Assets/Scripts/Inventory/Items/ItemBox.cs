using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkItem
{
    private NetworkVariable<int> net_inventory = new ();

    public Inventory Inventory => GameManager.Instance.GetInventory(net_inventory.Value);
    
    [Rpc(SendTo.Server)]
    public void SetInventoryRpc(int inventory)
    {
        net_inventory.Value = inventory;
    }

    public override bool Use()
    {
        Inventory.SpawnRandomItemsRpc();

        return true;
    }
}