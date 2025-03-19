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
        public int Delta;
        public int Health;
    }
    
    public ulong PlayerId => OwnerClientId;
    
    public int Health => health.Value;

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