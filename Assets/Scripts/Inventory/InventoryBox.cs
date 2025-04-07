using System;
using UnityEngine;

public class InventoryBox : MonoBehaviour
{
    
    [SerializeField]
    private Inventory m_inventory;

    public Player Player => m_inventory.Chair.Player;
    
    private void OnTriggerExit(Collider itemCollider)
    {
        var isFirstCollider = itemCollider.transform.parent?.GetComponentsInChildren<Collider>()[0] == itemCollider;
        var item = itemCollider.GetComponentInParent<NetworkItem>();
        
        if (!isFirstCollider || !item)
        {
            return;
        }
        
        item.ExitInventoryBox(this);
    }
    
    private void OnTriggerEnter(Collider itemCollider)
    {
        var isFirstCollider = itemCollider.transform.parent?.GetComponentsInChildren<Collider>()[0] == itemCollider;
        var item = itemCollider.GetComponentInParent<NetworkItem>();

        if (isFirstCollider && item)
        {
            item.EnterInventoryBox(this);
        }
    }
}

