using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Gravity Field")]
    public class GravityField : MonoBehaviour
    {
        // 施加给玩家的重力场 "力"的大小
        public float force = 75f;

        // 缓存当前物体的 Collider 组件
        protected Collider m_collider;

        protected virtual void OnTriggerStay(Collider other)
        {
            // 确认物体带有 “Player” 标签
            if (other.CompareTag(GameTags.Player))
            {
                // 尝试获取 Player 组件
                if(other.TryGetComponent<Player>(out var player))
                {
                    // 如果玩家处于地面状态下，则清空竖直速度
                    // 这样避免角色因地面检测被蜡烛，确保能被场的力抬起
                    if (player.isGrounded)
                    {
                        player.VerticalVelocity = Vector3.zero;
                    }

                    // 给玩家的速度施加一个"沿着当前物体的 up 方向"的力
                    //Time.deltaTime 确保力是逐帧平滑的
                    player.Velocity += transform.up * force * Time.deltaTime;
                }
            }
        }

        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_collider.isTrigger = true;
        }


    }
}
