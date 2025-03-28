using System;
using UnityEngine;
public class PlayClosingDoorSound : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_audio;
    
    [SerializeField]
    private Transform m_door;

    private bool _isOpening = false;
    
    void OnTriggerExit(Collider other)
    {
        if (other.transform == m_door)
        {
            _isOpening = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == m_door && _isOpening)
        {
            _isOpening = false;
            m_audio.time = 0;
            m_audio.Play();
        }
    }
}

