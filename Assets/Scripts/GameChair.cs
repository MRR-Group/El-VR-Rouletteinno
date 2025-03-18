using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameChair : NetworkBehaviour
{
    [SerializeField]
    private TeleportationAnchor m_anchor;
    
    private NetworkVariable<bool> isFree = new (true);

    public bool IsFree()
    {
        return isFree.Value;
    }
    
    public void SitDown()
    {
        Debug.Log("Player sit down");
        
        var player = NetworkManager.Singleton.LocalClient.ClientId;

        if (!IsFree() && GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }
        
        m_anchor.RequestTeleport();
        RegisterChairRpc(player);
        Debug.Log("Player sit down success");
    }
    
    [Rpc(SendTo.Server)]
    public void RegisterChairRpc(ulong player)
    {
        Debug.Log("Player sit down server");

        if (!IsFree() || GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }

        isFree.Value = false;
        GameManager.Instance.AddPlayer(player);
        
        Debug.Log("Player sit down server success");
    }
}
