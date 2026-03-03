using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider),typeof(AudioSource))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Portal")]
    public class Portal : MonoBehaviour
    {
        // 是否使用闪光效果
        public bool useFlash = true;
        // 出口传送门
        public Portal exit;
        // 出口的偏移量
        public float exitOffset = 1f;
        // 传送时播放的音效
        public AudioClip teleportClip;

        protected Collider m_collider;
        protected AudioSource m_audio;
        protected PlayerCamera m_camera;

        // 当前传送门位置
        public Vector3 position => transform.position;

        // 当前传送门朝向
        public Vector3 forward => transform.forward;

        // 初始化
        protected virtual void Start()
        {
            m_collider = GetComponent<Collider>();
            m_audio = GetComponent<AudioSource>();
            m_camera = FindFirstObjectByType<PlayerCamera>();
            // 设置碰撞器为触发器
            m_collider.isTrigger = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // 确认出口存在，并且进入的是玩家
            if(exit && other.TryGetComponent(out Player player))
            {
                // 计算玩家与当前传送门的高度差
                var yOffset = player.unsizedPosition.y - transform.position.y;

                // 设置玩家位置到出口处，并保持高度差
                player.transform.position = exit.position + Vector3.up * yOffset;
                // 让玩家朝向出口的方向
                player.FaceDirection(exit.forward);
                // 重置相机
                m_camera.Reset();

                // 获取玩家输入的相机方向
                var inputDirection = player.inputs.GetMovementCameraDirection();

                // 如果输入方向与出口方向相反，则让玩家转身
                if(Vector3.Dot(inputDirection,exit.forward) < 0)
                {
                    player.FaceDirection(-exit.forward);
                }

                // 在出口方向上增加一个偏移量，避免卡住
                player.transform.position += player.transform.forward * exit.exitOffset;
                // 让玩家保持原来的速度大小，但方向改为出口方向
                player.LateralVelocity = player.transform.forward * player.LateralVelocity.magnitude;

                // 如果启用了闪光特效，则触发
                if (useFlash)
                {
                    Flash.Instance?.Trigger();
                }
                // 播放传送音效
                m_audio.PlayOneShot(teleportClip);
            }
        }
    }
}
