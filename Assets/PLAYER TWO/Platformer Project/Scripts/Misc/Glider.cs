using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    /// <summary>
    /// 滑翔伞
    /// </summary>
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Glider")]
    public class Glider : MonoBehaviour
    {
        // 关联的玩家对象
        public Player player;
        // 用于播放音效的 AudioSource
        protected AudioSource m_audio;

        // 滑翔时的拖尾特效（可以有多个）
        public TrailRenderer[] trails;

        public float scaleDuration = 0.25f;

        [Header("Audio Settings")]
        public AudioClip openAudio; // 打开滑翔翼时播放的音效
        public AudioClip closeAudio; // 关闭滑翔翼时播放的音效

        /// <summary>
        /// Unity 生命周期：开始时初始化
        /// </summary>
        protected virtual void Start()
        {
            InitializePlayer();     // 初始化玩家引用
            InitializeAudio();      // 初始化音效
            InitializeCallbacks();  // 绑定事件
            InitializeGlider();     //初始化滑翔翼（隐藏）
        }

        /// <summary>
        /// 初始化玩家
        /// </summary>
        protected virtual void InitializePlayer()
        {
            if (!player)
            {
                player = GetComponentInParent<Player>();
            }
        }

        /// <summary>
        /// 初始化音效（如果没有 AudioSource,就自动添加一个）
        /// </summary>
        protected virtual void InitializeAudio()
        {
            if(!TryGetComponent(out m_audio)){
                m_audio = gameObject.AddComponent<AudioSource>();
            }
        }

        /// <summary>
        /// 绑定玩家的事件回调
        /// - 开始滑翔 -> 展示滑翔翼
        /// - 停止滑翔 -> 收起滑翔翼
        /// </summary>
        protected virtual void InitializeCallbacks()
        {
            player.playerEvents.OnGlidingStart.AddListener(ShowGlider);
            player.playerEvents.OnGlidingStop.AddListener(HideGlider);
        }

        /// <summary>
        /// 初始化滑翔翼：隐藏(缩放值为0，拖尾关闭)
        /// </summary>
        protected virtual void InitializeGlider()
        {
            SetTrailsEmitting(false);
            transform.localScale = Vector3.zero;
        }

        /// <summary>
        /// 展示滑翔翼（带缩放动画 + 打开拖尾 + 播放音效）
        /// </summary>
        protected virtual void ShowGlider()
        {
            StopAllCoroutines(); // 停止正在运行的缩放动画，避免冲突
            StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));  // 缩放到正常大小
            SetTrailsEmitting(true); // 开启拖尾
            m_audio.PlayOneShot(openAudio); // 播放展开音效 
        }

        /// <summary>
        /// 隐藏滑翔翼
        /// </summary>
        protected virtual void HideGlider()
        {
            StopAllCoroutines();
            StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero)); // 缩放到0
            SetTrailsEmitting(false); // 关闭拖尾
            m_audio.PlayOneShot(closeAudio); // 播放收起音效
        }

        /// <summary>
        /// 设置拖尾特效是否开启
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetTrailsEmitting(bool value)
        {
            if (trails == null) return;

            foreach(var trail in trails)
            {
                trail.emitting = value;
            }
        }

        /// <summary>
        /// 协程：在指定时间内平滑缩放滑翔翼
        /// </summary>
        /// <param name="from">起始缩放</param>
        /// <param name="to">目标缩放</param>
        /// <returns></returns>
        protected IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
        {
            var time = 0f;
            transform.localScale = from; // 初始缩放

            // 在 scaleDuration 时间内逐渐插值到目标缩放
            while (time < scaleDuration)
            {
                var scale = Vector3.Lerp(from, to, time / scaleDuration);
                transform.localScale = scale;
                time += Time.deltaTime;
                yield return null;
            }

            // 最后确保完全缩放到目标值
            transform.localScale = to;
        }
    }
}
