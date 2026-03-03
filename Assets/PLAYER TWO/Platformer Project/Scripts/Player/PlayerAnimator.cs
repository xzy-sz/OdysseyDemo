using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    // 要求当前对象必须挂载Player组件
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("Player TWO/Platformer Project/Player/Player Animator")]
    public class PlayerAnimator : MonoBehaviour
    {
        /// <summary>
        /// 定义一个强制过渡的类，用于指定某个玩家状态退出时。
        /// 强制跳转到Animator控制器中的某个动画
        /// </summary>
        [System.Serializable]
        public class ForcedTransition 
        {
            [Tooltip("玩家状态机中 'fromStateId' 的状态结束时，强制跳转到某个动画")]
            public int fromStateId;

            [Tooltip("目标动画所在的 Animator 层索引，默认0表示 Base Layer")]
            public int animationLayer;

            [Tooltip("要强制播放的动画状态名")]
            public string toAnimationState;
        }

        [Header("Parameters Names")] // Animator 参数的变量名.
        public string stateName = "State";
        public string lastStateName = "Last State";
        public string lateralSpeedName = "Lateral Speed";
        public string verticalSpeedName = "Vertical Speed";
        public string lateralAnimationSpeedName = "Lateral Animation Speed";
        public string healthName = "Health";
        public string jumpCounterName = "Jump Counter";
        public string isGroundedName = "Is Grounded";
        public string isHoldingName = "Is Holding";
        public string onStateChangedName = "On State Changed";

        [Header("Settings")]
        public float minLateralAnimationSpeed = 0.5f; // 横向动画播放的最小速度，防止太慢
        public List<ForcedTransition> forcedTransitions; // 强制过渡的列表

        // 角色 Animator 组件。
        public Animator animator;

        // Animator 参数的 Hash 值，避免字符串查找开销
        protected int m_stateHash;
        protected int m_lastStateHash;
        protected int m_lateralSpeedHash;
        protected int m_verticalSpeedHash;
        protected int m_lateralAnimationSpeedHash;
        protected int m_healthHash;
        protected int m_jumpCounterHash;
        protected int m_isGroundedHash;
        protected int m_isHoldingHash;
        protected int m_onStateChangeedHash;

        // 强制过渡的映射表（通过状态ID 快速查找）
        protected Dictionary<int, ForcedTransition> m_forcedTransitions;

        // 引用玩家对象
        protected Player m_player;

        /// <summary>
        /// 脚本启动时初始化所有逻辑
        /// </summary>
        protected virtual void Start ()
        {
            InitializePlayer();
            InitializeForcedTransitions();
            InitializeParametersHash();
            InitializeAnimatorTriggers();
        }

        protected virtual void LateUpdate() => HandleAnimatorParameters();

        protected virtual void HandleAnimatorParameters()
        {
            var lateralSpeed = m_player.LateralVelocity.magnitude;
            var verticalSpeed = m_player.VerticalVelocity.y;
            var lateralAnimationspeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);

            animator.SetInteger(m_stateHash, m_player.states.index);
            animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
            animator.SetInteger(m_jumpCounterHash,m_player.jumpCounter);
            animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
            animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
            animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationspeed);
            animator.SetBool(m_isGroundedHash, m_player.isGrounded);
            animator.SetBool(m_isHoldingHash, m_player.holding);
        }

        /// <summary>
        /// 初始化 Player 引用，并监听状态的切换事件。 
        /// </summary>
        protected virtual void InitializePlayer()
        {
            m_player = GetComponent<Player>();
            //当玩家状态发生变化时，执行强制过渡逻辑
            m_player.states.events.onChange.AddListener(HandleForcedTransitions);
        }

        protected virtual void InitializeForcedTransitions()
        {
            m_forcedTransitions = new Dictionary<int, ForcedTransition>();
            foreach(var transiton in forcedTransitions)
            {
                if (!m_forcedTransitions.ContainsKey(transiton.fromStateId))
                {
                    m_forcedTransitions.Add(transiton.fromStateId, transiton);
                }
            }
        }

        protected virtual void InitializeAnimatorTriggers()
        {
            m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangeedHash));
        }

        /// <summary>
        /// 执行强制过渡逻辑
        /// 如果上一个状态匹配强制过渡表，则播放对应的动画。
        /// </summary>
        protected virtual void HandleForcedTransitions()
        {
            var lastStateIndex = m_player.states.lastIndex;
            if(m_forcedTransitions.ContainsKey(lastStateIndex))
            {
                var layer = m_forcedTransitions[lastStateIndex].animationLayer;
                animator.Play(m_forcedTransitions[lastStateIndex].toAnimationState, layer);
            }
        }

        protected virtual void InitializeParametersHash()
        {
            m_stateHash = Animator.StringToHash(stateName);
            m_lastStateHash = Animator.StringToHash(lastStateName);
            m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
            m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
            m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
            m_healthHash = Animator.StringToHash(healthName);
            m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
            m_isGroundedHash = Animator.StringToHash(isGroundedName);
            m_isHoldingHash = Animator.StringToHash(isHoldingName);
            m_onStateChangeedHash = Animator.StringToHash(onStateChangedName);
        }
    }
}
