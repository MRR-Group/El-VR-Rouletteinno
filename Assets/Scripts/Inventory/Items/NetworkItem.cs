using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using XRMultiplayer;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(XRGeneralGrabTransformer))]
[RequireComponent(typeof(NetworkPhysicsInteractable))]
[RequireComponent(typeof(ClientNetworkTransform))]
public abstract class NetworkItem : NetworkBehaviour
{
    protected Rigidbody _rigidbody;
    protected XRGrabInteractable _interactable;
    protected NetworkObject _networkObject;

    [SerializeField] 
    protected Transform m_startingSpawnPoint;
    
    protected NetworkVariable<Vector3> net_spawnPoint = new();
    
    protected bool _isInBox;
    protected bool _isActionButtonPressed;
    protected bool _isGrabbed;
    protected bool _wasForceGrabbed;
    protected bool _isAnimatingUsage;
    protected bool _shouldBeInSpawn;
    
    [SerializeField] 
    protected int m_usages = 1;

    protected NetworkVariable<int> net_usages = new();
    protected NetworkVariable<ulong> net_used_by = new();

    [SerializeField] 
    protected bool m_isIndestructible;

    [SerializeField] 
    protected bool m_disableDropWhileInUse = true;

    [SerializeField] 
    protected int m_useAnimationTimeInSecounds = 0;
    
    [SerializeField] 
    protected Dissolve m_dissolve;
    
    private NetworkVariable<ulong> net_ownerId = new ();
    public ulong OwnerId => net_ownerId.Value;
    private bool _isOwnerAssigned = false;

    private const int EMPTY_SLOT = -1;
    protected int InventorySlotId = EMPTY_SLOT;

    protected void Update()
    {
        if (IsOwner && Vector3.Distance(transform.position, net_spawnPoint.Value) <= 0.1f)
        {
            _shouldBeInSpawn = false;
        }
        
        if (IsOwner && _shouldBeInSpawn)
        {
            _rigidbody.MovePosition(net_spawnPoint.Value);
            _rigidbody.MoveRotation(Quaternion.identity);   
        }
    }

    public virtual void EnterInventoryBox(InventoryBox box)
    {
        if (box.Player?.PlayerId != net_ownerId.Value)
        {
            return;
        }

        _isInBox = true;
    }

    public virtual void ExitInventoryBox(InventoryBox box)
    {
        if (box.Player?.PlayerId == net_ownerId.Value)
        {
            _isInBox = false;
        }
        
        ReturnToSpawnIfDropped();
    }

