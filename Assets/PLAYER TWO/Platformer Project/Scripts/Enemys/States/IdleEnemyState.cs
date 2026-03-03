using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Enemys.States
{
    public class IdleEnemyState : EnemyState
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
            enemy.Friction();
        }
    }
}
