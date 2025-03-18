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

    private NetworkVariable<List<bool>> ammo = new (new List<bool>());

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

    public override void Use(ulong target)
    {
        if (CanUse())
        {
            ShootRpc(target);
        }
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