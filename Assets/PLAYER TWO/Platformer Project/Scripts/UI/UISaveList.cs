using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.UI
{
    [AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save List")]
    public class UISaveList : MonoBehaviour
    {
        [Header("选中首个元素")]
        public bool focusFirstElement = true;   // 是否在启动时自动选中第一个存档槽

        [Header("存档卡片与容器")]
        public UISaveCard card;             // 存档卡片的预制体
        public RectTransform container;     // 存放存档卡片的容器

        // 内部列表，用于存储生成的存档卡片
        protected List<UISaveCard> m_cardList = new List<UISaveCard>();

        protected virtual void Awake()
        {
            // 从存档系统获取所有存档数据
            var data = GameSaver.Instance.LoadList();

            // 根据存档数据生成对应的 UI 卡片
            for(int i = 0;i < data.Length;i++)
            {
                m_cardList.Add(Instantiate(this.card, container));  // 实例化卡片
                m_cardList[i].Fill(i,data[i]);                      // 填充卡片数据
            }

            // 自动选中第一个存档槽
            if (focusFirstElement)
            {
                if (m_cardList[0].isFilled)
                {
                    // 第一行存档有数据，选中加载按钮
                    EventSystem.current.SetSelectedGameObject(m_cardList[0].loadButton.gameObject); 
                }
                else
                {
                    // 第一个存档为空，选中新建按钮
                    EventSystem.current.SetSelectedGameObject(m_cardList[0].newGameButton.gameObject);
                }
            }

        }
    }
}
