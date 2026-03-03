using System;
using UnityEngine.Events;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    /// <summary>
    /// 用于管理实体状态机事件的序列化类
    /// 通过该类可以在Inspector中绑定和触发状态的相关事件
    /// </summary>
    [Serializable]
    public class EntityStateManagerEvents
    {
        /// <summary>
        /// 当前状态发生切换时触发的事件。
        /// </summary>
        public UnityEvent onChange;
        
        /// <summary>
        /// 当前进入某个状态时触发的事件。
        /// 传递被进入状态的类型信息（Type），方便外部根据状态类型做不同的处理。
        /// </summary>
        public UnityEvent<Type> onEnter;

        /// <summary>
        /// 当前退出某状态时触发的事件。
        /// 传递退出状态的类型信息（Type）,方便外部根据状态类型做不同的处理。
        /// </summary>
        public UnityEvent<Type> onExit;
    }
}
