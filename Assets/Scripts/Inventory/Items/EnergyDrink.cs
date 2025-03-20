using UnityEngine;
using Unity.Netcode;

public class EnergyDrink : NetworkItem
{
    
    public void Use()
    {
        Use(NetworkManager.Singleton.LocalClientId);
    }
    
    public override void Use(ulong target)
    {
        
        if (!CanUse())
        {
            return;
        }
        GameManager.Instance.Round.Gun.RemoveCurrentBullet();
        
        Debug.Log("Use EnergyDrink");
        
        DestroyItemRpc(); 
    }
}