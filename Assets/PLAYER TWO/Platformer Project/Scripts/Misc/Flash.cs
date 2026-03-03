using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("PLAYER TWO/Platformer Project/Mics/Flash")]
    public class Flash : Singleton<Flash>
    {
        // 闪屏效果维持的时间
        public float duration = 0.1f;
        // 渐隐消失的时间
        public float fadeDuration = 0.5f;

        // 绑定的 UI Image
        protected Image image;

        public void Trigger() => Trigger(duration, fadeDuration);

        /// <summary>
        /// 触发闪屏效果
        /// </summary>
        /// <param name="duration">闪屏效果的全亮维持时间</param>
        /// <param name="fadeDuration">渐隐消失时间</param>
        public void Trigger(float duration,float fadeDuration)
        {
            // 防止多个协程叠加，先停止所有协程
            StopAllCoroutines();

            // 启动闪屏效果的协程
            StartCoroutine(Routine(duration, fadeDuration));
        }

        protected IEnumerator Routine(float duration,float fadeDuration)
        {
            var elapsedTime = 0f;       // 记录经过的时间
            var color = image.color;    // 获取当前颜色

            // 1.设置 alpha = 1 (完全不透明)
            color.a = 1;
            image.color = color;

            // 2.等待全亮持续时间
            yield return new WaitForSeconds(duration);

            // 3.渐隐：alpha 从 1 逐渐插值到 0
            while(elapsedTime < fadeDuration)
            {
                color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                image.color = color;

                yield return null;  // 等待下一帧
            }

            // 4.最后确保完全透明
            color.a = 0;
            image.color = color;
        }

        /// <summary>
        /// Unity 生命周期
        /// </summary>
        protected virtual void Start()
        {
            image = GetComponent<Image>();
        }
    }
}
