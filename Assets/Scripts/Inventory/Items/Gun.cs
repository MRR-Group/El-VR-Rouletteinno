using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : TargetableItem<Player>
{
    [SerializeField]
    private uint m_maxAmmo = 6;

    [SerializeField]
    private uint m_minAmmo = 2;
    
    [SerializeField]
    private ParticleSystem m_shootParticles;
    
    private NetworkVariable<List<bool>> _ammo = new (new List<bool>());
    
    public event EventHandler AmmoChanged;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _ammo.OnValueChanged += (_, __) => AmmoChanged?.Invoke(null, null);
    }
    
    [Rpc(SendTo.Server)]
    public void ChangeMagazineRpc()
    {
        _ammo.Value.Clear();

        for (var i = 0; i < Random.Range(m_minAmmo, m_maxAmmo); i++)
        {
            _ammo.Value.Add(Random.Range(0, 2) == 1);
        }

        _ammo.CheckDirtyState();
    }
    
    public override bool Use(Player player)
    {
        ShootRpc(player.PlayerId);

        return true;
    }
    
    [Rpc(SendTo.Server)]
    protected void ShootRpc(ulong target)
    {
        if (!CanUse())
        {
            return;
        }
        
        var isBulletLive = _ammo.Value[0];
        _ammo.Value.RemoveAt(0);
        _ammo.CheckDirtyState();
        
        if (isBulletLive)
        {
            PlayerManager.Instance.ById(target).DealDamageRpc(1);
            EmitParticlesRpc();
        }

        if (isBulletLive || !GameManager.Instance.Turn.IsPlayerTurn(target))
        {
            GameManager.Instance.Turn.NextTurnRpc();
        }
        
        if (IsMagazineEmpty())
        {
            GameManager.Instance.Round.StartRoundRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    protected void EmitParticlesRpc()
    {
        m_shootParticles.time = 0;
        m_shootParticles.Play();
    }

    protected override bool CanUse()
    {
        return base.CanUse() && !IsMagazineEmpty();
    }

    public bool IsMagazineEmpty()
    {
        return _ammo.Value.Count == 0;
    }

    public bool[] Magazine()
    {
        return _ammo.Value.ToArray();
    }

    [Rpc(SendTo.Server)]
    public void SkipCurrentBulletRpc()
    {
        if (IsMagazineEmpty())
        {
            return;
        }

        _ammo.Value.RemoveAt(0);
        _ammo.CheckDirtyState();
        
        if (IsMagazineEmpty())
        {
            GameManager.Instance.Round.StartRoundRpc();
        }
    }
}