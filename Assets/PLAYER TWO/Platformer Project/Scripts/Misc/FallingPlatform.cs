using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity.Interfaccs;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using System.Collections;
using UnityEngine;
using UnityEngine.WSA;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Falling Platform")]
    public class FallingPlatform : MonoBehaviour,IEntityContact
    {
        // 平台是否会在掉落后自动复位
        public bool autoReset = true;
        // 玩家踩上去后，延迟多久开始掉落
        public float fallDelay = 2f;
        // 平台掉落后，过多久重新复位
        public float resetDelay = 5f;
        // 平台下落的重力速度
        public float fallGravity = 40f;

        [Header("Shake Settings")]
        public bool shake = true;   // 是否在掉落前进行"抖动"效果
        public float speed = 45f;   // 抖动的频率
        public float height = 0.1f; // 抖动的幅度

        public bool activated { get; protected set; }

        public bool falling { get; protected set; }

        protected Collider[] m_overlaps = new Collider[32];

        // 平台自身的碰撞器
        protected Collider m_collider;

        // 平台的初始位置
        protected Vector3 m_initialPosition;

        public void OnEntityContact(EntityBase entity)
        {
            // 只要玩家并且站在平台上方时才触发
            if(entity is Player && entity.IsPointUnderStep(m_collider.bounds.max))
            {
                if (!activated)     // 防止重复触发
                {
                    activated = true;       // 标记已被激活
                    StartCoroutine(Routine());  // 开始掉落计时协程
                }
            }
        }

        protected IEnumerator Routine()
        {
            var timer = fallDelay;

            while(timer >= 0)
            {
                // 在倒计时后半段播放“移动效果”
                if(shake && (timer <= fallDelay / 2f))
                {
                    var shake = Mathf.Sin(Time.time * speed) * height;
                    transform.position = m_initialPosition + Vector3.up * shake;
                }

                timer -= Time.deltaTime;
                yield return null;
            }

            Fall();

            if (autoReset)
            {
                yield return new WaitForSeconds(resetDelay);
                Restart();
            }
        }

        /// <summary>
        /// 让平台掉落
        /// </summary>
        public virtual void Fall()
        {
            falling = true;
            m_collider.isTrigger = true;
        }

        /// <summary>
        /// 复位平台到最初状态
        /// </summary>
        public virtual void Restart()
        {
            activated = falling = false;
            transform.position = m_initialPosition;
            m_collider.isTrigger = false;
            OffsetPlayer();
        }

        protected virtual void OffsetPlayer()
        {
            var center = m_collider.bounds.center;      // 平台中心
            var extents = m_collider.bounds.extents;    // 平台范围
            var maxY = m_collider.bounds.max.y;         // 平台顶部的 y 值

            // 检测平台范围内所有碰撞体
            var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);

            for(int i = 0;i < overlaps; i++)
            {
                if (!m_overlaps[i].CompareTag(GameTags.Player)) continue;

                // 玩家和平台顶部的距离
                var distance = maxY - m_overlaps[i].transform.position.y;
                // 玩家高度
                var height = m_overlaps[i].GetComponent<Player>().height;
                // 计算向上的偏移量
                var offset = Vector3.up * (distance * height * 0.5f);

                // 修正玩家位置
                m_overlaps[i].transform.position += offset;
            }
        }


        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();  
            m_initialPosition = transform.position;
            tag = GameTags.Platform;
        }

        protected virtual void Update()
        {
            if (falling)
            {
                // 模拟重力下落效果
                transform.position += fallGravity * Vector3.down * Time.deltaTime;
            }
        }
    }
}
