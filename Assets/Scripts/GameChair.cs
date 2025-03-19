using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameChair : NetworkBehaviour
{
    [SerializeField]
    private TeleportationAnchor m_anchor;
    
    private NetworkVariable<bool> isFree = new (true);
    private NetworkVariable<ulong> playerId = new ();

    public Player Player => isFree.Value ? null : PlayerManager.Instance.Player[playerId.Value];
    
    public bool IsFree()
    {
        return isFree.Value;
    }
    
    public void SitDown()
    {
        var player = NetworkManager.Singleton.LocalClient.ClientId;

        if (!IsFree() && GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }
        
        m_anchor.RequestTeleport();
        RegisterChairRpc(player);
    }
    
    [Rpc(SendTo.Server)]
    public void RegisterChairRpc(ulong player)
    {
        if (!IsFree() || GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }

        isFree.Value = false;
        GameManager.Instance.AddPlayerRpc(player);
        playerId.Value = player;
    }
}
