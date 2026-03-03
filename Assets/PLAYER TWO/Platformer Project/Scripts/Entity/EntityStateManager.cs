using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    /// <summary>
    /// 抽象基类，用于管理实体状态机，带有事件支持
    /// </summary>
    public abstract class EntityStateManager : MonoBehaviour
    {
        public EntityStateManagerEvents events;
    }
    /// <summary>
    /// 泛型抽象类，基础EntityStateManager,管理特定实体类型T的状态机。
    /// </summary>
    /// <typeparam name="T">实体类型，继承自Entity-T-</typeparam>
    public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T> 
    {
        #region 成员变量
        //------------------------------------------------------------------------------

        /// <summary>
        /// 持有所有状态实例的列表，顺序定义状态管理器的状态顺序。
        /// </summary>
        protected List<EntityState<T>> m_list = new List<EntityState<T>>();

        /// <summary>
        /// 状态字典，键为状态类型，值为对应状态实例，方便快速查找。
        /// </summary>
        protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

        public EntityState<T> current { get; protected set; }

        public EntityState<T> last { get; protected set; }

        /// <summary>
        /// 当前状态在状态列表中的索引位置
        /// </summary>
        public int index => m_list.IndexOf(current);

        /// <summary>
        /// 上一个状态在状态列表中的索引位置。
        /// </summary>
        public int lastIndex => m_list.IndexOf(last);

        public T entity { get; protected set; }

        //-------------------------------------------------------------------------------------
        #endregion


        #region 虚方法
        //--------------------------------------------------------------------------------------------------

        public virtual void Start()
        {
            InitializeEntity();
            InitializeStates();
        }

        public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);

        public virtual bool IsCurrentOfType(Type type) => current != null && current.GetType() == type;

        /// <summary>
        /// 默认实现为从当前GameObject 获取实体组件
        /// </summary>
        protected virtual void InitializeEntity() => entity = GetComponent<T>();

        /// <summary>
        /// 初始化状态列表和状态字典
        /// 会调用GetState()获取状态列表，斌加入字典以便快速查找。
        /// 同时默认将current设为状态列表的第一个状态（如果存在）。
        /// </summary>
        public virtual void InitializeStates()
        {
            m_list = GetStateList();

            foreach (var state in m_list)
            {
                var type = state.GetType();
                if (!m_states.ContainsKey(type)) m_states.Add(type, state);
            }

            if (m_list.Count > 0)
            {
                current = m_list[0];
            }

        }

        /// <summary>
        /// 每帧调用，用于更新当前状态的逻辑
        /// </summary>
        public virtual void Step()
        {
            if (current != null && Time.timeScale > 0) 
            { 
                current.Step(entity);
            }
        }

        /// <summary>
        /// 根据状态类型泛型参数切换状态。
        /// </summary>
        /// <typeparam name="TState">目标状态类型，必须继承自EntityState-T-</typeparam>
        public virtual void Change<TState>() where TState : EntityState<T>
        {
            var type = typeof(TState);
            if (m_states.ContainsKey(type))
            {
                Change(m_states[type]);
            }
        }

        /// <summary>
        /// 根据状态列表索引切换当前状态
        /// </summary>
        /// <param name="to"></param>
        public virtual void Change(int to)
        {
            if(to >= 0 && to < m_list.Count)
            {
                Change(m_list[to]);
            }
        }

        /// <summary>
        /// 根据状态实例切换当前状态。
        /// 执行状态的退出与进入回调，并触发相关事件。
        /// </summary>
        /// <param name="to">目标状态实例</param>
        public virtual void Change(EntityState<T> to)
        {
            if(to != null && Time.timeScale > 0)
            {
                if(current != null)
                {
                    current.Exit(entity);
                    events.onExit.Invoke(current.GetType());
                    last = current;
                }
                current = to;
                current.Enter(entity);
                events.onEnter.Invoke(current.GetType());
                events.onChange?.Invoke();
            }
        }

        /// <summary>
        /// 当实体与其他碰撞体接触时调用，将使劲啊传递给当前状态
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnContact(Collider other)
        {
            if(current != null && Time.timeScale > 0)
            {
                current.OnContact(entity, other);
            }
        }

        //------------------------------------------------------------------------------------------------
        #endregion


        #region 抽象方法
        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 获取状态实例列表
        /// </summary>
        /// <returns>状态实例列表</returns>
        protected abstract List<EntityState<T>> GetStateList();

        //-----------------------------------------------------------------------------------------------------------
        #endregion

    }
}
