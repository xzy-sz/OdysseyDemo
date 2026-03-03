using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    [RequireComponent(typeof(Collider))]
    public class Sign : MonoBehaviour
    {
        [Header("Sign Settings")]
        [TextArea(15,20)]
        public string text = "Hello World";     // 告失牌上要显示的文字
        public float viewAngle = 90f;           // 玩家能看到告示牌的最大可视角度

        [Header("Canvas Settings")]
        public Canvas canvas;                   // UI 画布，用来展示告示文字
        public Text uiText;                     // UI 文本组件，用来显示 text
        public float scaleDuration = 0.25f;     // 告示牌 UI 缩放动画的持续时间

        [Space(15)]
        public UnityEvent onShow;       // 告示牌显示触发的事件
        public UnityEvent onHide;       // 告示牌隐藏时触发的事件

        // 运动时使用的内部变量
        protected Vector3 m_initialSacle;       // UI 初始缩放值
        protected bool m_showing;               // 当前是否正在显示告示牌
        protected Collider m_collider;          // 当前物体的碰撞器
        protected Camera m_camera;              // 主相机

        /// <summary>
        /// 玩家离开触发区域时隐藏告示牌
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                Hide();
            }
        }

        /// <summary>
        /// 玩家在触发范围内时，检测是否满足显示条件
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(GameTags.Player))
            {
                // 玩家 -> 告示牌的方向
                var direction = (other.transform.position - transform.position).normalized;
                // 玩家和告示牌正面的角度差
                var angle = Vector3.Angle(transform.forward, direction);
                // 玩家高度是否大于告示牌底部（避免蹲下或在地板下触发）
                var allowedHeight = other.transform.position.y > m_collider.bounds.min.y;
                // 相机是否在告示牌的前方(Dot < 0 表示面对着告示牌)
                var inCameraSight = Vector3.Dot(m_camera.transform.forward, transform.forward) < 0;

                //  满足角度，位置和相机朝向条件时 -> 显示
                if(angle < viewAngle && allowedHeight && inCameraSight)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        /// <summary>
        /// 隐藏告示牌
        /// </summary>
        public virtual void Hide()
        {
            if (m_showing)
            {
                m_showing = false;
                onHide?.Invoke();   // 触发隐藏事件
                StopAllCoroutines();
                // 执行缩放动画：从当前缩放到 0 
                StartCoroutine(Scale(canvas.transform.localScale, Vector3.zero));
            }
        }

        /// <summary>
        /// 显示告示牌
        /// </summary>
        public virtual void Show()
        {
            if (!m_showing)
            {
                m_showing = true;
                onShow?.Invoke();
                StopAllCoroutines();
                // 执行缩放动画：从 0 缩放到初始大小
                StartCoroutine(Scale(Vector3.zero, m_initialSacle));
            }
        }

        /// <summary>
        /// 缩放动画协程
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected virtual IEnumerator Scale(Vector3 from, Vector3 to)
        {
            var elapsedTime = 0f;
            var scale = canvas.transform.localScale;

            // 在 scaleDuration 时间内，逐渐插值缩放
            while(elapsedTime < scaleDuration)
            {
                scale = Vector3.Lerp(from, to, (elapsedTime / scaleDuration));
                canvas.transform.localScale = scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // 确保最终缩放到目标值
            canvas.transform.localScale = to;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Awake()
        {
            uiText.text = text;           // 设置 UI 显示文字
            m_initialSacle = canvas.transform.localScale;   // 记录初始缩放
            canvas.transform.localScale = Vector3.zero;     // 开始时先隐藏
            canvas.gameObject.SetActive(true);              // 确保画布处于激活状态
            m_collider = GetComponent<Collider>();          // 获取自身碰撞体
            m_camera = Camera.main;                         // 获取主相机
        }
    }
}
