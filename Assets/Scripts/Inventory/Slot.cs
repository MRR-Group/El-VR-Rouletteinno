using Unity.Netcode;
using UnityEngine;

public class Slot : NetworkBehaviour
{
    public NetworkItem Item = null;
    private NetworkVariable<bool> isFree = new (true);
    public bool IsFree => isFree.Value;
    
    [SerializeField]
    private Transform m_spawnPoint;
    
    [SerializeField]
    private Inventory m_inventory;
    public Inventory Inventory => m_inventory;
    public Transform SpawnPoint => m_spawnPoint;

    [Rpc(SendTo.Server)]
    public void OccupyRpc()
    {
        isFree.Value = false;
    }
}
