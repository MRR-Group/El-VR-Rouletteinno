using UnityEngine;
using Unity.Netcode;

public class Vape : NetworkItem
{
    [SerializeField]
    private int m_healAmout = 1;
    
    public void Use()
    {
        Use(NetworkManager.Singleton.LocalClientId);
    }
    
    public override void Use(ulong target)
    {
        Debug.Log("MaxHealth " + PlayerManager.Instance.Player[target].GetMaxHealth());
        Debug.Log("health " + PlayerManager.Instance.Player[target].Health);
        
        if (!CanUse() || !CanHeal(target))
        {
            return;
        }
        
        HealPlayer(target);
        DestroyItemRpc();
    }
    
    private void HealPlayer(ulong target)
    {
        PlayerManager.Instance.Player[target].AddHealthRpc(m_healAmout);
    }
    
    private bool CanHeal(ulong target)
    {
        var player = PlayerManager.Instance.Player[target];
        return player.Health + m_healAmout <= player.GetMaxHealth();
    }
}