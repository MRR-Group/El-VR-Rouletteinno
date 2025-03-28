using UnityEngine;
using Unity.Netcode;

public class EnergyDrink : NetworkItem
{
    [SerializeField]
    private AudioSource m_drinkAudio;
    
    protected override bool CanUse(ulong player)
    {
        return base.CanUse(player) && !GameManager.Instance.Round.Gun.IsMagazineEmpty();
    }

    public override bool Use()
    {
        GameManager.Instance.Round.Gun.SkipCurrentBulletRpc();

        PlayDrinkAudioRpc();

        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDrinkAudioRpc()
    {
        m_drinkAudio.time = 0;
        m_drinkAudio.Play();
    }
}