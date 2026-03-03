using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    /// <summary>
    /// 玩家粒子效果控制器
    /// - 管理玩家在不同状态下（行走，落地，冲刺，滑轨等）的粒子特效播放
    /// - 监听 Player 和 Entity 的事件来触发对应的粒子特效
    /// </summary>
    [RequireComponent(typeof(Player))]  // 要求必须挂载 Player 组件
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Particles")]
    public class PlayerParticles : MonoBehaviour
    {
        [Header("速度阈值设置")]
        public float walkDustMinSpeed = 3.5f;       // 行走扬尘效果的最小速度阈值
        public float landingParticleMinSpeed = 5f;  // 触发落地粒子特效的最小纵向速度阈值

        [Header("粒子特效引用")]
        public ParticleSystem walkDust;     // 行走扬尘
        public ParticleSystem landDust;     // 落地尘土
        public ParticleSystem hurtDust;     // 受伤尘土
        public ParticleSystem dashDust;     // 冲刺初始特效
        public ParticleSystem speedTrails;  // 冲刺速度残影
        public ParticleSystem grindTrails;  // 滑轨火花特效

        protected Player m_player;  // 当前绑定的 Player 引用

        /// <summary>
        /// 初始化，绑定玩家事件回调
        /// </summary>
        protected virtual void Start()
        {
            m_player = GetComponent<Player>();

            // 绑定落地事件 -> 播放落地尘土
            m_player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
            // 绑定受伤事件 -> 播放受伤粒子
            m_player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
            // 绑定冲刺开始事件 -> 播放冲刺特效
            m_player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
            // 绑定冲结束事件 -> 停止速度残影并清理
            m_player.playerEvents.OnDashStarted.AddListener(() => Stop(speedTrails, true));
        }

        /// <summary>
        /// 每帧跟新 （LateUpdate 后）
        /// - 检查并处理行走尘土与滑轨火花粒子
        /// </summary>
        protected virtual void Update()
        {
            HandleWalkParticle();
            HandleRailParticle();
        }

        /// <summary>
        /// 处理行走时的尘土粒子
        /// - 条件：必须在地面，非铁轨，非水面
        /// - 当水平速度大于阈值时触发，否则停止
        /// </summary>
        protected virtual void HandleWalkParticle()
        {
            if(m_player.isGrounded && !m_player.onRails && !m_player.onWater)
            {
                if(m_player.LateralVelocity.magnitude > walkDustMinSpeed)
                {
                    Play(walkDust);
                }
                else
                {
                    Stop(walkDust);
                }
            }
            else
            {
                Stop(walkDust);
            }
        }

        protected virtual void HandleRailParticle()
        {
            if (m_player.onRails)
            {
                Play(grindTrails);
            }
            else
            {
                Stop(grindTrails,true);
            }
        }

        /// <summary>
        /// 播放指定的粒子特效
        ///  - 如果当前粒子未播放则调用 Play()，
        /// </summary>
        /// <param name="particle">粒子特效</param>
        public virtual void Play(ParticleSystem particle)
        {
            if (!particle.isPlaying)
            {
                particle.Play();
            }
        }

        /// <summary>
        ///  处理落地粒子特效
        ///  - 条件：非水面落地，且纵向速度大于阈值
        /// </summary>
        protected virtual void HandleLandParticle()
        {
            if(!m_player.onWater && Mathf.Abs(m_player.Velocity.y) >= landingParticleMinSpeed)
            {
                Play(landDust);
            }
        }

        /// <summary>
        /// 处理受伤粒子特效(如果被攻击击中时触发)
        /// </summary>
        protected virtual void HandleHurtParticle() => Play(hurtDust);

        /// <summary>
        /// 冲刺开始时触发的特效
        /// - 播放冲刺爆发和速度残影
        /// </summary>
        protected virtual void OnDashStarted()
        {
            Play(dashDust);
            Play(speedTrails);
        }

        public virtual void Stop(ParticleSystem particle, bool clear = false)
        {
            if (particle.isPlaying)
            {
                // 根据 clear 参数决定是只停止发射还是清空已存在的粒子
                var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear : 
                                   ParticleSystemStopBehavior.StopEmitting;
                particle.Stop(true, mode);
            }
        }
    }
}
