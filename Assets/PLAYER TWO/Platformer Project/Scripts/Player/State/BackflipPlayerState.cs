using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class BackflipPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
            player.WallDrag(other);
            player.GrabPole(other);

        }

        protected override void OnEnter(Player player)
        {
            // 设置玩家可用的跳跃次数为1（避免后空翻过程中还能多次跳跃）
            player.SetJumps(1);

            // 触发玩家的跳跃事件（用于播放动画，音效等）
            player.playerEvents.Onjump.Invoke();

            if (player.stats.current.backflipLockMovement)
            {
                player.inputs.LockMovementDirection();
            }
        }

        protected override void OnExit(Player player)
        {
        }

        protected override void OnStep(Player player)
        {
            // 应用自定义的重力
            player.Gravity(player.stats.current.backflipGravity);

            // 处理后空翻中的速度变化
            player.BackflipAcceleration();

            // 如果已经落地
            if (player.isGrounded)
            {
                player.LateralVelocity = Vector3.zero;

                player.states.Change<IdlePlayerState>();
            }
            else if(player.VerticalVelocity.y < 0) // 如果还在空中并且已经开始下落
            {
                player.Spin();        // 旋转攻击
                player.StompAttack(); // 踩踏攻击
                player.AirDive();     // 空中俯冲攻击
                player.Glide();       // 滑翔
            }
        }
    }
}
