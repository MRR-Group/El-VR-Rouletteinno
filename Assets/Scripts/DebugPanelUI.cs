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
        GameManager.Instance.game.Win += Game_OnWinChanged;
        GameManager.Instance.turn.TurnChanged += Turn_OnTurnChanged;
        GameManager.Instance.round.gun.AmmoChanged += Gun_OnAmmoChanged;
        GameManager.Instance.round.RoundStared += Gun_OnAmmoChanged;

        NetworkManager.Singleton.OnConnectionEvent += (_, __) => PlayerManager.Instance.Client().HealthChanged += Player_OnHealthChanged;
    }

    private void Gun_OnAmmoChanged(object sender, EventArgs e)
    {
        m_bullets.text = m_gun.Magazine().Select(value => value ? "1" : "0").ToArray().ToString("");    
    }

    private void Player_OnHealthChanged(object sender, Player.HealthChangedArgs e)
    {
        m_hp.text = e.Health.ToString();
    }

    private void Turn_OnTurnChanged(object sender, EventArgs e)
    {
        m_isMyTurn.text = GameManager.Instance.turn.IsClientTurn() ? "true" : "false";
    }

    private void Game_OnWinChanged(object sender, EventArgs e)
    {
        m_wins.text = GameManager.Instance.game.GetPlayerWins(NetworkManager.Singleton.LocalClientId).ToString();
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        m_gameState.text = e.State.ToString();
        m_isMyTurn.text = GameManager.Instance.turn.IsClientTurn() ? "true" : "false";
    }

    public void ShootEnemy()
    {
        var target = GameManager.Instance.game.players.Value.Find((id) => id != NetworkManager.Singleton.LocalClientId);
        m_gun.Use(target);
    }
    
    public void ShootSelf()
    {
        m_gun.Use(NetworkManager.Singleton.LocalClientId);
    }
}
