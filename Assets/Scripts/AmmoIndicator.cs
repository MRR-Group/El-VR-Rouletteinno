using System;
using System.Linq;
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
    
    public void Start()
    {
         GameManager.Instance.Round.RoundStared += Round_OnRoundStared;
         HideAll();
    }

    private void HideAll()
    {
        foreach (var led in m_leds)
        {
            led.gameObject.SetActive(false);
        }
    }

    private void Round_OnRoundStared(object sender, EventArgs e)
    {
        HideAll();

        var lives = m_gun.Magazine().Count(bullet => bullet);

        for (var i = 0; i < lives; i++)
        {
            m_leds[i].gameObject.SetActive(true);
            m_leds[i].material = m_liveMaterial;
        }
        
        for (var i = lives; i < m_gun.Magazine().Length; i++)
        {
            m_leds[i].gameObject.SetActive(true);
            m_leds[i].material = m_blandMaterial;
        }
    }
}
