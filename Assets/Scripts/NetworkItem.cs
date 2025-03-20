using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
public abstract class NetworkItem : NetworkBehaviour
{
    protected Rigidbody rigidbody;
    protected XRGrabInteractable interactable;
    protected NetworkObject networkObject;
    
    [SerializeField]
    protected Transform m_spawnPoint;
    
    [SerializeField]
    protected XRInteractionManager m_interactionManager;
    
    protected void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        rigidbody = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
    }
    protected void Start()
    {
        interactable.selectEntered.AddListener(HandleGrab);
        interactable.selectExited.AddListener(HandleDrop);
    }
    
    public override void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(HandleGrab);
        interactable.selectExited.RemoveListener(HandleDrop);
    }

    private void HandleGrab(SelectEnterEventArgs _)
    {
        if (!CanUse())
        {
            ForceDrop();
        }
    }

    protected void ForceDrop()
    {
        m_interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
    }

    protected void HandleDrop(SelectExitEventArgs _)
    {
        TeleportToSpawnRpc();
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        rigidbody.MovePosition(m_spawnPoint.position);
    }
    
    protected bool CanUse()
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && GameManager.Instance.turn.IsClientTurn();
    }
    
    [Rpc(SendTo.Server)]
    public void DestroyItemRpc()
    {
        networkObject.Despawn();
    }
    
    public abstract void Use(ulong target);
}
