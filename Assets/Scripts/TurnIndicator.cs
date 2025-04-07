using System;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class TurnIndicator : MonoBehaviour
{
    [SerializeField]
    private Material m_onMaterial;

    [SerializeField]
    private Renderer m_led;
    
    private Material _offMaterial;
    
    private GameChair _chair;
    private Inventory _inventory;
    
    public void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }
    
    public void Start()
    {
        _chair = _inventory.Chair;
        _offMaterial = m_led.material;
    }
    
    // TODO - Convert to events        
    private void Update()
    {
        if (_chair.IsFree || !_chair.Player)
        {
            m_led.material = _offMaterial;
            return;
        }

        var isPlayerTurn = GameManager.Instance.Turn.IsPlayerTurn(_chair.Player.PlayerId);
        m_led.material = isPlayerTurn ? m_onMaterial : _offMaterial;
    }
}
