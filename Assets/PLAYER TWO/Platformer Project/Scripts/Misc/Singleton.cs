using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// 静态实例对象，用于全局访问
        /// </summary>
        protected static T m_instance;

        /// <summary>
        /// 公有静态属性，外部通过 Singleton.Instance 获取单例对象
        /// 如果当前还没有实例，会自动在场景中查找一个
        /// </summary>
        public static T Instance 
        {
            get 
            {
                // 如果实例为空，则在场景中查找 T 类型对象
                if(m_instance == null)
                {
                    m_instance = FindAnyObjectByType<T>();
                }
                return m_instance;
            } 
        }

        /// <summary>
        /// Awake 生命周期方法，确保单例唯一性
        /// 如果对象不是单例实例，则销毁自己，避免重复存在
        /// </summary>
        protected virtual void Awake()
        {
            if(Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
