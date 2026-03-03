using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class WallDragPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            player.ResetJumps();
            player.ResetAirSpins();
            player.ResetAirDash();

            // 清空当前速度
            player.Velocity = Vector3.zero;

            // 根据上一次碰到的墙面法线调整玩家朝向(水平面方向)
            var direction = player.lastWallNormal;
            direction = new Vector3(direction.x,0,direction.z).normalized;
            player.FaceDirection(direction);

            // 皮肤偏移，避免模型穿墙
            player.skin.position += player.transform.rotation * player.stats.current.wallDragSkinOffset;
        }

        protected override void OnExit(Player player)
        {
            // 恢复皮肤位置
            player.skin.position -= player.transform.rotation * player.stats.current.wallDragSkinOffset;

            // 如果离开墙面且未着地，解除父子关系
            if(!player.isGrounded && player.transform.parent != null)
            {
                player.transform.parent = null;
            }
        }

        protected override void OnStep(Player player)
        {
            // 墙面下滑重力
            player.VerticalVelocity += Vector3.down * player.stats.current.wallDragGravity * Time.deltaTime;

            // 如果已着或不再贴墙 -> 切换到闲置状态
            if(player.isGrounded || !player.CapsuleCast(-player.transform.forward, player.radius))
            {
                player.states.Change<IdlePlayerState>();
            }
            // 玩家按下跳跃键 -> 墙面跳跃
            else if (player.inputs.GetJumpDown())
            {
                // 如果配置锁定移动方向 -> 锁定
                if (player.stats.current.wallJumpLockMovement)
                {
                    player.inputs.LockMovementDirection();
                }

                // 墙面方向跳跃
                player.DirectionalJump(
                    player.transform.forward,
                    player.stats.current.wallJumpHeight,
                    player.stats.current.wallJumpDistance);

                // 跳跃后切换到下落状态
                player.states.Change<FallPlayerState>();
            }
        }
    }
}
