using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private int m_maxHealth = 5;

    private NetworkVariable<int> net_health = new ();
    public Inventory Inventory => InventoryManager.Instance.ByClientId(PlayerId);
    
    [SerializeField]
    private ParticleSystem m_particleSystem;
    
    [SerializeField]
    private AudioSource m_vapeAudio;
    
    public event EventHandler<HealthChangedArgs> HealthChanged;
    public class HealthChangedArgs : EventArgs
    {
        public int Delta;
        public int Health;
    }
    
    public ulong PlayerId => OwnerClientId;
    
    public int Health => net_health.Value;

    public bool isLocalClient => OwnerClientId == NetworkManager.Singleton.LocalClientId;
    
    private NetworkVariable<bool> net_isHandcuffed = new ();
    public bool IsHandcuffed => net_isHandcuffed.Value;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsServer)
        {
            net_health.Value = m_maxHealth;
        }

        if (NetworkManager.Singleton.LocalClientId == PlayerId)
        {
            PlayerManager.Instance.LoadPlayers();
            InventoryManager.Instance.LoadInventories();
        }
        else
        {
            PlayerManager.Instance.RegisterPlayer(PlayerId, this);
        }
        
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
    
    [Rpc(SendTo.Everyone)]
    public void EmitVapeEffectsRpc(float animationTime)
    {
        m_vapeAudio.time = 0;
        m_vapeAudio.Play();
        
        Invoke(nameof(ShowVapeParticles), animationTime);
    }

    private void ShowVapeParticles()
    {
        m_particleSystem.time = 0;
        m_particleSystem.Play();
    }

    [Rpc(SendTo.Server)]
    public void CuffRpc()
    {
        net_isHandcuffed.Value = true;
        Inventory.Chair.ActivateCageRpc();
    }
    [Rpc(SendTo.Server)]
    public void UncuffRpc()
    {
        net_isHandcuffed.Value = false;
        Inventory.Chair.DeactivateCageRpc();
    }
}