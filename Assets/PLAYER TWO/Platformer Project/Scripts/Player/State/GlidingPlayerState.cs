using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State
{
    public class GlidingPlayerState : PlayerState
    {
        public override void OnContact(Player player, Collider other)
        {
            player.WallDrag(other);
            player.GrabPole(other);
        }

        protected override void OnEnter(Player player)
        {
            player.VerticalVelocity = Vector3.zero; // 清空垂直速度
            player.playerEvents.OnGlidingStart.Invoke();  // 调用事件：滑翔开始
        }

        protected override void OnExit(Player player)
        {
            player.playerEvents.OnGlidingStop.Invoke(); // 调用事件：滑翔结束
        }

        protected override void OnStep(Player player)
        {
            // 获取输入方向(相对于摄像机)
            var InputDirection = player.inputs.GetMovementCameraDirection();

            // 处理滑翔吃重力，是下落速度受限
            HandleGlidingGravity(player);

            // 角色面向水平移动方向
            player.FaceDirection(player.LateralVelocity);

            // 水平加速
            player.Accelerate(
                InputDirection,
                player.stats.current.glidingTurningDrag,
                player.stats.current.airAcceleration,
                player.stats.current.topSpeed);

            // 尝试抓取悬挂物(如悬崖或杆)
            player.LedgeGrab();

            // 如果落地 -> 切换到下落状态
            if (player.isGrounded)
            {
                player.states.Change<IdlePlayerState>();
            }
            else if(!player.inputs.GetGlide()) // 如果松开滑翔键 -> 切换到下落状态
            {
                player.states.Change<FallPlayerState>();
            }
        }

        /// <summary>
        /// 处理滑翔重力
        /// - 角色在空中缓慢下落
        /// - 下落速度不会超过 glidingMaxFallSpeed
        /// </summary>
        /// <param name="player"></param>
        protected virtual void HandleGlidingGravity(Player player)
        {
            var yVelocity = player.VerticalVelocity.y;
            // 按照画像重力计算速度
            yVelocity -= player.stats.current.glidingGravity * Time.deltaTime;
            // 限制最大下落速度
            yVelocity = Mathf.Max(yVelocity, -player.stats.current.glidingMaxFallSpeed);
            // 更新垂直速度
            player.VerticalVelocity = new Vector3(0, yVelocity, 0);
        }
    }
}
