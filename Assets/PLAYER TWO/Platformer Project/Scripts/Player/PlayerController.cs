using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Controller")]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// 增加玩家1点生命
        /// </summary>
        /// <param name="player"></param>
        public void AddHealth(Player player) => AddHealth(player, 1);

        /// <summary>
        /// 增加玩家指定数值的生命
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        public void AddHealth(Player player, int amount) => player.health.Increase(amount);
    }
}
