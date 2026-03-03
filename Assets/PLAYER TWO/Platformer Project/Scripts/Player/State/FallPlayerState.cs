using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class FallPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);

                player.WallDrag(other);
                player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {
        }

        protected override void OnExit(Player player)
        {
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();       // 应用重力
            player.SnapToGround();  // 吸附地面
            player.FaceDirectionSmooth(player.LateralVelocity); // 平滑转向
            player.AccelerateToInputDirection();                // 根据玩家输入的方向加速
            player.Jump();          // 冲刺处理
            player.Dash();          // 空中冲刺
            player.Spin();          // 旋转攻击
            player.PickAndThrow();  // 空中拾取或投掷物体
            player.StompAttack();   // 空中踩踏攻击
            player.AirDive();       // 空中俯冲攻击
            player.Glide();         // 滑翔
            player.LedgeGrab();     // 抓取悬挂

            if (player.isGrounded)
            {
                player.states.Change<IdlePlayerState>();
            }
        }
    }
}
