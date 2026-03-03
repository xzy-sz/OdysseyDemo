using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.WayPoints;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(WaypointManager))]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PLAYER TWO/Platformer Projerct/Misc/Moving Platform")]
    public class MovingPlatform : MonoBehaviour
    {
        [Header("移动设置")]
        // 千米移动速度（单位：米。秒）
        public float speed = 3f;

        // 公开只读属性，表示该平台的路径点管理器
        public WaypointManager waypoints { get; protected set; }

        protected virtual void Awake()
        {
            // 给平台打上指定标签
            tag = GameTags.Platform;
            // 获取路径点管理器
            waypoints = GetComponent<WaypointManager>();
        }

        protected virtual void Update()
        {
            // 当前的平台位置
            var position = transform.position;

            // 当前目标路径点的位置
            var target = waypoints.current.position;

            // 平滑移动：以 speed 的速度，逐步靠近目标点
            position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);

            // 更新平台位置
            transform.position = position;

            // 如果平台达到了目标路径点
            if(Vector3.Distance(transform.position,target) == 0)
            {
                // 切换到下一个路径点
                waypoints.Next();
            }
        }
    }
}
