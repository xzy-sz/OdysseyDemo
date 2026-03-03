using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.UI
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Level Card")]
    public class UILevelCard : MonoBehaviour
    {
        [Header("UI 元素")]
        public Text title;  // 观关卡标题文本
        public Text description;    // 关卡描述文本
        public Text coins;          // 关卡收集金币数量文本
        public Text time;           // 关卡完成时间文本
        public Image image;         // 关卡图片
        public Button play;         // 开始关卡按钮
        public Image[] starsImages; // 星星图标数组

        // 内部锁定状态
        protected bool m_locked;

        // 当前关卡场景名
        public string scene { get;set; }

        /// <summary>
        /// 初始化
        /// 给 play 按钮绑定点击事件
        /// </summary>
        protected virtual void Start()
        {
            play.onClick.AddListener(Play);
        }

        /// <summary>
        /// 点击 Play 按钮时调用，加载关卡场景
        /// </summary>
        public virtual void Play()
        {
            GameLoader.Instance.Load(scene);    // 调用游戏加载器加载指定场景
        }

        /// <summary>
        /// 是否锁定关卡
        /// 设置时会同时控制 play 按钮是否可交互
        /// </summary>
        public bool locked
        {
            get { return m_locked; }
            set
            {
                m_locked = value;
                play.interactable = !m_locked;  // 如果锁定则按钮不可点击
            }
        }
        /// <summary>
        /// 使用关卡数据填充 UI 卡片
        /// </summary>
        /// <param name="level"></param>
        public virtual void Fill(GameLevel level)
        {
            if(level !=  null)
            {
                // 设置锁定状态和场景名
                locked = level.locked;
                scene = level.scene;

                // 更新 UI 文本和图片
                title.text = level.name;
                description.text = level.description;
                time.text = GameLevel.FormattedTime(level.time);
                coins.text = level.coins.ToString("000");
                image.sprite = level.image;

                // 根据关卡星星数据显示或隐藏星星图标
                for(int i = 0;i < starsImages.Length;i++)
                {
                    starsImages[i].enabled = level.stars[i];
                }
            }
        }
    }
}
