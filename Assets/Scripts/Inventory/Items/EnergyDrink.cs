using UnityEngine;
using Unity.Netcode;

public class EnergyDrink : NetworkItem
{
    protected override bool CanUse()
    {
        return base.CanUse() && !GameManager.Instance.Round.Gun.IsMagazineEmpty();
    }

    public override bool Use()
    {
        GameManager.Instance.Round.Gun.SkipCurrentBulletRpc();

        return true;
    }
}