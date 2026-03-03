using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Die Player State")]
    public class DiePlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
           
        }

        protected override void OnEnter(Player player)
        {
           
        }

        protected override void OnExit(Player player)
        {
            
        }

        protected override void OnStep(Player player)
        {
            player.Gravity();       // 受到重力影响
            player.Friction();      // 受到摩擦力影响
            player.SnapToGround();  // 紧贴地面
        }
    }
}
