using System;
using Unity.Netcode;

public class Round : NetworkBehaviour
{
    public event EventHandler RoundStared;

    public Gun Gun { get; }

    [Rpc(SendTo.Server)]
    public void StartRoundRpc()
    {
        Gun.ChangeMagazineRpc();
        InvokeRoundStartedEventRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void InvokeRoundStartedEventRpc()
    {
        RoundStared?.Invoke(this, EventArgs.Empty);
    }
}
