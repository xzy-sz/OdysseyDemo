using Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity.Interfaccs;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider),typeof(Rigidbody))]
    [AddComponentMenu("PLAYER TWO/Platformewr Project/Mics/Pickable")]
    public class Pickable : MonoBehaviour,IEntityContact
    {
        [Header("General Settings")]
        public Vector3 offset;                  // 物体被拾取后在玩家手中的位置偏移
        public float releaseOffset = 0.5f;      // 物体被释放时向前偏移的距离

        [Header("Respawn Settings")]
        public bool autoRespawn;                // 是否开启自动重生
        public bool respawnOnHitHazards;        // 碰到危险物(Hazard)是否重生
        public float respawnHeightLimit = -100; // 超过某个高度(掉落过深)是否重生

        [Header("Attack Settings")]
        public bool attackEnemies = true;       // 是否可以攻击别人
        public int damage = 1;                  // 对敌人造成的伤害值
        public float minDamageSpeed = 5f;       // 物体速度超过这个阈值时才会造成伤害

        [Space(15)]

        // 当前物体被拾取时触发
        public UnityEvent onPicked;

        // 当前物体被释放时触发
        public UnityEvent onReleased;

        // 当前物体被重生时触发
        public UnityEvent onRespawn;

        protected Collider m_collider;              //  缓存物体的碰撞器
        protected Rigidbody m_rigidBody;            // 缓存物体的刚体  

        protected Vector3 m_initialPosition;        // 初始位置         (用于重生)
        protected Quaternion m_initialRotation;     // 初始旋转
        protected Transform m_initialParent;        // 初始父物体

        protected RigidbodyInterpolation m_interpolation;   //  保存插值模式(被拾取时关闭)

        public bool beingHold { get; protected set; }   // 是否带你给钱正被玩家持有

        /// <summary>
        /// 拾取物体
        /// </summary>
        /// <param name="slot">玩家的拾取槽</param>
         public virtual void PickUp(Transform slot)
        {
            if (!beingHold)
            {
                beingHold = true;                                           // 物体挂到拾取槽下
                transform.parent = slot;                                    // 设置偏移位置
                transform.localPosition = Vector3.zero + offset;            // 重置旋转
                transform.localRotation = Quaternion.identity;              // 关闭物理模拟
                m_rigidBody.isKinematic = true;                             // 设为触发器避免碰撞
                m_collider.isTrigger = true;                                // 保存插值模式
                m_interpolation = m_rigidBody.interpolation;                // 临时关闭插值
                m_rigidBody.interpolation = RigidbodyInterpolation.None;    // 触发拾取事件
                onPicked?.Invoke();
            }
        }

        /// <summary>
        /// 释放物体
        /// </summary>
        /// <param name="direction">释放方向</param>
        /// <param name="force">释放的力度</param>
        public virtual void Release(Vector3 direction, float force)
        {
            transform.parent = null;                                                // 脱离玩家父物体
            transform.position += direction * releaseOffset;                        // 稍微往释放方向偏移
            m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false;     // 恢复物理
            m_rigidBody.interpolation = m_interpolation;                            // 恢复插值模式
            m_rigidBody.velocity = direction * force;                               // 赋予速度
            onReleased?.Invoke();                                                   // 触发释放事件
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_rigidBody = GetComponent<Rigidbody>();
            m_initialPosition = transform.localPosition;    // 记录初始本地坐标
            m_initialRotation = transform.localRotation;     // 记录初始本地旋转
            m_initialParent = transform.parent;             // 记录初始父对象
        }

        protected virtual void Update()
        {
            if(autoRespawn && transform.position.y <= respawnHeightLimit)
            {
                Respawn();
            }
        }

        /// <summary>
        /// 碰到触发器时调用
        /// </summary>
        protected virtual void OnTriggerEnter(Collider other) => EvluateHazardRespawn(other);

        /// <summary>
        /// 碰到碰撞体时调用
        /// </summary>
        protected virtual void OnCollisionEnter(Collision collision) => EvluateHazardRespawn(collision.collider);

        /// <summary>
        //.检查是否需要因为碰到危险物体而重生
        /// </summary>
        /// <param name="other"></param>
        protected virtual void EvluateHazardRespawn(Collider other)
        {
            if(autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
            {
                Respawn();
            }
        }

        public virtual void Respawn()
        {
            m_rigidBody.velocity = Vector3.zero;                                         // 清除速度
            transform.parent = m_initialParent;                                          // 恢复初始父物体
            transform.SetLocalPositionAndRotation(m_initialPosition, m_initialRotation); // 恢复位置和旋转
            m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;          // 恢复物理属性
            onRespawn?.Invoke();                                                         // 触发重生事件
        }

        /// <summary>
        /// 当物体与实体接触时触发
        /// </summary>
        /// <param name="entity"></param>
        public void OnEntityContact(EntityBase entity)
        {
            // 如果允许攻击敌人，并且接触对象是敌人，且物体速度大于阈值 -> 造成伤害
            if(attackEnemies && entity is Enemy && m_rigidBody.velocity.magnitude > minDamageSpeed)
            {
                entity.ApplyDamage(damage,transform.position);
            }
        }
    }
}
