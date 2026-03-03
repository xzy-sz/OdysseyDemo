using Assets.PLAYER_TWO.Platformer_Project.Scripts.Levels;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    [RequireComponent(typeof(Player))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Audio")]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("Voices")] // 玩家语音类音效
        public AudioClip[] jump;        // 跳跃音效
        public AudioClip[] hurt;        // 受伤音效
        public AudioClip[] attack;      // 攻击音效
        public AudioClip[] lift;        // 举起物品，攀爬音效
        public AudioClip[] maneuver;    // 高级动作（比如后空翻）的音效

        [Header("Effects")] // 玩家动作相关效果音
        public AudioClip spin;              // 旋转攻击音效
        public AudioClip pickUp;            // 捡起物品音效
        public AudioClip drop;              // 丢下物品音效
        public AudioClip airDive;           // 空中俯冲音效
        public AudioClip stompSpin;         // 踩踏开始旋转音效
        public AudioClip stompLanding;      // 踩踏落地音效
        public AudioClip ledgeGrabbing;     // 抓住墙边音效
        public AudioClip dash;              // 冲刺音效
        public AudioClip startRailGrind;    // 开始滑轨音效
        public AudioClip railGrind;         // 滑轨过程音效

        [Header("Other Sources")]
        public AudioSource grindAudio;      // 独立的音效，用于播放滑轨音效

        protected Player m_player;          // 玩家引用
        protected AudioSource m_audio;      // 音源组件，用于播放音效


        protected virtual void Start()
        {
            InitializeAudio();      // 确保有 AudioSource
            InitializePlayer();     // 获取 Player 引用
            InitializeCallbacks();  // 绑定事件与音效
        }

        /// <summary>
        /// 从一组音中随选择并播放
        /// </summary>
        /// <param name="clips"></param>
        protected virtual void PlayRandom(AudioClip[] clips)
        {
            if(clips != null && clips.Length > 0)
            {
                var index = Random.Range(0, clips.Length);
                if (clips[index]) Play(clips[index]);
            }
        }

        protected virtual void Play(AudioClip audio, bool stopPrevious = true)
        {
            if(audio == null) return;

            if(stopPrevious) m_audio.Stop();

            m_audio.PlayOneShot(audio); // 播放一次音效
        }

        /// <summary>
        /// 绑定玩家事件和音效回调
        /// </summary>
        protected virtual void InitializeCallbacks()
        {   
            m_player.playerEvents.Onjump.AddListener(() => PlayRandom(jump));       // 跳跃
            m_player.playerEvents.OnHurt.AddListener(() => PlayRandom(hurt));       // 受伤
            m_player.playerEvents.OnThrow.AddListener(() => Play(drop,false));      // 丢东西(不打断)
            m_player.playerEvents.OnStompStarted.AddListener(()=> Play(stompSpin,false));   // 踩踏开始
            m_player.playerEvents.OnStompLanding.AddListener(()=> Play(stompLanding));      // 踩踏落地
            m_player.playerEvents.OnLedgeGrabbed.AddListener(()=> Play(ledgeGrabbing,false));   // 抓住边缘
            m_player.playerEvents.OnLedgeClimbing.AddListener(()=> PlayRandom(lift));      // 爬上边缘
            m_player.playerEvents.OnBackflip.AddListener(() => PlayRandom(maneuver));      // 后空翻
            m_player.playerEvents.OnDashStarted.AddListener(() => Play(dash));      // 冲刺开始
            m_player.entityEvents.OnRailsExit.AddListener(()=> grindAudio?.Stop()); // 离开滑轨

            // 拾取物品
            m_player.playerEvents.OnPickUp.AddListener(() => 
            { 
                PlayRandom(lift);
                m_audio.PlayOneShot(pickUp); 
            });
            // 旋转攻击
            m_player.playerEvents.OnSpin.AddListener(() =>
            {
                PlayRandom(attack);
                m_audio.PlayOneShot(spin);
            });
            // 空中俯冲
            m_player.playerEvents.OnAirDive.AddListener(() =>
            {
                PlayRandom(attack);
                m_audio.PlayOneShot(airDive);
            });
            // 进入滑轨
            m_player.entityEvents.OnRailsEneter.AddListener(() =>
            {
                Play(startRailGrind, false);
                grindAudio?.Play();
            });

            // 游戏暂停时，暂停玩家音频
            LevelPauser.Instance?.OnPause.AddListener(() =>
            {
                m_audio.Pause();
                grindAudio.Pause();
            });
            // 游戏恢复时，恢复音频
            LevelPauser.Instance?.OnUnpause.AddListener(() =>
            {
                m_audio.UnPause();
                grindAudio?.UnPause();
            });

        }

        /// <summary>
        /// 初始化玩家引用
        /// </summary>
        protected virtual void InitializePlayer() => m_player = GetComponent<Player>();

        /// <summary>
        /// 初始化音频组件，如果物件没有 AudioSource 就自动添加一个
        /// </summary>
        protected virtual void InitializeAudio()
        {
            if(!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }
        }
    }
}
