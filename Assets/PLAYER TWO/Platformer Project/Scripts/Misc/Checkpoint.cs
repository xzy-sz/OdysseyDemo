using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Checkpoint")]
    public class Checkpoint : MonoBehaviour
    {
        /// <summary> 玩家重生点的位置和旋转 </summary>
        public Transform respawn;

        /// <summary> 激活检查点时播放的音效</summary>
        public AudioClip clip;

        /// <summary> 当检查点被激活时触发的事件</summary>
        public UnityEvent onActivate;

        /// <summary> 本组件的 Collioder 引用</summary>
        protected Collider m_collider;

        /// <summary> 本组件的 AudioSource 引用</summary>
        protected AudioSource m_audio;

        /// <summary> 判断检查点是否已被激活</summary>
        public bool activated {  get; protected set; }

        protected virtual void Awake()
        {
            // 尝试获取 AudioSource 组件，没有则添加一个
            if(!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }

            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;    // 设置为触发器模式
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // 如果检查点未激活，且碰撞体是玩家
            if(!activated && other.CompareTag(GameTags.Player))
            {
                // 尝试获取玩家组件
                if(other.TryGetComponent<Player>(out var player))
                {
                    Activate(player);   // 激活检查点
                }
            }
        }

        /// <summary>
        /// 激活检查点，并设置玩家的重生点位置和旋转
        /// </summary>
        /// <param name="player"></param>
        public virtual void Activate(Player player)
        {
            if(!activated)
            {
                activated = true;
                m_audio.PlayOneShot(clip);
                player.SetRespawn(respawn.position,respawn.rotation);   // 设置玩家重生点
                onActivate?.Invoke();
            }
        }
    }
}
