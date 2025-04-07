using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using XRMultiplayer;

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
    
    [SerializeField]
    private ParticleSystem m_winParticles;
    
    public override void OnNetworkSpawn()
    {
        XRINetworkGameManager.Instance.playerStateChanged += XRINetworkGameManager_OnPlayerConnectionChanged;
        
        if (NetworkManager.Singleton.IsServer)
        {
            net_isFree.Value = true;
            net_playerId.Value = 0;
        }
    }
    
    public override void OnNetworkDespawn()
    {
        XRINetworkGameManager.Instance.playerStateChanged -= XRINetworkGameManager_OnPlayerConnectionChanged;

        if (NetworkManager.Singleton.IsServer)
        {
            net_isFree.Value = true;
            net_playerId.Value = 0;
        }
    }
    
    private void XRINetworkGameManager_OnPlayerConnectionChanged(ulong playerId, bool isConnected)
    {
        Debug.Log($"Chair Player: {playerId} is Connected: {isConnected}");
        
        if (isConnected || net_isFree.Value)
        {
            return;
        }

        if (playerId == net_playerId.Value && NetworkManager.Singleton.IsServer)
        {
            net_isFree.Value = true;
            net_playerId.Value = 0;
        }
    }

    public void SitDown()
    {
        var player = NetworkManager.Singleton.LocalClient.ClientId;
        Debug.Log($"Is Free: {IsFree}, player: {player}, netPlayer: {net_playerId.Value}, state: {GameManager.Instance.GameState}");
        
        if (!IsFree && net_playerId.Value == player)
        {
            m_anchor.RequestTeleport();
            Debug.Log("Exit 1");
            return;
        }
        
        if (!IsFree || GameManager.Instance.GameState != GameState.PREPARE || InventoryManager.Instance.HasPlayerUi(player) )
        {
            Debug.Log("Exit 2");
            return;
        }
        
        m_anchor.RequestTeleport();
        RegisterChairRpc(player);
        Debug.Log("Exit 3");
    }
    
    [Rpc(SendTo.Server)]
    public void RegisterChairRpc(ulong player)
    {
        if (!IsFree || GameManager.Instance.GameState != GameState.PREPARE)
        {
            Debug.Log("Exit 4");
            return;
        }

        net_isFree.Value = false;
        Inventory.RegisterRpc(player);
        GameManager.Instance.AddPlayerRpc(player);
        Debug.Log("Player added! RPC called");
        
        net_playerId.Value = player;
        
        if (NetworkManager.Singleton.LocalClientId == player)
        {
            Inventory.SpawnStartGameButton(player);
        }
    }
    
    [Rpc(SendTo.Everyone)]
    public void ActivateCageRpc()
    {
        m_cage.SetActive(true);
    }
    
    [Rpc(SendTo.Everyone)]
    public void DeactivateCageRpc()
    {
        m_cage.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    public void DisplayDeathParticlesRpc()
    {
        m_deathParticles.time = 0;
        m_deathParticles.Play();
    }
    
    [Rpc(SendTo.Everyone)]
    public void DisplayWinParticlesRpc()
    {
        m_winParticles.time = 0;
        m_winParticles.Play();
    }
    
}
