using UnityEngine;
using Unity.Netcode;

public class Vape : NetworkItem
{
    private const int HEAL_AMOUNT = 1;
    public override void Use(ulong target)
    {
        if (!CanUse()) return;

        HealPlayer(target);
        DestroyItem();
    }

    public void Use()
    {
        Use(NetworkManager.Singleton.LocalClientId);
    }

    private void HealPlayer(ulong target)
    {
        PlayerManager.Instance.Player[target].AddHealthRpc(HEAL_AMOUNT);
    }
}