using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

public class DebugPanelUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_gameState;
    
    [SerializeField]
    private TextMeshProUGUI m_wins;
    
    [SerializeField]
    private TextMeshProUGUI m_isMyTurn;
    
    [SerializeField]
    private TextMeshProUGUI m_hp;
    
    [SerializeField]
    private TextMeshProUGUI m_bullets;

    [SerializeField]
    private Gun m_gun;

    public void Start()
    {
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
        GameManager.Instance.Game.Win += Game_OnWinChanged;
        GameManager.Instance.Round.Gun.AmmoChanged += Round_OnRoundStarted;
        GameManager.Instance.Round.RoundStared += Round_OnRoundStarted;

        NetworkManager.Singleton.OnConnectionEvent += (_, __) => PlayerManager.Instance.Client().HealthChanged += Player_OnHealthChanged;
    }

    public void Update()
    {
        // TODO - Convert to event
        m_isMyTurn.text = GameManager.Instance.Turn.IsClientTurn() ? "true" : "false";
    }

    private void Round_OnRoundStarted(object sender, EventArgs e)
    {
        m_bullets.text = m_gun.Magazine().Select(value => value ? "1" : "0").ToArray().ToString("");  
        m_isMyTurn.text = GameManager.Instance.Turn.IsClientTurn() ? "true" : "false";
    }

    private void Player_OnHealthChanged(object sender, Player.HealthChangedArgs e)
    {
        m_hp.text = e.Health.ToString();
    }
    
    private void Game_OnWinChanged(object sender, EventArgs e)
    {
        m_wins.text = GameManager.Instance.Game.GetPlayerWins(NetworkManager.Singleton.LocalClientId).ToString();
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        m_gameState.text = e.State.ToString();
        m_isMyTurn.text = GameManager.Instance.Turn.IsClientTurn() ? "true" : "false";
    }

    public void ShootEnemy()
    {
        var target = GameManager.Instance.Game.GetRandomPlayer(new [] { NetworkManager.Singleton.LocalClientId });

        if (target)
        {
            m_gun.Use(target);
        }
    }
    
    public void ShootSelf()
    {
        m_gun.Use(PlayerManager.Instance.Client());
    }
}
