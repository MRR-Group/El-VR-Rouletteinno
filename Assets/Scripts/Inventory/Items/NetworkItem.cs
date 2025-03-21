using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
    protected NetworkPhysicsInteractable _physicsInteractable;
    protected NetworkObject _networkObject;
    
    [SerializeField]
    protected Transform m_spawnPoint;
    
    private const string ITEM_BOX_TAG = "ItemBox";

    protected bool _isInBox;
    protected bool _isInUse;
    protected bool _isGrabbed;
    
    [SerializeField]
    protected int m_usages = 1;
    
    protected NetworkVariable<int> net_usages = new();
    
    [SerializeField]
    protected bool m_isIndestructible;
    
    protected virtual void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
        _physicsInteractable = GetComponent<NetworkPhysicsInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();
    }
    protected virtual void Start()
    {
        _interactable.selectEntered.AddListener(HandleGrab);
        _interactable.selectExited.AddListener(HandleDrop);
        _interactable.interactionManager = GameManager.Instance.InteractionManager;
        _physicsInteractable.ActivateNetworkedEventAll.AddListener(Network_OnActivate);
    }

    public override void OnNetworkSpawn()
    {
        net_usages.Value = m_usages;
    }

    public override void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(HandleGrab);
        _interactable.selectExited.RemoveListener(HandleDrop);
        _physicsInteractable.ActivateNetworkedEventAll.RemoveListener(Network_OnActivate);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ITEM_BOX_TAG))
        {
            _isInBox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(ITEM_BOX_TAG))
        {
            return;
        }
        
        _isInBox = false;
        
        ReturnToSpawnIfDropped();
    }

    private void ReturnToSpawnIfDropped()
    {
        if (_isGrabbed || _isInBox)
        {
            return;
        }
        
        TeleportToSpawnRpc();
    }

    protected void HandleGrab(SelectEnterEventArgs _)
    {
        _isGrabbed = true;
        
        if (!CanUse())
        {
            ForceDrop();
        }
    }

    protected void ForceDrop()
    {
        GameManager.Instance.InteractionManager.SelectExit(_interactable.firstInteractorSelecting, _interactable);
    }

    protected void HandleDrop(SelectExitEventArgs _)
    {
        _isGrabbed = false;
        ReturnToSpawnIfDropped();
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        _rigidbody.MovePosition(m_spawnPoint.position);
        _rigidbody.MoveRotation(Quaternion.identity);
    }
    
    public void SetSpawnPoint(Transform spawnPoint)
    {
        m_spawnPoint = spawnPoint;
    }

    protected virtual void Network_OnActivate(bool isActive)
    {
        _isInUse = isActive;

        if (!isActive)
        {
            return;
        }

        if (!CanUse())
        {
            ForceDrop();
            
            return;
        }

        if (!Use())
        {
            return;
        }
        
        if (m_isIndestructible)
        {
            ForceDrop();
        }
        else
        {
            DecrementUsagesRpc();
        }
        
        if (net_usages.Value <= 0)
        {
            DestroyItemRpc();
        }
    }
    
    protected virtual bool CanUse()
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && GameManager.Instance.Turn.IsClientTurn();
    }

    [Rpc(SendTo.Server)]
    protected void DecrementUsagesRpc()
    {
        if (!m_isIndestructible)
        {
            net_usages.Value -= 1;
        }
    }
    
    [Rpc(SendTo.Server)]
    public void DestroyItemRpc()
    {
        _networkObject.Despawn();
    }

    public abstract bool Use();
}
