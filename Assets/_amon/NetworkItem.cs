using Unity.Netcode;
using UnityEngine;

public abstract class NetworkItem : NetworkBehaviour
{
    protected bool CanUse()
    {
        Debug.Log("Is client: " + GameManager.Instance.turn.IsClientTurn());
        return GameManager.Instance.GameState == GameState.IN_PROGRESS && GameManager.Instance.turn.IsClientTurn();
    }

    public abstract void Use(ulong target);
}
