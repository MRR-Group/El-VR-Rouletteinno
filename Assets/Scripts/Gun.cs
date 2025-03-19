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
    
    private NetworkVariable<List<bool>> ammo = new (new List<bool>());

    [SerializeField]
    private GameObject m_bullet;
    
    public event EventHandler AmmoChanged;
    
    public override void OnNetworkSpawn()
    {
        ammo.OnValueChanged += (_, __) => AmmoChanged?.Invoke(null, null);
    }
    
    [Rpc(SendTo.Server)]
    public void ChangeMagazineRpc()
    {
        ammo.Value.Clear();

        for (var i = 0; i < Random.Range(m_minAmmo, m_maxAmmo); i++)
        {
            ammo.Value.Add(Random.Range(0, 2) == 1);
        }

        ammo.CheckDirtyState();
    }

    public void PullTrigger()
    {
        Debug.Log("Clicked!");
        
        // if (!CanUse())
        // {
        //     return;
        // }

        var target = StartRayCast();
        Debug.Log("Target: " + target + " player id " + target?.PlayerId);

        if (target != null)
        {
            ShootRpc(target.PlayerId);
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
        var success= Physics.Raycast(m_raycastStart.position, transform.TransformDirection(Vector3.right), out hit, Mathf.Infinity);
        Debug.Log("RayCast success: " + success + " target " + hit.transform?.gameObject.name);

        return success ? hit.transform?.GetComponent<Player>() : null;
    }

    [Rpc(SendTo.Server)]
    private void ShootRpc(ulong target)
    {
        if (IsMagazineEmpty())
        {
            return;
        }
        
        var isBulletLive = ammo.Value[0];
        ammo.Value.RemoveAt(0);
        ammo.CheckDirtyState();
        
        if (isBulletLive)
        {
            PlayerManager.Instance.Player[target].DealDamageRpc(1);
            m_shootParticles.time = 0;
            m_shootParticles.Play();
        }

        if (isBulletLive || !GameManager.Instance.turn.IsPlayerTurn(target))
        {
            GameManager.Instance.turn.NextTurnRpc();
        }
        
        if (IsMagazineEmpty())
        {
            GameManager.Instance.round.StartRoundRpc();
        }
    }

    private bool IsMagazineEmpty()
    {
        return ammo.Value.Count == 0;
    }

    public bool[] Magazine()
    {
        return ammo.Value.ToArray();
    }
}