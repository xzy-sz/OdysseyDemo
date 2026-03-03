using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys.States
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Follow Enemy State")]
    public class FollowEnemyState : EnemyState
    {
        public override void OnContact(Enemy enemy, Collider other)
        {
            
        }

        protected override void OnEnter(Enemy enemy)
        {
            
        }

        protected override void OnExit(Enemy enemy)
        {
            
        }

        protected override void OnStep(Enemy enemy)
        {
            enemy.Gravity();
            enemy.SnapToGround();

            // 计算敌人到玩家头部的方向向量
            var head = enemy.player.position - enemy.position;
            // 只保留水平方向，忽略y轴，并单位化方向向量
            var direction = new Vector3(head.x, 0, head.z).normalized;
            //让敌人朝玩家方向加速
            // followAcceleration:跟随加速度
            // followToSpeed:跟随是的最大速度
            enemy.Accelerate(
                direction,
                enemy.stats.current.followAcceleration,
                enemy.stats.current.followTopSpeed);
            // 平滑的旋转敌人朝向玩家的方向
            enemy.FaceDirectionSmooth(direction);


        }
    }
}
