using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Buoyancy")]
    public class Buoyancy : MonoBehaviour
    {
        /// <summary> 浮力大小 </summary>
        public float force = 10f;

        /// <summary> 物体自身的刚体组件引用 </summary>
        protected Rigidbody m_rigidbody;

        /// <summary> 初始化，获取刚体组件 </summary>
        protected virtual void Start()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// 当物体持续处于触发器区域时调用
        /// </summary>
        /// <param name="other">触发器碰撞体</param>
        protected virtual void OnTriggerStay(Collider other)
        {
            // 判断物体是否部分或完全在水体部分(Y轴位置低于水面)
            if(transform.position.y < other.bounds.max.y)
            {
                // 计算浮力乘数，基于物体沉没的深度，范围为 0-1；
                var multiplier = Mathf.Clamp01(other.bounds.max.y -  transform.position.y);

                // 计算向上的浮力向量
                var buoyancy = Vector3.up * force * multiplier;

                // 作用浮力到刚体上
                m_rigidbody.AddForce(buoyancy);
            }
        }

    }
}
