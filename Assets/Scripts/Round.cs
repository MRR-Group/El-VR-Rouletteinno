using System;
using Unity.Netcode;
using UnityEngine;

public class Round : NetworkBehaviour
{
    public event EventHandler RoundStared;
    
    public Gun gun;

    [Rpc(SendTo.Server)]
    public void StartRoundRpc()
    {
        gun.ChangeMagazineRpc();
        InvokeRoundStartedEventRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void InvokeRoundStartedEventRpc()
    {
        RoundStared?.Invoke(this, EventArgs.Empty);
    }
}
