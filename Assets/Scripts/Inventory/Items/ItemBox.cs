using UnityEngine;
using Unity.Netcode;

public class ItemBox : NetworkItem
{
    
    public override void Use(ulong target)
    {
        if (!CanUse()) return;
        DestroyItem();
    }

    public void Use()
    {
        Use(NetworkManager.Singleton.LocalClientId);
    }
}