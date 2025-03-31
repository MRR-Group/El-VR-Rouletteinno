using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class AmmoIndicator : MonoBehaviour
{
    [SerializeField]
    private Renderer[] m_leds;

    [SerializeField]
    private Material m_blandMaterial;

    [SerializeField]
    Material m_liveMaterial;

    [SerializeField]
    Gun m_gun;

    private int _lives = 0;
    private int _all = 0;

    public void Start()
    {
         m_gun.Reloaded += Gun_OnReloaded;
         m_gun.BulletSkipped += Gun_OnBulletSkipped;
         HideAll();
    }
    
    private void HideAll()
    {
        foreach (var led in m_leds)
        {
            led.gameObject.SetActive(false);
        }
    }

    private void Gun_OnReloaded(object sender, Gun.ReloadedEventArgs e)
    {
        _all = e.Bullets.Length;
        _lives = e.Bullets.Count(bullet => bullet);

        HideAll();
        ShowBullets();
    }

    void Gun_OnBulletSkipped(object sender, Gun.BulletSkippedEventArgs e)
    {
        _all -= 1;

        if (e.Bullet)
        {
            _lives -= 1;
        }

        HideAll();
        ShowBullets();
    }
    
    protected void ShowBullets()
    {
        for (var i = 0; i < _lives; i++)
        {
            m_leds[i].gameObject.SetActive(true);
            m_leds[i].material = m_liveMaterial;
        }
        
        for (var i = _lives; i < _all; i++)
        {
            m_leds[i].gameObject.SetActive(true);
            m_leds[i].material = m_blandMaterial;
        }
    }
}
