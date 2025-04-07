using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

public class ItemBox : NetworkItem
{
    [SerializeField]
    private AudioSource m_openingSound;
    
    public Inventory Inventory => InventoryManager.Instance.ByClientId(OwnerId);
    
    public override bool Use()
    {
        PlayOpeningSoundRpc();
        
        Invoke(nameof(OpenItemBox), m_useAnimationTimeInSecounds);
        
        return true;
    }

    private void OpenItemBox()
    {
        Inventory.SpawnRandomItemsRpc(NetworkManager.Singleton.LocalClientId);
        Inventory.MarkItemBoxAsUsedRpc();
    }

    protected override bool CanUse(ulong currentPlayer)
    {
        Debug.Log($"Can use box: Owener {OwnerId}, currwetnPlayer: {currentPlayer}");
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && OwnerId == currentPlayer;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayOpeningSoundRpc()
    {
        m_openingSound.time = 0;
        m_openingSound.Play();
    }
}