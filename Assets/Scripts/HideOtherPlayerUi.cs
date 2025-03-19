using System;
using UnityEngine;

public class HideOtherPlayerUi : MonoBehaviour
{
    [SerializeField]
    private GameObject m_player;
    
    [SerializeField]
    private GameObject m_menu;
    
    private void Start()
    {
        
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State == GameState.PREPARE)
    }
}
