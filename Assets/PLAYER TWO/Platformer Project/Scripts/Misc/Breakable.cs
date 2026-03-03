using UnityEngine;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider),typeof(AudioSource))] // 要求挂载此脚本的物体必须带有Collider和AudioSource组件
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Breakable")] // 在 untiy 组件菜单中显示的路径
    public class Breakable : MonoBehaviour
    {
        /// <summary>用于显示的游戏对象（可破坏物体的模型等） </summary>
        public GameObject display;

        /// <summary>破坏时播放的音效</summary>
        public AudioClip clip;

        /// <summary>当物体被破坏时触发的事件</summary>
        public UnityEvent OnBreak;

        // 该物体的碰撞器组件引用
        protected Collider m_collider;

        // 该物体的音频源组件引用
        protected AudioSource m_audio;

        // 该物体的刚体组件引用（如果存在）
        protected Rigidbody m_rigidbody;

        /// <summary>表示该物体是否已经被破坏</summary>
        public bool broken { get; protected set; }

        /// <summary>
        /// 触发破坏动作的方法
        /// </summary>
        public virtual void Break()
        {
            // 如果还未破坏，则进行破坏处理
            if(!broken)
            {
                // 如果有刚体，将其设为运动学，停止物理模拟
                if (m_rigidbody)
                {
                    m_rigidbody.isKinematic = true;
                }
                broken = true;              // 标记为已破坏
                display.SetActive(false);   // 隐藏显示的物体模型
                m_collider.enabled = false; // 禁用碰撞器，使其不再参与碰撞检测
                m_audio.PlayOneShot(clip);  // 播放破坏音效
                OnBreak?.Invoke();          // 触发破坏事件，通知其他系统
            }
        }

        /// <summary>
        /// 初始化组件引用
        /// </summary>
        protected virtual void Start()
        {
            m_audio = GetComponent<AudioSource>();   // 获取 AudioSource 组件
            m_collider = GetComponent<Collider>();   // 获取 Collider 组件
            TryGetComponent(out m_rigidbody);        // 尝试获取 Rigidbody 组件(可能没有)
        }
    }
}
