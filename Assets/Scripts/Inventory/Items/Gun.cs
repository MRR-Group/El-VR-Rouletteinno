using System;
using System.Collections;
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
    private ParticleSystem m_shootLiveParticles;
    
    [SerializeField]
    private ParticleSystem m_shootBlankParticles;

    [SerializeField]
    private AudioSource m_shootAudio;

    [SerializeField]
    private AudioSource m_reloadAudio;
    
    private NetworkVariable<List<bool>> _ammo = new (new List<bool>());
    
    public event EventHandler AmmoChanged;
    public event EventHandler<BulletSkippedEventArgs> BulletSkipped;
    public class BulletSkippedEventArgs
    {
        public bool Bullet;
    }


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
        ShootRpc(player.PlayerId, NetworkManager.Singleton.LocalClientId);
        
        return true;
    }
    
    [Rpc(SendTo.Server)]
    protected void ShootRpc(ulong target, ulong shooter)
    {
        if (!CanUse(shooter))
        {
            return;
        }
        
        var isBulletLive = _ammo.Value[0];
        _ammo.Value.RemoveAt(0);
        _ammo.CheckDirtyState();

        PlayShootAudioRpc();
        
        if (isBulletLive)
        {
            EmitLiveParticlesRpc();
            PlayerManager.Instance.ById(target).DealDamageRpc(1);
        }
        else
        {
            EmitBlankParticlesRpc();
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

    [Rpc(SendTo.Everyone)]
    protected void EmitLiveParticlesRpc()
    {
        m_shootLiveParticles.time = 0;
        m_shootLiveParticles.Play();
    }
    
    [Rpc(SendTo.Everyone)]
    protected void EmitBlankParticlesRpc()
    {
        m_shootBlankParticles.time = 0;
        m_shootBlankParticles.Play();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayShootAudioRpc()
    {
        
        m_shootAudio.time = 0;
        m_shootAudio.Play();
    }

    protected override bool CanUse(ulong player)
    {
        return base.CanUse(player) && !IsMagazineEmpty();
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

        var bullet = _ammo.Value[0];
        
        _ammo.Value.RemoveAt(0);
        _ammo.CheckDirtyState();
        EmitBulletSkippedRpc(bullet);
        
        if (IsMagazineEmpty())
        {
            GameManager.Instance.Round.StartRoundRpc();
        }
    }


    [Rpc(SendTo.Everyone)]
    protected void EmitBulletSkippedRpc(bool bullet)
    {
        BulletSkipped?.Invoke(this, new BulletSkippedEventArgs { Bullet = bullet });
    }
}