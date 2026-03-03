using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity.Interfaccs;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Spring")]
    public class Spring : MonoBehaviour,IEntityContact
    {
        /// <summary>弹簧施加的向上力的大小</summary>
        public float force = 25f;

        /// <summary>弹簧触发时播放的音效</summary>
        public AudioClip clip;

        /// <summary>音效播放器</summary>
        protected AudioSource m_audio;

        /// <summary>碰撞体，用于检测玩家是否接触到弹簧</summary>
        protected Collider m_collider;

        protected virtual void Start()
        {
            // 设置标签为 Spring
            tag = GameTags.Spring;

            // 获取碰撞体
            m_collider = GetComponent<Collider>();

            // 获取音效组件，如果没有就自动添加一个
            if(!TryGetComponent(out m_audio))
            {
                m_audio = gameObject.AddComponent<AudioSource>();
            }
        }

        public void OnEntityContact(EntityBase entity)
        {
            // 检查实体是否从弹簧的上方踩下（碰撞点在碰撞体顶部附近）
            // 并且该实体是 Player 且玩家还活着
            if(entity.IsPointUnderStep(m_collider.bounds.max) && 
                entity is Player player && player.isAlive)
            {
                // 施加弹簧力
                ApplyForce(player);

                // 重置玩家空中状态
                player.SetJumps(1);
                player.ResetAirSpins();
                player.ResetAirDash(); ;

                // 强制切换到“下落状态”
                player.states.Change<FallPlayerState>();
            }
        }

        /// <summary>
        /// 对指定玩家施加弹簧的向上力
        /// </summary>
        /// <param name="player">要施加的玩家对象</param>
        public void ApplyForce(Player player)
        {
            // 仅当玩家竖直速度向下（y <= 0）时才触发
            if(player.VerticalVelocity.y <= 0)
            {
                // 播放弹簧音乐
                m_audio.PlayOneShot(clip);

                // 设置玩家的竖直速度为向上的力
                player.VerticalVelocity = Vector3.up * force;
            }
        }
    }
}
