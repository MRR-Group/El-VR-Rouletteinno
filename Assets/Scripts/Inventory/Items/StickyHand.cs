using UnityEngine;
using Unity.Netcode;

public class StickyHand : TargetableItem<NetworkItem>
{

    public override bool Use(NetworkItem item)
    {
        if (item.OwnerId == NetworkManager.Singleton.LocalClientId)
        {
            return false;
        }
        
        item.StealItem();
        return true;
    }
}