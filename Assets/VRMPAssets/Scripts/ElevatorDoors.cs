using System;
using Unity.Mathematics.Geometry;
using Unity.Netcode;
using UnityEngine;

namespace XRMultiplayer
{
    public class ElevatorDoors : NetworkBehaviour
    {
        private NetworkVariable<bool> net_areOpen = new (false);
        
        [SerializeField] 
        private Vector3 m_doorSize;
        
        [SerializeField] 
        private Transform m_leftDoor;
        
        [SerializeField] 
        private Transform m_rightDoor;
        
        [SerializeField] 
        private float m_closingSpeed = 2.0f;
        
        private Vector3 _startPositionLeft;
        private Vector3 _startPositionRight;
        private Vector3 _endPositionLeft;
        private Vector3 _endPositionRight;
        
        private NetworkVariable<float> net_timeToAutoClose = new(10);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                net_areOpen.OnValueChanged += OnAreOpenValueChanged;
                net_areOpen.Value = true;
            }
        }

        public void OnConnectedToServer()
        {
            OpenRpc();
        }

        [Rpc(SendTo.Server)]
        private void OpenRpc()
        {
            net_areOpen.Value = true;
        }

        private void OnAreOpenValueChanged(bool _, bool areOpen)
        {
            if (areOpen)
            {
                net_timeToAutoClose.Value = 10.0f;
            }
        }
        
        [Rpc(SendTo.Server)]
        public void ToggleRpc()
        {
            net_areOpen.Value = !net_areOpen.Value;
        }

        void Start()
        {
            _startPositionLeft = m_leftDoor.position;
            _startPositionRight = m_rightDoor.position;
            _endPositionLeft = m_leftDoor.position - m_doorSize;
            _endPositionRight = m_rightDoor.position + m_doorSize;
        }
        
        void Update()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                net_timeToAutoClose.Value -= Time.deltaTime;

                if (net_timeToAutoClose.Value <= 0)
                {
                    net_timeToAutoClose.Value = 10;
                    net_areOpen.Value = false;
                }
            }
            
            var leftTargetPosition = net_areOpen.Value ? _endPositionLeft : _startPositionLeft;
            m_leftDoor.position = Vector3.MoveTowards(m_leftDoor.position, leftTargetPosition, Time.deltaTime * m_closingSpeed);
            
            var rightTargetPosition = net_areOpen.Value ? _endPositionRight : _startPositionRight;
            m_rightDoor.position = Vector3.MoveTowards(m_rightDoor.position, rightTargetPosition, Time.deltaTime * m_closingSpeed);
        }
    }
}