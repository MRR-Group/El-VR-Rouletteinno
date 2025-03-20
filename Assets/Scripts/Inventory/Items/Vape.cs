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
        var player = PlayerManager.Instance.ById(target);
        
        if (!CanUse() || !player.CanBeHealed(m_healAmout))
        {
            return;
        }
        
        HealPlayer(target);
        DestroyItemRpc(); 
    }
    
    private void HealPlayer(ulong target)
    {
        PlayerManager.Instance.ById(target).HealRpc(m_healAmout);
    }
}