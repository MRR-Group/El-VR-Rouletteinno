using UnityEngine;
using Unity.Netcode;

public class Vape : NetworkItem
{
    private const int HEAL_AMOUNT = 1;
    
    public override void Use(ulong target)
    {
        Debug.Log("MaxHealth " + PlayerManager.Instance.Player[target].GetMaxHealth());
        Debug.Log("health " + PlayerManager.Instance.Player[target].Health);
        if (!CanUse()) return;
        if (!CanHeal(target)) return;

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

    private bool CanHeal(ulong target)
    {
        int healthAfterHeal = PlayerManager.Instance.Player[target].Health + HEAL_AMOUNT;
        int maxHealth = PlayerManager.Instance.Player[target].GetMaxHealth();
        
        return healthAfterHeal <= maxHealth;
    }
}