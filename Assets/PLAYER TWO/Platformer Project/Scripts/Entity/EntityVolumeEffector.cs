using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Volume Effector")]
    public class EntityVolumeEffector : MonoBehaviour
    {
        [Header("进入液体中：对实体运动属性的影响倍率")]
        /// <summary> 进入区域时，实体速度的乘法转换因子 </summary>
        public float velocityConversion = 1f;

        /// <summary> 进入区域时，实体加速度的乘法因子</summary>
        public float accelerationMultiplier = 1f;

        /// <summary> 进入区域时，实体最大速度的乘法因子</summary>
        public float topSpeedMultiplier = 1f;

        /// <summary> 进入区域时，实体减速度的乘法因子</summary>
        public float decelerationMultiplier = 1f;

        /// <summary> 进入区域时，实体转向阻力的乘法因子</summary>
        public float turningDragMultiplier = 1f;

        /// <summary> 进入区域时，实体重力影响的乘法因子</summary>
        public float gravityMultiplier = 1f;

        /// <summary>
        /// 缓存的 Collier 组件引用，用于设置触发器属性
        /// </summary>
        protected Collider m_collider;

        /// <summary>
        /// 当其他碰撞体进入触发器时调用
        /// 如果碰撞体挂载了 EntityBase 组件，则根据设定参数调整实体的运动属性
        /// </summary>
        /// <param name="oyher">进入触发器的碰撞体</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            // 尝试获取碰撞体上的 EntityBase 组件
            if(other.TryGetComponent(out EntityBase entity)){
                // 通过乘法因子修改实体当前的速度
                entity.Velocity *= velocityConversion;
                // 设置实体各类运动属性的倍率。影响后续运动行为
                entity.accelerationMultiplier = accelerationMultiplier;
                entity.topSpeedMultiplier = topSpeedMultiplier;
                entity.decelerationMultiplier = decelerationMultiplier;
                entity.turningDragMultiplier = turningDragMultiplier;
                entity.gravityMultiplier = gravityMultiplier;

            }
        }

        /// <summary>
        /// 退出时重置实体运动属性为 默认值1
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerExit(Collider other)
        {
            // 尝试获取碰撞体上的 EntityBase 组件
            if(other.TryGetComponent(out EntityBase entity))
            {
                // 将所有引动属性倍率重置为默认值
                entity.accelerationMultiplier = 1f;
                entity.topSpeedMultiplier = 1f;
                entity.decelerationMultiplier = 1f;
                entity.turningDragMultiplier = 1f;
                entity.gravityMultiplier = 1f;
            }
        }

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            // 将 Collider 设置为触发器，以便检测实体进入和退出事件，而不产生物理碰撞
            m_collider.isTrigger = true;
        }
    }
}
