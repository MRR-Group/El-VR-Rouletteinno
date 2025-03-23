using UnityEngine;
using Unity.Netcode;

public class EnergyDrink : NetworkItem
{
    protected override bool CanUse(ulong player)
    {
        return base.CanUse(player) && !GameManager.Instance.Round.Gun.IsMagazineEmpty();
    }

    public override bool Use()
    {
        GameManager.Instance.Round.Gun.SkipCurrentBulletRpc();

        return true;
    }
}