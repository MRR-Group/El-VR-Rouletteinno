using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    public class ElevatorDoors : NetworkBehaviour
    {
        private NetworkVariable<bool> net_isOpen = new();
        
        [SerializeField]
        private Vector3 m_doorSize;
        
        [SerializeField] 
        private Transform m_leftDoor;
        
        [SerializeField] 
        private Transform m_rightDoor;
        
        [SerializeField] 
        private float m_closingSpeed = 0.5f;
        
        [SerializeField] 
        private float m_closeDelay = 10f;

        [SerializeField]
        private AudioSource m_audio;
        
        [SerializeField]
        private AudioSource m_buttonAudio;
        
        private Vector3 startPositionLeft;
        private Vector3 startPositionRight;
        private Vector3 endPositionLeft;
        private Vector3 endPositionRight;
        private Coroutine closeCoroutine;

        [Rpc(SendTo.Server)]
        private void OpenRpc()
        {
            net_isOpen.Value = true;
            HandleAutoClose();
            PlaySoundEffectRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void PlaySoundEffectRpc()
        {
            m_audio.Play();
            
            if (m_audio.time >= m_audio.clip.length)
            {
                m_audio.time = 0;
            }
        }

        private void HandleAutoClose()
        {
            StopCoroutine(closeCoroutine);

            if (!net_isOpen.Value)
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
            net_isOpen.Value = false;

            PlaySoundEffectRpc();
        }

        private IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(m_closeDelay);
            CloseRpc();
        }

        [Rpc(SendTo.Server)]
        public void ToggleDoorStateRpc()
        {
            net_isOpen.Value = !net_isOpen.Value;
            HandleAutoClose();
            PlaySoundEffectRpc();
            PlayButtonSoundRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void PlayButtonSoundRpc()
        {
            m_buttonAudio.time = 0;
            m_buttonAudio.Play();
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
            var leftTargetPosition = net_isOpen.Value ? endPositionLeft : startPositionLeft;
            
            m_leftDoor.position =
                Vector3.MoveTowards(m_leftDoor.position, leftTargetPosition, Time.deltaTime * m_closingSpeed);

            var rightTargetPosition = net_isOpen.Value ? endPositionRight : startPositionRight;
            
            m_rightDoor.position =
                Vector3.MoveTowards(m_rightDoor.position, rightTargetPosition, Time.deltaTime * m_closingSpeed);
        }
    }
}