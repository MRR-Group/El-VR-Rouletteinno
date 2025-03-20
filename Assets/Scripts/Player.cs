using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int m_MaxHealth = 5;
    private NetworkVariable<int> health = new ();

    public event EventHandler<HealthChangedArgs> HealthChanged;
    public class HealthChangedArgs : EventArgs
    {
        public int Delta;
        public int Health;
    }
    
    public ulong PlayerId => OwnerClientId;
    
    public int Health => health.Value;

    public void Awake()
    {
        health.Value = m_MaxHealth;
    }

    public bool IsCurrentPlayer()
    {
        return PlayerId == NetworkManager.Singleton.LocalClientId;
    }

    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.Player.Add(PlayerId, this);
        health.OnValueChanged += (oldValue, value) => HealthChanged?.Invoke(this, new HealthChangedArgs { Health = value, Delta = value - oldValue});
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    [Rpc(SendTo.Server)]
    public void ResetHealthRpc()
    {
        health.Value = m_MaxHealth;
    }

    [Rpc(SendTo.Server)]
    public void DealDamageRpc(int damage)
    {
        health.Value = Math.Clamp(health.Value - damage, 0, m_MaxHealth);
        
        if (IsDead())
        {
            GameManager.Instance.game.RemoveDeadPlayerRpc(PlayerId);
        }
    }

    [Rpc(SendTo.Server)]
    public void AddHealthRpc(int amount)
    {
        health.Value = Math.Clamp(health.Value + amount, 0, m_MaxHealth);
    }

    public int GetMaxHealth()
    {
        return m_MaxHealth;
    }
}