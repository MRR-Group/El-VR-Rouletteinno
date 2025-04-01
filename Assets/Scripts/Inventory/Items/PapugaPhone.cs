using System.Collections;
using System.Xml.Serialization;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using XRMultiplayer;

public class PapugaPhone : NetworkItem
{
    [SerializeField]
    private GameObject m_screen;

    [SerializeField]
    private AudioClip m_ringtone;

    [SerializeField]
    private AudioClip[] m_blankSound;

    [SerializeField]
    private AudioClip[] m_liveSound;

    [SerializeField]
    private AudioClip m_noAnswer;

    [SerializeField]
    private AudioSource m_audioSource;

    [SerializeField]
    private int m_minAmmo = 2;

    private int _selectedBullet;
    
    private string[] _ammoSlotNames = 
    { 
        "First", "Second", "Third", "Fourth", "Fifth", 
        "Sixth", "Seventh", "Eighth", "Ninth", "Tenth" 
    };

    public override bool Use()
    {
        var magazine = GameManager.Instance.Round.Gun.Magazine();
        
        Invoke(nameof(DisplayPapugaRpc), 1.5f);
        PlayRingtoneRpc();
        SelectRandomBullet(magazine);
        
        var prophecyClip = GetProphecyClip(magazine);

        AdjustAnimationTime(prophecyClip);
        
        StartCoroutine(PlayProphecy(prophecyClip, m_ringtone.length));
        StartCoroutine(DelayedDisplayAnswerNotification(magazine, m_ringtone.length + 4.0f));
       
        return true;
    }

    private void SelectRandomBullet(bool[] magazine)
    {
        _selectedBullet = UnityEngine.Random.Range(0, magazine.Length);
    }

    private IEnumerator DelayedDisplayAnswerNotification(bool[] magazine, float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayAnswerNotification(magazine);
    }
    
    private void DisplayAnswerNotification(bool[] magazine)
    {
        var bullet = magazine[_selectedBullet];
        
        var bulletType = bullet ? "live" : "blank";
        var bulletSlot = _selectedBullet;

        if (magazine.Length <= m_minAmmo)
        {
            PlayerHudNotification.Instance.ShowText($"Try again later...");
        }
        
        PlayerHudNotification.Instance.ShowText($"{_ammoSlotNames[bulletSlot]} bullet is {bulletType}");
    }

    private AudioClip GetProphecyClip(bool[] magazine)
    {
        var bullet = magazine[_selectedBullet];

        if(magazine.Length <= m_minAmmo)
        {
            return m_noAnswer;
        }

        return bullet ? m_liveSound[_selectedBullet] : m_blankSound[_selectedBullet];
    }

    private IEnumerator PlayProphecy(AudioClip prophecyClip, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        m_audioSource.clip = prophecyClip;
        m_audioSource.time = 0;
        m_audioSource.Play();
    }

    private void AdjustAnimationTime(AudioClip clip)
    {
        m_useAnimationTimeInSecounds = Mathf.CeilToInt(m_ringtone.length + clip.length);
    }

    [Rpc(SendTo.Everyone)]
    private void DisplayPapugaRpc()
    {
        m_screen.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayRingtoneRpc()
    {
        m_audioSource.clip = m_ringtone;
        m_audioSource.time = 0;
        m_audioSource.Play();
    }
}
