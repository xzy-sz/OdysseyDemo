using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    /// <summary>
    /// 重击踩踏攻击
    /// - 玩家在空中蓄力然后快速下落
    /// - 可触发空中计时，下落力，落地事件及落地反弹
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Stomp Player State")]
    public class StompPlayerState : PlayerState
    {
        // 空中计时器,用于计算蓄力时间
        protected float m_airTimer;
        // 落地计时器，哦用于控制落地停留时间
        protected float m_groundTimer;

        // 玩家是否开始下落
        protected bool m_falling;
        // 玩家是否已经落地
        protected bool m_landed;

        public override void OnContact(Player player, Collider other)
        {
            
        }

        protected override void OnEnter(Player player)
        {
            // 每次进入踩踏攻击时初始化状态信息
            m_landed = m_falling = false;
            m_airTimer = m_groundTimer = 0;
            player.Velocity = Vector3.zero;
            player.playerEvents.OnStompStarted?.Invoke();
        }

        protected override void OnExit(Player player)
        {
            player.playerEvents.OnStompEnding?.Invoke();
        }

        protected override void OnStep(Player player)
        {
            // 没有开始下落，正在蓄力
            if(!m_falling)
            {
                // 空中计时
                m_airTimer += Time.deltaTime;

                // 空中蓄力时间结束，开始下落
                if(m_airTimer >= player.stats.current.stompAirTime)
                {
                    m_falling = true;
                    player.playerEvents.OnStompFalling?.Invoke();
                }
            }
            else // 开始下落
            {
                // 下落阶段施加向下力
                player.VerticalVelocity += Vector3.down * player.stats.current.stompDownwardForce;
            }

            // 玩家落地处理
            if(player.isGrounded)
            {
                if(!m_landed) // 表示这一帧是刚接触地面
                {
                    m_landed = true; // 第一次落地触发事件
                    player.playerEvents.OnStompLanding?.Invoke();
                }

                if(m_groundTimer >= player.stats.current.stompGroundTime)
                {
                    // 落地停留时间结束 -> 小跳反弹并切换到下落状态
                    player.VerticalVelocity = Vector3.up * player.stats.current.stompGroundLeapHeight;
                    player.states.Change<FallPlayerState>();
                }
                else
                {
                    // 继续增加落地计时
                    m_groundTimer += Time.deltaTime;
                }
            }
        }
    }
}
