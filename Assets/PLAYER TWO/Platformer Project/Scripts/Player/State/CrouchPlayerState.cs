using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class CrouchPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
        }

        /// <summary>
        /// 调整碰撞体高度为蹲伏高度
        /// </summary>
        /// <param name="player"></param>
        protected override void OnEnter(Player player)
        {
            player.ResizeCollider(player.stats.current.crouchHeight);
        }

        /// <summary>
        /// 调整碰撞体高度为原始高度
        /// </summary>
        /// <param name="player"></param>
        protected override void OnExit(Player player)
        {
            player.ResizeCollider(player.originalHeight);
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();           // 应用重力
            player.SnapToGround();      // 吸附地面
            player.Fall();              // 检查下落
            player.Decelerate(player.stats.current.crouchFriction); // 应用下蹲摩擦力。逐渐减速

            // 获取输入的移动方向（相对世界，不考虑相机）
            var inputDirection = player.inputs.GetMovementDirection();

            // 如果玩家任然按下“下蹲”键，或因为·障碍物不能站起来
            if(player.inputs.GetCrouchAndCraw() || !player.canStandUp)
            {
                // 1. 玩家有方向输入，并且角色手上没拿东西
                if(inputDirection.sqrMagnitude > 0 && !player.holding)
                {
                    // 如果速度为0 -> 进入爬行状态
                    if(player.LateralVelocity.sqrMagnitude == 0)
                    {
                        player.states.Change<CrawlingPlayerState>();
                    }
                }
                else if (player.inputs.GetJumpDown())
                {
                   player.Backflip(player.stats.current.backflipBackwardForce);
                }
            }
            else
            {
                player.states.Change<IdlePlayerState>();
            }
        }
    }
}
