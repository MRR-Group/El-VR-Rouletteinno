using UnityEngine;

public class Slot : MonoBehaviour
{
    public NetworkItem Item = null;
    [SerializeField]
    private Transform m_spawnPoint;
    public Transform SpawnPoint => m_spawnPoint;
}
