using Unity.Mathematics.Geometry;
using UnityEngine;

namespace XRMultiplayer
{
    public class ElevatorDoors : MonoBehaviour
    {
        bool isOpen = false;
        [SerializeField] private Vector3 m_doorSize;
        [SerializeField] private Transform m_leftDoor;
        [SerializeField] private Transform m_rightDoor;
        [SerializeField] private float m_closingSpeed;
        private Vector3 startPositionLeft;
        private Vector3 startPositionRight;
        private Vector3 endPositionLeft;
        private Vector3 endPositionRight;

        public void Open()
        {
            isOpen = true;
        }

        public void Close()
        {
            isOpen = false;
        }

        void Start()
        {
            startPositionLeft = m_leftDoor.position;
            startPositionRight = m_rightDoor.position;
            endPositionLeft = m_leftDoor.position - m_doorSize;
            endPositionRight = m_rightDoor.position + m_doorSize;
        }

        // Update is called once per frame

        void Update()
        {
            var leftTargetPosition = isOpen ? endPositionLeft : startPositionLeft;
            m_leftDoor.position = Vector3.Lerp(m_leftDoor.position, leftTargetPosition, Time.deltaTime * m_closingSpeed);
            
            var rightTargetPosition = isOpen ? endPositionRight : startPositionRight;
            m_rightDoor.position = Vector3.Lerp(m_rightDoor.position, rightTargetPosition, Time.deltaTime * m_closingSpeed);
        }
    }
}