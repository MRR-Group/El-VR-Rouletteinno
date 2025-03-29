using System.Xml.Serialization;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

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

    private int _selectedBullet;

    protected override void Start()
    {
        base.Start();
    }

    public override bool Use()
    {
        Invoke(nameof(DisplayPapugaRpc), 1.5f);
        PlayRingtoneRpc();

        SelectRandomBullet();

        AdjustAnimationTime(GetProphecyClip());
        Invoke(nameof(PlayProphecy), m_ringtone.length);
       
        return true;
    }

    private void SelectRandomBullet()
    {
        var magazine = GameManager.Instance.Round.Gun.Magazine();
        _selectedBullet = UnityEngine.Random.Range(0, magazine.Length);
    }

    private AudioClip GetProphecyClip()
    {
        var magazine = GameManager.Instance.Round.Gun.Magazine();
        var bullet = magazine[_selectedBullet];

        if(magazine.Length <= 2)
        {
            return m_noAnswer;
        }

        return bullet ? m_liveSound[_selectedBullet] : m_blankSound[_selectedBullet];
    }

    private void PlayProphecy()
    {
        m_audioSource.clip = GetProphecyClip();
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
