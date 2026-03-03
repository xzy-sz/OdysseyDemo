using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    /// <summary>
    /// 玩家冲刺状态
    /// - F 键控制
    /// </summary>
    public class DashPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            
            player.WallDrag(other);     // 墙面减速
            player.GrabPole(other);     // 抓杆
            player.PushRigidbody(other);  

        }

        protected override void OnEnter(Player player)
        {
            player.VerticalVelocity = Vector3.zero; // 冲刺状态下垂直速度为0（不会再冲刺时下落）
            player.LateralVelocity = player.transform.forward * player.stats.current.dashForce;
            player.playerEvents.OnDashStarted.Invoke(); // 冲刺开始，调用对应事件
        }

        protected override void OnExit(Player player)
        {
            // 限制水平速度在最大速度范围
            player.LateralVelocity = Vector3.ClampMagnitude(player.LateralVelocity, player.stats.current.topSpeed);
            player.playerEvents.OnDashEnded.Invoke(); // 调用事件 ： 冲刺结束
        }

        protected override void OnStep(Player player)
        {
            player.Jump(); // 冲刺时任然可以跳跃
            
            // 判断是否超过冲刺持续时间
            if(TimeSinceEntered > player.stats.current.dashDuration)
            {
                if (player.isGrounded)
                {
                    player.states.Change<WalkPlayerState>();
                }
                else
                {
                    player.states.Change<FallPlayerState>();
                }
            }
        }
    }
}
