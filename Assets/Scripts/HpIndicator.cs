using System;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class HpIndicator : MonoBehaviour
{
    [SerializeField]
    private Renderer[] m_leds;

    [SerializeField]
    private Material m_onMaterial;
    
    private Material _offMaterial;

    private Inventory _inventory;
    
    public void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }

    public void Start()
    {
        _offMaterial = m_leds[0].material;
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
    }
    
    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State == GameState.IN_PROGRESS && !_inventory.Chair.IsFree)
        {
            _inventory.Chair.Player.HealthChanged += Player_OnHealthChanged;
            UpdateLeds(_inventory.Chair.Player.Health);
        }
    }

    private void Player_OnHealthChanged(object sender, Player.HealthChangedArgs e)
    {
        UpdateLeds(e.Health);
    }

    private void UpdateLeds(int hp)
    {
        foreach (var led in m_leds)
        {
            led.material = _offMaterial;
        }

        for (var i = 0; i < Math.Min(m_leds.Length, hp); i++)
        {
            m_leds[i].material = m_onMaterial;
        }
    }
}
