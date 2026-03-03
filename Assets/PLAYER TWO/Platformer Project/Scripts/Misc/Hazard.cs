using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity.Interfaccs;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    /// <summary>
    /// 实现IEntutyContac 接口，表示该物体能与实体交互(通常用于伤害判定)
    /// </summary>
    // 要求该组件依赖 Collider 组件（没有会自动添加）
    [RequireComponent(typeof(Collider))]
    // 在Unity编辑器菜单中的组件路径
    [AddComponentMenu("Player TWO/Platformer Project/Misc/Hazard")]
    public class Hazard : MonoBehaviour, IEntityContact
    {
        // 是否是实心的（true = 物理阻挡，false = 仅触发伤害）
        public bool isSolid;

        // 是否只能从“上方”对玩家造成伤害
        public bool damageOnlyFromAbove;

        // 每次碰撞对玩家造成的伤害值
        public int damage = 1;

        // 当前物体的 Collider
        protected Collider m_collider;

        public virtual void Awake()
        {
            // 设置标签为 Hazard (统一标识为陷阱类物体)
            tag = GameTags.Hazard;

            // 缓存 Collider
            m_collider = GetComponent<Collider>();

            // 如果不是实心，则设为 Trigger (不会物理阻挡，只会触发)
            m_collider.isTrigger = !isSolid;
        }

        /// <summary>
        /// 尝试对玩家造成伤害
        /// </summary>
        /// <param name="player">接触到的玩家对象</param>
        protected virtual void TryToApplyDamageTo(Player player)
        {
            // 如果设置了“只从上方受伤”
            // 那么需要满足两个条件之一：
            // 1：玩家竖直速度 <= 0 (说明玩家是从上往下落)
            // 2：玩家检测到在Collider 的上边界下方。
            if (!damageOnlyFromAbove || (player.Velocity.y <= 0 && player.IsPointUnderStep(m_collider.bounds.max)))
            {
                player.ApplyDamage(damage, transform.position);
            }
        }

        /// <summary>
        /// IEntityContact 接口实现，当实体接触到该物体时触发
        /// </summary>
        /// <param name="entity"></param>
        public void OnEntityContact(EntityBase entity)
        {
            // 只对 Player 生效
            if(entity is Player player)
            {
                TryToApplyDamageTo(player);
            }
        }

        /// <summary>
        /// 当前 Collider 停留在触发区域中时执行
        /// 这里主要用于非实心陷阱 （Trigger）.
        /// 比如毒雾，火焰区等持续性伤害类效果
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                // 获取 Player 组件
                if(other.TryGetComponent<Player>(out var player))
                {
                    // 尝试对玩家施加伤害
                    TryToApplyDamageTo(player);
                }
            }
        }
    }
}
