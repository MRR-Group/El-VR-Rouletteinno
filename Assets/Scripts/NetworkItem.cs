using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
public abstract class NetworkItem : NetworkBehaviour
{
    protected Rigidbody rb;
    protected XRGrabInteractable interactable;

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
    }

    protected void Start()
    {
        interactable.selectEntered.AddListener((_) => HandleGrab());
        interactable.selectExited.AddListener((_) => HandleDrop());
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

    protected void HandleGrab()
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

    protected void HandleDrop()
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

    public void DestroyItem()
    {
        Destroy(rb.gameObject);
    }
    
    public abstract void Use(ulong target);
}
