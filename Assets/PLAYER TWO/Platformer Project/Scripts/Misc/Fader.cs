using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("PLAYER TWO/Platfoemer Project/Misc/Fader")]
    public class Fader : Singleton<Fader>
    {
        public float speed = 1f;
        protected Image m_image;

        /// <summary>
        /// 渐隐，没有回调函数
        /// </summary>
        public void FadeOut() => FadeOut(() => { });

        /// <summary>
        /// 渐显，没有回调函数
        /// </summary>
        public void FadeIn() => FadeIn(() => { });

        public void FadeOut(Action onFinished)
        {
            // 先停止所有正在运行的协程，防止多多个渐变冲突
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine(onFinished));
        }

        public void FadeIn(Action onFinished)
        {
            // 先停止所有正在运行的协程，防止多多个渐变冲突
            StopAllCoroutines();
            StartCoroutine(FadeInRoutine(onFinished));
        }

        public virtual void SetAlpha(float alpha)
        {
            var color = m_image.color;
            color.a = alpha;
            m_image.color = color;
        }

        public virtual IEnumerator FadeOutRoutine(Action onFinished)
        {
            // 循环执行
            while(m_image.color.a < 1)
            {
                var color = m_image.color;
                color.a += speed * Time.deltaTime;
                m_image.color = color;
                yield return null;
            }
            onFinished?.Invoke();
        }

        /// <summary>
        /// 协程：逐渐减小透明度直到完全透明，然后调用回调
        /// </summary>
        /// <param name="onFinished"></param>
        /// <returns></returns>
        protected virtual IEnumerator FadeInRoutine(Action onFinished)
        {
            // 循环执行，知道 alpha <= 0
            while(m_image.color.a > 0)
            {
                var color = m_image.color;
                // 每帧根据速度和时间减小 alpha
                color.a -= speed * Time.deltaTime;
                m_image.color = color;
                // 等待下一帧继续
                yield return null;
            }
            // 执行回调
            onFinished?.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();
            m_image = GetComponent<Image>();
        }
    }
}
