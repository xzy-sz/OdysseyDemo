using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.WayPoints;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Enemys/Enemy")]
    public class Enemy : Entity<Enemy>
    {
        // 敌人相关的事件
        public EnemyEvents enemyEvents;

        // 用于存储视野检测的碰撞体缓存
        protected Collider[] m_sightOverlaps = new Collider[1024];

        // 用与缓存接触攻击检测的碰撞体缓存
        protected Collider[] m_contactAttackOverlaps = new Collider[1024];

        /// <summary>
        /// 血量组件实例
        /// </summary>
        public Health health { get; protected set; }

        /// <summary>
        /// 敌人属性管理器
        /// </summary>
        public EnemyStatsManager stats { get; protected set; }

        /// <summary>
        /// 当前被敌人法线的玩家实例
        /// </summary>
        public Player player { get; protected set; }

        public WaypointManager waypoints { get; protected set; }    

        protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>(); 

        protected virtual void InitializeWaypointsManager() => waypoints = GetComponent<WaypointManager>();

        protected virtual void InitializeHealth() => health = GetComponent<Health> ();

        protected virtual void InitializeTag() => tag = GameTags.Enemy;

        protected override void Awake()
        {
            base.Awake();
            InitializeTag();                // 设置标签
            InitializeStatsManager();       // 初始化属性管理器
            InitializeWaypointsManager();   // 初始化路径管理器
            InitializeHealth();
        }

        /// <summary>
        /// 每帧更新逻辑
        /// </summary>
        protected override void OnUpdate()
        {
            HandleSight();      // 检测玩家
            ContactAttack();    // 检测接触攻击
        }

        public virtual void ContactAttack()
        {
            if (stats.current.canAttackOnContact)
            {
                // 检测出指定范围内的实体
                var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);

                for(int i = 0; i < overlaps;i++)
                {
                    // 如果时玩家
                    if (m_contactAttackOverlaps[i].CompareTag(GameTags.Player) &&
                        m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
                    {
                        // 计算脚下位置（防止玩家从上方踩到）
                        // controller.bounds.max:这是敌人的碰撞盒（Collider）的最高点坐标，通常是敌人头顶的位置
                        // Vector3.down * stats.current.contactSteppingTolerance:沿着 Y 轴向下偏移一个很小的距离
                        // 合起来，stepping 代表敌人"从头顶往下一点点"的位置，作为判断玩家是否站在敌人头上的参考点
                        var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

                        // 避免玩家从敌人上方踩踏时，被敌人错误判断为接触攻击
                        if (!player.IsPointUnderStep(stepping))
                        {
                            // 如果开启击退效果
                            if (stats.current.contactPushback)
                            {
                                LateralVelocity = -transform.forward * stats.current.contactPushBackForce;
                            }

                            // 对玩家造成伤害
                            player.ApplyDamage(stats.current.contactDamage, transform.position);
                            enemyEvents.OnPlayerContact?.Invoke(); // 触发接触事件
                        }
                    }
                }
            }
        }

        protected virtual void HandleSight()
        {
            if (!player)
            {
                var overlaps = Physics.OverlapSphereNonAlloc(
                                position, stats.current.spotRange, m_sightOverlaps);
                for (int i = 0; i < overlaps; i++)
                {
                    // 如果是玩家
                    if (m_sightOverlaps[i].CompareTag(GameTags.Player))
                    {
                        if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
                        {
                            this.player = player;
                            enemyEvents.OnPlayerSpotted?.Invoke();
                            return;
                        }
                    }
                }
            }
            else
            {
                var distance = Vector3.Distance(position, player.position);

                // 如果玩家死亡或超出视野范围
                if ((player.health.current == 0) || (distance > stats.current.viewRange))
                {
                    player = null;
                    enemyEvents.OnPlayerSpotted?.Invoke(); // 触发玩家逃脱事件
                }
            }
        }

        /// <summary>
        /// 对敌人造成伤害，并根据血量触发相应状态
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="origin"></param>
        public override void ApplyDamage(int amount, Vector3 origin)
        {
            if(!health.isEmpty && !health.recovering)
            {
                health.Damage(amount);
                enemyEvents.OnDamage?.Invoke();

                if (health.isEmpty)
                {
                    controller.enabled = false;
                    enemyEvents.OnDie?.Invoke();
                }
            }
        }

        public virtual void Friction() => Decelerate(stats.current.friction);

        public virtual void Accelerate(Vector3 direction,float acceleration,float topSpeed) =>
            Accelerate(direction,stats.current.turningDrag, acceleration,topSpeed);

        public virtual void Decelerate() => Decelerate(stats.current.deceleration);

        public virtual void FaceDirectionSmooth(Vector3 direction) => 
            FaceDirection(direction,stats.current.rotationSpeed);

        public virtual void Gravity() => Gravity(stats.current.gravity);

        public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
    }
}