    protected virtual void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();
    }

    protected virtual void Start()
    {
        _interactable.selectEntered.AddListener(HandleGrab);
        _interactable.selectExited.AddListener(HandleDrop);
        _interactable.interactionManager = GameManager.Instance.InteractionManager;
        _interactable.activated.AddListener(Interactable_OnActivate);
    }

    public override void OnNetworkSpawn()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        net_usages.Value = m_usages;

        if (m_startingSpawnPoint != null)
        {
            net_spawnPoint.Value = m_startingSpawnPoint.position;
        }
    }

    public override void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(HandleGrab);
        _interactable.selectExited.RemoveListener(HandleDrop);
        _interactable.activated.RemoveListener(Interactable_OnActivate);
    }
    
    protected void ReturnToSpawnIfDropped()
    {
        if (_isGrabbed || _isInBox)
        {
            return;
        }
        
        Debug.Log("Teleport to spawn!!!!");
        TeleportToSpawnRpc();
    }

    protected void HandleGrab(SelectEnterEventArgs _)
    {
        _isGrabbed = true;
        
        if (!CanUse(NetworkManager.Singleton.LocalClientId))
        {
            ForceDrop();
        }

        UpdateUserRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void UpdateUserRpc(ulong grabber)
    {
        if (CanUse(grabber))
        {
            net_used_by.Value = grabber;
        }
    }

    protected void ForceDrop()
    {
        if (_interactable.firstInteractorSelecting == null)
        {
            return;
        }
        
        GameManager.Instance.InteractionManager.SelectExit(_interactable.firstInteractorSelecting, _interactable);
        ReturnToSpawnIfDropped();
    }
    
    protected void ForceGrab(IXRSelectInteractor interactor)
    {
        _wasForceGrabbed = true;
        GameManager.Instance.InteractionManager.SelectEnter(interactor, _interactable);
    }

    protected void HandleDrop(SelectExitEventArgs e)
    {
        if (_isAnimatingUsage && m_disableDropWhileInUse)
        {
            if (m_disableDropWhileInUse)
            {
                ForceGrab(e.interactorObject);
            }

            return;
        }
        
        _isGrabbed = false;
        _wasForceGrabbed = false;

        ReturnToSpawnIfDropped();
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        _shouldBeInSpawn = true;
    }

    [Rpc(SendTo.Server)]
    public void SetSpawnPointRpc(Vector3 spawnPoint)
    {
        net_spawnPoint.Value = spawnPoint;
    }

    [Rpc(SendTo.Server)]
    public void SetOwnerRpc(ulong ownerId)
    {
        net_ownerId.Value = ownerId;
        _isOwnerAssigned = true;
    }
    
    [Rpc(SendTo.Everyone)]
    public void SetInventorySlotIdRpc(int slotId)
    {
        InventorySlotId = slotId;
    }

    protected virtual void Interactable_OnActivate(ActivateEventArgs e)
    {
        if (_isAnimatingUsage)
        {
            return;
        }

        var clientId = NetworkManager.Singleton.LocalClientId;

        if (!CanUse(clientId))
        {
            ForceDrop();

            return;
        }

        _isAnimatingUsage = true;

        if (!Use())
        {
            _isAnimatingUsage = false;
            return;
        }

        StartCoroutine(UsageAnimation(clientId));
    }

    protected IEnumerator UsageAnimation(ulong clientId)
    {
        if (m_useAnimationTimeInSecounds > 0)
        {
            yield return new WaitForSeconds(m_useAnimationTimeInSecounds);
        }
        
        if (m_dissolve && m_usages <= 1 && !m_isIndestructible)
        {
            m_dissolve.DissolveRpc();
            yield return new WaitForSeconds(m_dissolve.DissolvingTime);
        }
        
        _isAnimatingUsage = false;

        if (m_isIndestructible || _wasForceGrabbed)
        {
            ForceDrop();
        }
        
        AfterUseRpc(clientId);

        yield return null;
    }

    [Rpc(SendTo.Server)]
    protected void AfterUseRpc(ulong clientId)
    {
        DecrementUsages();

        if (net_usages.Value <= 0)
        {
            DestroyItem(clientId);
        }
    }

    protected virtual bool CanUse(ulong currentPlayer)
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS &&
               GameManager.Instance.Turn.IsPlayerTurn(currentPlayer) &&
               (!_isOwnerAssigned || OwnerId.Equals(currentPlayer));
    }

    protected void DecrementUsages()
    {
        if (!m_isIndestructible)
        {
            net_usages.Value -= 1;
        }
    }

    public void DestroyItem(ulong clientId)
    {
        _networkObject.Despawn();

        if (InventorySlotId < 0)
        {
            return;
        }
        
        var inventory = InventoryManager.Instance.ByClientId(clientId);
        var slot = inventory.GetSlot(InventorySlotId);
        
        slot.vacateRpc();
    }

    public void StealItem(IXRSelectInteractor interactor)
    {
        if (_isAnimatingUsage)
        {
            return;
        }

        var newOwnerId = NetworkManager.Singleton.LocalClientId;
        var oldOwner = OwnerId;
        
        SetOwnerRpc(newOwnerId);

        if (!CanUse(newOwnerId))
        {
            ForceDrop();

            return;
        }

        _isAnimatingUsage = true;

        if (!Use())
        {
            _isAnimatingUsage = false;
            return;
        }

        if (m_disableDropWhileInUse)
        {
            ForceGrab(interactor);
        }
        
        StartCoroutine(UsageAnimation(oldOwner));
        
        SetOwnerRpc(oldOwner);
    }


    public abstract bool Use();
}