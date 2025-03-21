using UnityEngine;

public class Vape : NetworkItem
{
    [SerializeField]
    private int m_healAmount = 1;
    
    [SerializeField]
    private Material m_ledOnMaterial;
    
    [SerializeField]
    private Material m_ledOffMaterial;
    
    [SerializeField]
    private Renderer[] m_usageLed;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        net_usages.OnValueChanged += UpdateLeds;
    }
    private void UpdateLeds(int _, int usages)
    {
        for (var i = 0; i < m_usages - usages; i++)
        {
            m_usageLed[i].material = m_ledOnMaterial;
        }
        
        for (var i = usages; i < m_usages; i++)
        {
            m_usageLed[i].material = m_ledOffMaterial;
        }
    }

    public override bool Use()
    {
        var player = PlayerManager.Instance.Client();
        
        if (!player.CanBeHealed(m_healAmount))
        {
            return false;
        }
        
        player.HealRpc(m_healAmount);

        return true;
    }
}