using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
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

    [SerializeField] protected Transform m_startingSpawnPoint;
    protected NetworkVariable<Vector3> net_spawnPoint = new();

    private const string ITEM_BOX_TAG = "ItemBox";

    protected bool _isInBox;
    protected bool _isActionButtonPressed;
    protected bool _isGrabbed;
    protected bool _isAnimatingUsage;

    [SerializeField] protected int m_usages = 1;

    protected NetworkVariable<int> net_usages = new();
    protected NetworkVariable<ulong> net_used_by = new();

    [SerializeField] protected bool m_isIndestructible;

    [SerializeField] protected int m_useAnimationTimeInSecounds = 0;

    private NetworkVariable<ulong> _ownerId = new ();
    public ulong OwnerId => _ownerId.Value;
    private bool _isOwnerAssigned = false;


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

        if (!CanUse(NetworkManager.Singleton.LocalClientId))
        {
            ForceDrop();
        }

        GrabRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void GrabRpc(ulong grabber)
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
    }

    protected void HandleDrop(SelectExitEventArgs _)
    {
        _isGrabbed = false;
        ReturnToSpawnIfDropped();
    }

    [Rpc(SendTo.Owner)]
    public void TeleportToSpawnRpc()
    {
        _rigidbody.MovePosition(net_spawnPoint.Value);
        _rigidbody.MoveRotation(Quaternion.identity);
    }

    [Rpc(SendTo.Server)]
    public void SetSpawnPointRpc(Vector3 spawnPoint)
    {
        net_spawnPoint.Value = spawnPoint;
    }

    [Rpc(SendTo.Server)]
    public void SetItemOwnerRpc(ulong ownerId)
    {
        _ownerId.Value = ownerId;
        _isOwnerAssigned = true;
    }

    protected virtual void Interactable_OnActivate(ActivateEventArgs e)
    {
        if (_isAnimatingUsage)
        {
            return;
        }


        if (!CanUse(NetworkManager.Singleton.LocalClientId))
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

        StartCoroutine(UsageAnimation());
    }

    protected IEnumerator UsageAnimation()
    {
        if (m_useAnimationTimeInSecounds > 0)
        {
            yield return new WaitForSeconds(m_useAnimationTimeInSecounds);
        }

        if (m_isIndestructible)
        {
            ForceDrop();
        }

        AfterUseRpc();

        _isAnimatingUsage = false;

        yield return null;
    }

    [Rpc(SendTo.Server)]
    protected void AfterUseRpc()
    {
        DecrementUsages();

        if (net_usages.Value <= 0)
        {
            DestroyItem();
        }
    }

    protected virtual bool CanUse(ulong currentPlayer)
    {
        return GameManager.Instance.GameState == GameState.IN_PROGRESS &&
               GameManager.Instance.Turn.IsPlayerTurn(currentPlayer) &&
               (!_isOwnerAssigned || _ownerId.Value.Equals(currentPlayer));
    }

    protected void DecrementUsages()
    {
        if (!m_isIndestructible)
        {
            net_usages.Value -= 1;
        }
    }

    public void DestroyItem()
    {
        _networkObject.Despawn();
    }

    public abstract bool Use();
}