using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class WalkPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.PushRigidbody(other);
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
            // 保持贴地
            player.SnapToGround();
            // 跳跃处理
            player.Jump();
            // 下落处理
            player.Fall();
            //冲刺处理
            player.Dash();
            // 旋转攻击
            player.Spin();
            // 空中拾取或投掷物体
            player.PickAndThrow();
            // 常规坡面力处理
            player.RegularSlopeFactor();

            var inputDirection = player.inputs.GetMovementCameraDirection();
            
            if(inputDirection.sqrMagnitude > 0)
            {
                var dot = Vector3.Dot(inputDirection, player.LateralVelocity);

                if(dot >= player.stats.current.brakeThreshold)
                {
                    player.Accelerate(inputDirection);
                    player.FaceDirectionSmooth(player.LateralVelocity);
                }
                else
                {
                    // 低于刹车阈值 -> 进入刹车状态
                    player.states.Change<BrakePlayerState>();
                }
            }
            else
            {
                // 没有输入 -> 使用摩擦力减速
                player.Friction();
                // 当水平速度为0 -> 切换到待机状态
                if(player.LateralVelocity.sqrMagnitude <= 0)
                {
                    player.states.Change<IdlePlayerState>();
                }
            }

            if (player.inputs.GetCrouchAndCraw())
            {
                player.states.Change<CrouchPlayerState>();
            }
        }
    }
}
