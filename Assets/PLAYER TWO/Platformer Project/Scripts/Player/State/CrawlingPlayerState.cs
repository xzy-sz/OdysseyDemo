using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class CrawlingPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            
        }

        /// <summary>
        /// 进入爬行状态时调用
        /// - 调整角色碰撞器高度为 “ 蹲伏高度”
        /// </summary>
        /// <param name="player"></param>
        protected override void OnEnter(Player player)
        {
            player.ResizeCollider(player.stats.current.crouchHeight);
        }

        /// <summary>
        /// 离开爬行状态时调用
        /// - 恢复角色碰撞体高度为原始站立高度
        /// </summary>
        /// <param name="player"></param>
        protected override void OnExit(Player player)
        {
            player.ResizeCollider(player.originalHeight);
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();
            player.SnapToGround();
            player.Jump();
            player.Fall();

            // 获取输入的移动方向（相对世界，不考虑相机）
            var inputDirection = player.inputs.GetMovementCameraDirection();

            // 如果玩家任然按下“下蹲”键，或因为·障碍物不能站起来
            if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
            {
                // 如果方向输入 -> 爬行移动
                if(inputDirection.sqrMagnitude > 0)
                {
                    player.CrawlingAccelerate(inputDirection);

                    // 平滑转向，使角色朝向当前移动方向
                    player.FaceDirectionSmooth(player.LateralVelocity);
                }
                else
                {
                    player.Decelerate(player.stats.current.crawlingFriction);
                }
            }
            else
            {
                player.states.Change<IdlePlayerState>();
            }
        }
    }
}
