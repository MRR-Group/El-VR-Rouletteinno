using UnityEngine;
using Unity.Netcode;

public class MysteriousButton : NetworkItem
{
    [SerializeField]
    private AudioSource m_buttonAudio;
    

    public override bool Use()
    {
        GameManager.Instance.Round.Gun.SwitchCurrentBulletRpc();

        PlayButtonAudioRpc();

        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayButtonAudioRpc()
    {
        m_buttonAudio.time = 0;
        m_buttonAudio.Play();
    }
}