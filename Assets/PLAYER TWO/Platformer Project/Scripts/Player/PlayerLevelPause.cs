using Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Level Pause")]
    public class PlayerLevelPause : MonoBehaviour
    {
        protected Player m_player;
        protected LevelPauser m_pauser;

        protected virtual void Start()
        {
            m_player = GetComponent<Player>();
            m_pauser = LevelPauser.Instance;
        }

        protected virtual void Update()
        {
            if (m_player.inputs.GetPauseDown())
            {
                var value = m_pauser.paused;
                m_pauser.Pause(!value);
            }
        }
    }
}
