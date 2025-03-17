using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameChair : NetworkBehaviour
{
    [SerializeField]
    private GameManager m_gameManager;
    
    [SerializeField]
    private TeleportationAnchor m_anchor;
    
    private NetworkVariable<bool> isFree = new (true);

    public bool IsFree()
    {
        return isFree.Value;
    }
    
    public void SitDown()
    {
        var player = NetworkManager.Singleton.LocalClient.ClientId;
        Debug.Log("SitDown: " + player);

        if (!IsFree())
        {
            return;
        }

        isFree.Value = false;
        m_gameManager.AddPlayer(player);
        m_anchor.RequestTeleport();
    }
}
