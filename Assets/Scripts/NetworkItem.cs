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
    protected Rigidbody rb;
    protected XRGrabInteractable interactable;
    protected NetworkObject networkObject;
    
    [SerializeField]
    protected Transform m_spawnPoint;
    
    [SerializeField]
    protected XRInteractionManager m_interactionManager;

    private const string ITEM_BOX_TAG = "ItemBox";

    private bool isInBox = false;
    private bool isGrabbed;
    
    protected void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ITEM_BOX_TAG))
        {
            isInBox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(ITEM_BOX_TAG))
        {
            return;
        }
        
        isInBox = false;
        
        ReturnToSpawnIfDropped();
    }

    private void ReturnToSpawnIfDropped()
    {
        if (isGrabbed || isInBox)
        {
            return;
        }
        
        TeleportToSpawnRpc();
    }

    protected void HandleGrab(SelectEnterEventArgs _)
    {
        Debug.Log("Grabed");
        
        isGrabbed = true;
        
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
        Debug.Log("Dropped");
        isGrabbed = false;
        ReturnToSpawnIfDropped();
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        rb.MovePosition(m_spawnPoint.position);
        rb.MoveRotation(Quaternion.identity);
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
