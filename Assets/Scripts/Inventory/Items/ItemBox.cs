using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkItem
{
    [SerializeField]
    private Inventory m_inventory;
    
    public override void Use(ulong target)
    {
        if (!CanUse())
        {
            return;
        }

        m_inventory.SpawnRandomItems();
        
        DestroyItemRpc();
    }

    public void Use()
    {
        Use(NetworkManager.Singleton.LocalClientId);
    }
}