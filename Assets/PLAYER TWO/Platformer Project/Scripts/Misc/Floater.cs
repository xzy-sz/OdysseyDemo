using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Floater")]
    public class Floater : MonoBehaviour
    {
        // 浮动的速度
        public float speed = 2f;

        // 浮动的振幅
        public float amplitude = 0.5f;

        protected virtual void LateUpdate()
        {
            // 计算一个随时间变化的正弦波值
            // Time.rime * speed 控制频率
            //再乘上 amplitude 可控制振幅
            var wave = Mathf.Sin(Time.time * speed) * amplitude;

            // 根据物体自身的up方向，叠加一个位移
            // wave * Time.deltaTime 保证每帧移动平滑
            transform.position += transform.up * wave * Time.deltaTime;
        }
    }
}
