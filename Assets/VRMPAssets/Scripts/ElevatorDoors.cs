using System.Collections;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    public class ElevatorDoors : NetworkBehaviour
    {
        private NetworkVariable<bool> isOpen  = new();
        [SerializeField] private Vector3 m_doorSize;
        [SerializeField] private Transform m_leftDoor;
        [SerializeField] private Transform m_rightDoor;
        [SerializeField] private float m_closingSpeed;
        [SerializeField] private float m_closeDelay = 10f;
        private Vector3 startPositionLeft;
        private Vector3 startPositionRight;
        private Vector3 endPositionLeft;
        private Vector3 endPositionRight;
        private Coroutine closeCoroutine;
        
        [Rpc(SendTo.Server)]
        private void OpenRpc()
        {
            isOpen.Value = true;
            HandleAutoClose();
        }

        private void HandleAutoClose()
        {
            if (!isOpen.Value)
            {
                return;
            }
            

            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }

            closeCoroutine = StartCoroutine(AutoClose());
        }

        [Rpc(SendTo.Server)]
        private void CloseRpc()
        {
            isOpen.Value = false;
        }
        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(10f);
            CloseRpc();
        }

        [Rpc(SendTo.Server)]
        public void ToggleDoorStateRpc()
        {
            isOpen.Value = !isOpen.Value;
            HandleAutoClose();
        }

        private void Start()
        {
            startPositionLeft = m_leftDoor.position;
            startPositionRight = m_rightDoor.position;
            endPositionLeft = m_leftDoor.position - m_doorSize;
            endPositionRight = m_rightDoor.position + m_doorSize;
            
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnOnClientConnectedCallback;
        }

        private void NetworkManager_OnOnClientConnectedCallback(ulong obj)
        {
            OpenRpc();
        }

        private void Update()
        {
            var leftTargetPosition = isOpen.Value ? endPositionLeft : startPositionLeft;
            m_leftDoor.position = Vector3.Lerp(m_leftDoor.position, leftTargetPosition, Time.deltaTime * m_closingSpeed);
            
            var rightTargetPosition = isOpen.Value ? endPositionRight : startPositionRight;
            m_rightDoor.position = Vector3.Lerp(m_rightDoor.position, rightTargetPosition, Time.deltaTime * m_closingSpeed);
        }
    }
}