using System;
using UnityEngine;

<<<<<<< Updated upstream
public class HideOtherPlayerUi : MonoBehaviour
{
    [SerializeField]
    private GameObject m_player;
=======
[RequireComponent(typeof(GameChair))]
public class HideOtherPlayerUi : MonoBehaviour
{
    [SerializeField]
    private GameChair m_player;
>>>>>>> Stashed changes
    
    [SerializeField]
    private GameObject m_menu;
    
    private void Start()
    {
<<<<<<< Updated upstream
        
=======
>>>>>>> Stashed changes
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State == GameState.PREPARE)
    }
}
