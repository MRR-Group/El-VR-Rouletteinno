using System;
using LazySquirrelLabs.MinMaxRangeAttribute;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Round : NetworkBehaviour
{
    public event EventHandler RoundStared;

    public Gun Gun;
    
    [MinMaxRange(0, 6)]
    [SerializeField] 
    private Vector2Int m_itemCountRange = new (2, 6);
    
    public int CurrentItemCount { get; private set; }

    public void ItemsCountToGenerate()
    {
        CurrentItemCount = Random.Range(m_itemCountRange.x, m_itemCountRange.y + 1);
    }

    [Rpc(SendTo.Server)]
    public void StartRoundRpc()
    {
        ItemsCountToGenerate();
        Gun.ChangeMagazineRpc();
        SpawnItemBoxRpc();
        InvokeRoundStartedEventRpc();
    }

    [Rpc(SendTo.Server)]
    private void SpawnItemBoxRpc()
    {
        foreach (var index in GameManager.Instance.Game.AlivePlayers)
        {
            var player = PlayerManager.Instance.ById(index);
            
            player?.Inventory.SpawnItemBox(player.PlayerId);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void InvokeRoundStartedEventRpc()
    {
        RoundStared?.Invoke(this, EventArgs.Empty);
    }
}
