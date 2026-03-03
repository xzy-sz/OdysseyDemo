using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class BrakePlayerState : PlayerState
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

        /// <summary>
        /// 每帧更新调用（刹车逻辑）
        /// </summary>
        /// <param name="player"></param>
        protected override void OnStep(Player player)
        {
            // 获取玩家输入方向
            var inputDirection = player.inputs.GetMovementCameraDirection();

            // 执行后空翻动作
            if( player.stats.current.canBackflip &&
                Vector3.Dot(inputDirection, player.transform.forward) < 0 &&
                player.inputs.GetJumpDown())
            {
                player.Backflip(player.stats.current.backflipBackwardTurnForce);
            }
            else
            {
                // 贴地
                player.SnapToGround();
                // 跳跃
                player.Jump();
                // 下落
                player.Fall();
                // 减速
                player.Decelerate();

                // 如果玩家水平速度为0
                if (player.LateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
            }         
        }
    }
}
