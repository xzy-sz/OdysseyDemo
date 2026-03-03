using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class IdlePlayerState : PlayerState
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
            player.Gravity();
            player.SnapToGround();
            player.Jump();
            player.Fall();
            player.Spin();          // 旋转攻击
            player.PickAndThrow();  // 空中拾取或投掷物体
            player.RegularSlopeFactor();    // 处理坡面影响
            player.Friction();      // 应用摩擦力

            var inputDirection = player.inputs.GetMovementDirection();
            if (inputDirection.sqrMagnitude > 0 || player.LateralVelocity.sqrMagnitude > 0)
            {
                player.states.Change<WalkPlayerState>();
            }
            // 如果按下下蹲/爬行 -> 切换到 Crouch 状态
            else if (player.inputs.GetCrouchAndCraw())
            {
                player.states.Change<CrouchPlayerState>();
            }
        }
    }
}
