using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(XRGrabInteractable))]
public abstract class NetworkItem : NetworkBehaviour
{
    protected Rigidbody _rigidbody;
    protected XRGrabInteractable _interactable;
    protected NetworkObject _networkObject;
    
    [SerializeField]
    protected Transform m_spawnPoint;
    
    private const string ITEM_BOX_TAG = "ItemBox";

    private bool _isInBox = false;
    private bool _isGrabbed;
    
    protected void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();
    }
    protected void Start()
    {
        _interactable.selectEntered.AddListener(HandleGrab);
        _interactable.selectExited.AddListener(HandleDrop);
    }
    
    public override void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(HandleGrab);
        _interactable.selectExited.RemoveListener(HandleDrop);
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
    
    protected bool CanUse()
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && GameManager.Instance.Turn.IsClientTurn();
    }
    
    [Rpc(SendTo.Server)]
    public void DestroyItemRpc()
    {
        _networkObject.Despawn();
    }
    
    public abstract void Use(ulong target);
}
