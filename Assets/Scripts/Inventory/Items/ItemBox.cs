using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkItem
{
    private NetworkVariable<ulong> net_player = new ();

    public Inventory Inventory => InventoryManager.Instance.ByClientId(net_player.Value);
    
    public void SetPlayer(ulong player)
    {
        net_player.Value = player;
    }

    public override bool Use()
    {
        Inventory.SpawnRandomItemsRpc();
        Inventory.MarkItemBoxAsUsedRpc();
        
        return true;
    }

    protected override bool CanUse()
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS;
    }
}