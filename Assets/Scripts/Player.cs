using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int m_maxHealth = 5;

    private NetworkVariable<int> net_health = new ();

    public NetworkVariable<int> net_inventory = new ();
    
    public event EventHandler<HealthChangedArgs> HealthChanged;
    public class HealthChangedArgs : EventArgs
    {
        public int Delta;
        public int Health;
    }
    
    public ulong PlayerId => OwnerClientId;
    
    public int Health => net_health.Value;

    public Inventory Inventory => GameManager.Instance.GetInventory(net_inventory.Value);

    [Rpc(SendTo.Server)]
    public void SetInventoryRpc(int inventory)
    {
        net_inventory.Value = inventory;
    }

    public bool IsCurrentPlayer()
    {
        return PlayerId == NetworkManager.Singleton.LocalClientId;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsServer)
        {
            net_health.Value = m_maxHealth;
        }

        PlayerManager.Instance.RegisterPlayer(PlayerId, this);
        net_health.OnValueChanged += (oldValue, value) => HealthChanged?.Invoke(this, new HealthChangedArgs { Health = value, Delta = value - oldValue});
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    [Rpc(SendTo.Server)]
    public void ResetHealthRpc()
    {
        net_health.Value = m_maxHealth;
    }

    [Rpc(SendTo.Server)]
    public void DealDamageRpc(int damage)
    {
        net_health.Value = Math.Clamp(net_health.Value - damage, 0, m_maxHealth);
        
        if (IsDead())
        {
            GameManager.Instance.Game.RemoveDeadPlayerRpc(PlayerId);
        }
    }

    [Rpc(SendTo.Server)]
    public void HealRpc(int amount)
    {
        net_health.Value = Math.Clamp(net_health.Value + amount, 0, m_maxHealth);
    }

    public int GetMaxHealth()
    {
        return m_maxHealth;
    }
    
    public bool CanBeHealed(int healAmount)
    {
        return Health + healAmount <= GetMaxHealth();
    }
}