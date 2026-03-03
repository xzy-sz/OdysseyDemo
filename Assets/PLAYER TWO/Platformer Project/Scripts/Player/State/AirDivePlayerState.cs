using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/AirDive Player State")]
    public class AirDivePlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            if(!player.isGrounded)
            {
                player.WallDrag(other);
                player.GrabPole(other);
            }
        }

        protected override void OnEnter(Player player)
        {
            //进入俯冲，清空垂直速度
            player.VerticalVelocity = Vector3.zero; 
            // 向前施加俯冲速度
            player.LateralVelocity = player.transform.forward * player.stats.current.airDiveForwardForce;
        }

        protected override void OnExit(Player player)
        {
            
        }

        protected override void OnStep(Player player)
        {
            // 应用重力
            player.Gravity();
            // 检查跳跃 （俯冲时也能跳）
            player.Jump();
            // 如果开启坡度因子修正，则根据坡度调整推力
            if (player.stats.current.applyDiveSlopsFactor)
            {
                player.SlopeFactor(
                    player.stats.current.slopeUpwardForce,
                    player.stats.current.slopeDownwardForce);
            }

            // 朝向移动方向
            player.FaceDirection(player.LateralVelocity);

            // 落地时的处理
            if (player.isGrounded)
            {
                // 获取玩家输入的方向(基于摄像机)
                var inputDirection = player.inputs.GetMovementCameraDirection();
                // 转换到局部坐标
                var localInputDirection = player.transform.InverseTransformDirection(inputDirection);
                // 根据横向输入，计算旋转角度
                var rotation = localInputDirection.x * player.stats.current.airDiveRotationSpeed * Time.deltaTime;
                // 将旋转应用到当前水平速度上
                player.LateralVelocity = Quaternion.Euler(0, rotation, 0) * player.LateralVelocity;
                // 如果在斜坡上
                if (player.OnSlopingGround())
                {
                   player.Decelerate(player.stats.current.airDiveSlopsFriction);
                }
                else
                {
                    // 普通地图时使用普通摩擦力减速
                    player.Decelerate(player.stats.current.airDiveFriction);

                    // 如果完全停下(速度为0)
                    if(player.LateralVelocity.sqrMagnitude == 0)
                    {
                        // 给玩家一个向上的反弹力
                        player.VerticalVelocity = Vector3.up * player.stats.current.airDiveGroundLeapHeight;
                        // 切换到坠落状态
                        player.states.Change<FallPlayerState>();
                    }
                }
            }
        }
    }
}
