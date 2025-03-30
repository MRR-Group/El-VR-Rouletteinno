using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class StickyHand : TargetableItem<NetworkItem>
{

    public override bool Use(NetworkItem item)
    {
        if (item.OwnerId == NetworkManager.Singleton.LocalClientId)
        {
            return false;
        }

        StartCoroutine(StealItem(item));
        
        return true;
    }

    private IEnumerator StealItem(NetworkItem item)
    {
        yield return new WaitForSeconds(m_useAnimationTimeInSecounds);

        item.StealItem(_interactable.firstInteractorSelecting);

        yield return null;
    }
    
}