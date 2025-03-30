using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class GameChair : NetworkBehaviour
{
    
    [SerializeField]
    private TeleportationAnchor m_anchor;
    
    [SerializeField]
    private GameObject m_cage;
    
    private NetworkVariable<bool> net_isFree = new (true);
    private NetworkVariable<ulong> net_playerId = new ();
    
    public bool IsFree => net_isFree.Value;
    public Player Player => IsFree ? null : PlayerManager.Instance.ById(net_playerId.Value);
    
    [SerializeField]
    private Inventory m_inventory;
    public Inventory Inventory => m_inventory;
    
    [SerializeField]
    private ParticleSystem m_deathParticles;
 
    public void SitDown()
    {
        var player = NetworkManager.Singleton.LocalClient.ClientId;

        if (!IsFree || GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }
        
        m_anchor.RequestTeleport();
        RegisterChairRpc(player);
    }
    
    [Rpc(SendTo.Server)]
    public void RegisterChairRpc(ulong player)
    {
        if (!IsFree || GameManager.Instance.GameState != GameState.PREPARE)
        {
            return;
        }

        net_isFree.Value = false;
        Inventory.RegisterRpc(player);
        GameManager.Instance.AddPlayerRpc(player);
        
        net_playerId.Value = player;
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateCageRpc()
    {
        m_cage.SetActive(true);
    }
    
    [Rpc(SendTo.Everyone)]
    public void DeactivateCageRpc()
    {
        m_cage.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    public void DisplayDeathParticlesRpc()
    {
        m_deathParticles.time = 0;
        m_deathParticles.Play();
    }
}
