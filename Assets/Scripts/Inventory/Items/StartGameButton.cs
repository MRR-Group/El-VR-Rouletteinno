using Unity.Netcode;
using UnityEngine;

public class StartGameButton : NetworkItem
{
    [SerializeField]
    private AudioSource m_clickSound;
    
    public override bool Use()
    {
        PlaySoundRpc();
        
        GameManager.Instance.TryStartGame();
        
        return true;
    }
    
    protected override bool CanUse(ulong currentPlayer)
    {
        return GameManager.Instance.GameState == GameState.PREPARE && OwnerId == currentPlayer;
    }

    [Rpc(SendTo.Everyone)]
    private void PlaySoundRpc()
    {
        m_clickSound.time = 0;
        m_clickSound.Play();
    }
}