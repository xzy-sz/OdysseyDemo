using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys
{
    public class EnemyStats : EntityStats<EnemyStats>
    {
        [Header("General Stats")]
        public float gravity = 35f;             // 重力值
        public float snapForce = 15f;           // 吸附力
        public float rotationSpeed = 970f;      // 旋转速度
        public float deceleration = 28f;        // 减速度
        public float friction = 16f;            // 摩擦力
        public float turningDrag = 28f;         // 转向阻力

        [Header("Follow Stats")]
        public float followAcceleration = 10f;  // 追踪时的加速度
        public float followTopSpeed = 4;        // 追踪时的最好移动速度

        [Header("Waypoint Stats")]
        public bool faceWaypoint = true;            // 是否朝向当前路径点旋转
        public float waypointMinDistance = 0.5f;    // 距离路径点多近时算到达，切换下一个点
        public float waypointAcceleration = 10f;    // 巡逻时的加速度
        public float waypointTopSpeed = 2f;         // 巡逻时的最高移动速度

        [Header("View Stats")]
        public float spotRange = 5f;        // 发现玩家的视野范围
        public float viewRange = 8f;        // 敌人追踪的最大视野范围

        [Header("Contact Attack Stats")]
        public bool canAttackOnContact = true;          // 是否允许敌人通过接触攻击玩家
        public bool contactPushback = true;             // 接触攻击时是否施加击退效果
        public float contactOffset = 0.15f;             // 接触检测的偏移量，检测攻击范围的大小
        public int contactDamage = 1;                   // 接触攻击造成的伤害值
        public float contactPushBackForce = 18f;        // 击退时施加的推力大小
        public float contactSteppingTolerance = 0.1f;   // 接触攻击中容忍玩家踩踏的距离
    }
}
