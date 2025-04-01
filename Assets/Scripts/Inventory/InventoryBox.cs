using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryBox : MonoBehaviour
{
    private Collider _collider;
    
    [SerializeField]
    private Inventory m_inventory;

    public Player Player => m_inventory.Chair.Player;
    
    void Start()
    {
        _collider = GetComponent<Collider>();
    }
    
    private void OnTriggerExit(Collider itemCollider)
    {
        var isFirstCollider = itemCollider.transform.parent?.GetComponentsInChildren<Collider>()[0] == itemCollider;
        var item = itemCollider.GetComponentInParent<NetworkItem>();

        Debug.Log($"is first collider: {isFirstCollider}: item {item}, Object: {itemCollider.transform.parent?.name}");
        
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
        Debug.Log($"is first collider: {isFirstCollider}: item {item}, Object: {itemCollider.transform.parent?.name}");

        if (isFirstCollider && item)
        {
            item.EnterInventoryBox(this);
        }
    }
}

