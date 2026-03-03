using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    /// <summary>
    /// 玩家旋转状态
    /// - 玩家在空中进行旋转动作
    /// - 可在旋转期间进行加速，空中冲刺或踩踏攻击
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Spin Player State")]
    public class SpinPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
        }

        protected override void OnEnter(Player player)
        {
            if(!player.isGrounded)
            {
                // 设置玩家向上的速度
                player.VerticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
            }
        }

        protected override void OnExit(Player player)
        {
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();           // 应用重力
            player.SnapToGround();      // 修正玩家贴地位置
            player.AirDive();           // 空中冲刺
            player.StompAttack();       // 空中踩踏攻击
            player.AccelerateToInputDirection(); // 根据输入方向加速移动
            
            // 如果旋转四化建超过预定持续时间
            if(TimeSinceEntered > player.stats.current.spinDuration)
            {
                if(player.isGrounded)
                {
                    // 着地 -> 空闲状态
                    player.states.Change<IdlePlayerState>();
                }
                else
                {
                    // 空中 -> 下落状态
                    player.states.Change<FallPlayerState>();
                }
            }
        }
    }
}
