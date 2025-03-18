using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<int> health = new (5);

    public event EventHandler<HealthChangedArgs> HealthChanged;
    public class HealthChangedArgs : EventArgs
    {
        public int Health;
    }
    
    public ulong PlayerId => OwnerClientId;
    
    public int Health => health.Value;
    
    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.Player.Add(PlayerId, this);
        health.OnValueChanged += (_, value) => HealthChanged?.Invoke(this, new HealthChangedArgs { Health = value });
    }

    public bool IsDead()
    {
        return Health <= 0;
    }

    [Rpc(SendTo.Server)]
    public void ResetHealthRpc()
    {
        health.Value = 5;
    }

    [Rpc(SendTo.Server)]
    public void DealDamageRpc(int damage)
    {
        health.Value -= damage;
        
        if (IsDead())
        {
            GameManager.Instance.game.RemoveDeadPlayerRpc(PlayerId);
        }
    }
}