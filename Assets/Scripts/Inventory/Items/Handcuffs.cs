using UnityEngine;
using Unity.Netcode;

public class Handcuffs : TargetableItem<Player>
{
    [SerializeField]
    private AudioSource m_handcuffsAudio;


    public override bool Use(Player target)
    {
        if (target.PlayerId == NetworkManager.Singleton.LocalClientId)
        {
            return false;
        }
        
        
        PlayHandcuffsAudioRpc();
        return true;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayHandcuffsAudioRpc()
    {
        m_handcuffsAudio.time = 0;
        m_handcuffsAudio.Play();
    }
}