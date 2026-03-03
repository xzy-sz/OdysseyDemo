using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    /// <summary>
    /// 玩家受伤状态
    /// - 当玩家受伤时进入该状态
    /// - 在空中或地面受到冲击时都会触发此状态
    /// - 根据血量判断是否继续游戏或切换到死亡状态
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Hurt Player State")]
    public class HurtPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
        }

        protected override void OnEnter(Player player)
        {
        }

        protected override void OnExit(Player player)
        {
        }

        protected override void OnStep(Player player)
        {
            // 应用重力，使受伤玩家自然下落
            player.Gravity();

            if(player.isGrounded && (player.VerticalVelocity.y <= 0))
            {
                if(player.health.current > 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
                else
                {
                   player.states.Change<DiePlayerState>();
                }
            }
        }
    }
}
