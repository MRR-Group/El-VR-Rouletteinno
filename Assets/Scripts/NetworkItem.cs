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
    protected Rigidbody rigidbody;
    protected XRGrabInteractable interactable;

    [SerializeField]
    protected Transform m_spawnPoint;
    
    [SerializeField]
    protected XRInteractionManager m_interactionManager;

    protected void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        rigidbody = GetComponent<Rigidbody>();
    }

    protected void Start()
    {
        interactable.selectEntered.AddListener((_) => HandleGrab());
        interactable.selectExited.AddListener((_) => HandleDrop());
    }

    protected void HandleGrab()
    {
        Debug.Log("Grabed");
        
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
        TeleportToSpawn();
    }

    public void TeleportToSpawn()
    {
        rigidbody.MovePosition(m_spawnPoint.position);
    }
    
    protected bool CanUse()
    {
        return true;
        
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && GameManager.Instance.turn.IsClientTurn();
    }

    public abstract void Use(ulong target);
}
