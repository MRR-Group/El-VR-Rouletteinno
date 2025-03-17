using System;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<int> Health = new NetworkVariable<int>(5);
}