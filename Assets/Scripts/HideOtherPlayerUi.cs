using UnityEngine;

[RequireComponent(typeof(GameChair))]
public class HideOtherPlayerUi : MonoBehaviour
{
    private GameChair _chair;
    
    [SerializeField]
    private GameObject m_menu;

    private void Awake()
    {
        _chair = GetComponent<GameChair>();
    }

    private void Start()
    {
        GameManager.Instance.GameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(object sender, GameManager.GameStateChangedArgs e)
    {
        if (e.State is GameState.PREPARE or GameState.FINISHED)
        {
            m_menu.SetActive(false);
        }
 
        if (e.State == GameState.IN_PROGRESS)
        {
            m_menu.SetActive(_chair.Player?.IsCurrentPlayer() ?? false);
        }
    }
}
