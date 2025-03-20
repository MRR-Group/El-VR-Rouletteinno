using UnityEngine;

public class DamageAnimation : MonoBehaviour
{
    [SerializeField] 
    private Player m_player;
    
    [SerializeField] 
    private ParticleSystem m_particles;

    public void Start()
    {
        m_player.HealthChanged += Player_OnHealthChanged;
    }
    
    private void Player_OnHealthChanged(object sender, Player.HealthChangedArgs e)
    {
        if (e.Delta > -1)
        {
            return;
        }
            
        m_particles.time = 0;
        m_particles.Play();
    }
}
