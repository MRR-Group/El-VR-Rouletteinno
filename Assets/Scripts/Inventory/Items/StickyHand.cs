using UnityEngine;
using Unity.Netcode;

public class StickyHand : TargetableItem<NetworkItem>
{

    public override bool Use(NetworkItem item)
    {
        item.Use();
        
        return true;
    }
}