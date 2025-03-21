using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkItem
{
    [SerializeField]
    private Inventory m_inventory;
    
    public override bool Use()
    {
        m_inventory.SpawnRandomItems();

        return true;
    }
}