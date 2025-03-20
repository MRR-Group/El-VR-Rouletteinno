using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : NetworkItem
{
    [SerializeField]
    private uint m_maxAmmo = 6;

    [SerializeField]
    private uint m_minAmmo = 2;

    [SerializeField]
    private Transform m_raycastStart;
    
    [SerializeField]
    private ParticleSystem m_shootParticles;

    [SerializeField] 
    private LayerMask m_layermask;
    
    private NetworkVariable<List<bool>> _ammo = new (new List<bool>());
    
    public event EventHandler AmmoChanged;
    
    public override void OnNetworkSpawn()
    {
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

    public void PullTrigger()
    {
        if (!CanUse())
        {
            return;
        }
        
        var target = StartRayCast();
        
        if (target != null)
        {
            ShootRpc(target.PlayerId);
            ForceDrop();
        }
    }

    public override void Use(ulong target)
    {
        if (CanUse())
        {
            ShootRpc(target);
        }
    }

    private Player StartRayCast()
    {
        RaycastHit hit;
        var success= Physics.Raycast(m_raycastStart.position, transform.TransformDirection(Vector3.right), out hit, Mathf.Infinity, m_layermask);

        return success ? hit.transform?.GetComponentInParent<Player>() : null;
    }

    [Rpc(SendTo.Server)]
    private void ShootRpc(ulong target)
    {
        if (IsMagazineEmpty())
        {
            return;
        }
        
        var isBulletLive = _ammo.Value[0];
        _ammo.Value.RemoveAt(0);
        _ammo.CheckDirtyState();
        
        if (isBulletLive)
        {
            PlayerManager.Instance.ById(target).DealDamageRpc(1);
            m_shootParticles.time = 0;
            m_shootParticles.Play();
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

    private bool IsMagazineEmpty()
    {
        return _ammo.Value.Count == 0;
    }

    public bool[] Magazine()
    {
        return _ammo.Value.ToArray();
    }
}