using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    // 碰撞半径，用于计算玩家与杆子的距离
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/State/Pole Climbing Player State")]
    public class PoleClimbingPlayerState : PlayerState
    {
        // 碰撞半径，用于计算玩家与杆子的距离
        protected float m_collisionRadius;

        // 玩家与杆子之间的微小偏移，避免穿透
        protected const float k_poleOffset = 0.01f;

        public override void OnContact(Player player, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            player.ResetJumps();     // 重置跳跃次数
            player.ResetAirDash();   // 重置空中冲刺次数
            player.ResetAirSpins();  // 重置空中旋转次数
            player.Velocity = Vector3.zero;

            // 获取玩家到杆子方向并计算碰撞半径
            player.pole.GetDirectionToPole(player.transform, out m_collisionRadius);

            // 调整玩家皮肤偏移以贴合杆子
            player.skin.position += player.transform.rotation * player.stats.current.poleClimbSkinOffset;
        }

        protected override void OnExit(Player player)
        {
            player.skin.position -= player.transform.rotation * player.stats.current.poleClimbSkinOffset;
        }

        protected override void OnStep(Player player)
        {
            // 获取玩家到杆子的方向
            var poleDirection = player.pole.GetDirectionToPole(player.transform);
            var inputDirection = player.inputs.GetMovementDirection();

            // 玩家朝向杆子方向
            player.FaceDirection(poleDirection);

            // 左右旋转（绕杆旋转）
            player.LateralVelocity = player.transform.right * inputDirection.x *
                                     player.stats.current.climbRotationSpeed;

            // 上下移动
            if(inputDirection.z != 0)
            {
                var speed = inputDirection.z > 0 ? 
                            player.stats.current.climbUpSpeed : -player.stats.current.climbDownSpeed;
                player.VerticalVelocity = Vector3.up * speed;
            }
            else
            {
                player.VerticalVelocity = Vector3.zero;
            }

            // 玩家跳离杆子
            if (player.inputs.GetJumpDown())
            {
                player.FaceDirection(-poleDirection); // 面向相反方向
                player.DirectionalJump(
                    -poleDirection,
                    player.stats.current.poleJumpHeight,
                    player.stats.current.poleJumpDistance);
                player.states.Change<FallPlayerState>();
            }

            // 玩家到地面，切换到空闲状态
            if(player.isGrounded)
            {
                player.states.Change<IdlePlayerState>();
            }

            // 计算玩家贴杆位置
            var offset = player.height * 0.5f + player.center.y;
            var center = new Vector3(player.pole.center.x, player.transform.position.y, player.pole.center.z);
            var posotion = center - poleDirection * m_collisionRadius;

            // 将玩家位置约束在杆子高度范围内
            player.transform.position = player.pole.ClampPointToPoleHeight(posotion,offset);
        }
    }
}
