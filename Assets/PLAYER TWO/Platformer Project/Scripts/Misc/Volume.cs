using UnityEngine;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    /// <summary>
    /// Volume（区域体积触发器）
    /// 用于检测物体进入或离开某个区域时触发事件，并可播放对应的音效
    /// </summary>
    [RequireComponent(typeof(Collider))] // 确保物体上有 Collider 组件
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Volume")] // 在 Unity 菜单中显示
    public class Volume : MonoBehaviour
    {
        /// <summary>
        /// 当有物体进入触发区域时调用的事件
        /// </summary>
        public UnityEvent onEnter;

        /// <summary>
        /// 当有物体离开触发区域时调用的事件
        /// </summary>
        public UnityEvent onExit;

        /// <summary>
        /// 进入区域时播放的音效
        /// </summary>
        public AudioClip enterClip;

        /// <summary>
        /// 离开区域时播放的音效
        /// </summary>
        public AudioClip exitClip;

        /// <summary>
        /// 当前物体上的触发器碰撞体
        /// </summary>
        protected Collider m_collider;

        /// <summary>
        /// 用于播放音效的资源
        /// </summary>
        protected AudioSource m_audio;
        
        /// <summary>
        /// 初始化资源
        /// </summary>
        protected virtual void Start()
        {
            InitializeCollider();
            InitializeAudioSource();
        }

        /// <summary>
        /// 初始化Collider ，并设置为触发器(Trigger)
        /// </summary>
        protected virtual void InitializeCollider()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;    // 设置为触发器模式
        }

        protected virtual void InitializeAudioSource()
        {
            if(!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }

            // 设置音源的空间混合（0 = 2D，1 = 3D，这里0.5 表示半2D 半3D）
            m_audio.spatialBlend = 0.5f;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // 检查进入物体的边界点是否完全在本区域内
            if(!m_collider.bounds.Contains(other.bounds.max) || !m_collider.bounds.Contains(other.bounds.min))
            {
                // 播放进入音效
                m_audio.PlayOneShot(enterClip);
                // 触发进入事件
                onEnter?.Invoke();
            }
        }
    }
}
